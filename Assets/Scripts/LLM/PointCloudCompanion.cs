using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using LLMUnity;
using UnityEngine.UI;
using System.IO;

namespace LLMPCCompanionBubble
{
    public class PointCloudCompanion : MonoBehaviour
    {
        // CHATBOT COMPONENTS
        public Transform chatContainer;
        public Color playerColor = new Color32(81, 81, 164, 255);
        public Color aiColor = new Color32(29, 29, 29, 255);
        public Color fontColor = Color.white;
        public Font font;
        public int fontSize = 16;
        public int bubbleWidth = 600;
        public LLMCharacter llmCharacter;
        public float textPadding = 10f;
        public float bubbleSpacing = 10f;
        public Sprite sprite;
        public Button stopButton;

        // Reference to the cloud controller for function calling 
        [SerializeField] public CloudControllerLLM cloudControllerLLM;

        // Reference to bubble setup and user input in bubbles
        private LLMInputBubble inputBubble;
        private List<BubbleUICreate> chatBubbles = new List<BubbleUICreate>();
        private bool blockInput = true;
        private BubbleUI playerUI, aiUI;
        private bool warmUpDone = false;
        private int lastBubbleOutsideFOV = -1;

        // reference context
        private string staticContext;

        // Function Calling related variables
        private static FunctionHandler _functionHandler;
        private static FunctionHandler functionHandler
        {
            get
            {
                // If already cached and still valid, return it
                if (_functionHandler != null)
                    return _functionHandler;

                // Otherwise, find it in the scene and cache it
                _functionHandler = FindFirstObjectByType<FunctionHandler>();

                if (_functionHandler == null)
                    Debug.LogWarning("PointCloudController not found in scene!");

                return _functionHandler;
            }
        }

        void Start()
        {
            if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            playerUI = new BubbleUI
            {
                sprite = sprite,
                font = font,
                fontSize = fontSize,
                fontColor = fontColor,
                bubbleColor = playerColor,
                bottomPosition = 0,
                leftPosition = 0,
                textPadding = textPadding,
                bubbleOffset = bubbleSpacing,
                bubbleWidth = bubbleWidth,
                bubbleHeight = -1
            };
            aiUI = playerUI;
            aiUI.bubbleColor = aiColor;
            aiUI.leftPosition = 1;

            // Add context to the user prompt - only if file exists
            string contextPath = "Assets/Resources/PCRAG/PointCloudContext.md";
            if (File.Exists(contextPath))
            {
                staticContext = File.ReadAllText(contextPath);
            }

            inputBubble = new LLMInputBubble(chatContainer, playerUI, "InputBubble", "Loading...", 4);
            inputBubble.AddSubmitListener(onInputFieldSubmit);
            inputBubble.AddValueChangedListener(onValueChanged);
            inputBubble.setInteractable(false);
            stopButton.gameObject.SetActive(true);
            ShowLoadedMessages();
            _ = llmCharacter.Warmup(WarmUpCallback);

            string introduction = "The user has just opened the application. Please introduce yourself and your capabilities to the user. Also, inform the user that you might not interpret the first input correctly, but that they should just try the same statement again.";

            BubbleUICreate aiBubble = AddBubble("Loading Companion...", false);

            Task chatTask = llmCharacter.Chat(introduction, aiBubble.SetText, AllowInput);
        }

        BubbleUICreate AddBubble(string message, bool isPlayerMessage)
        {
            BubbleUICreate bubble = new BubbleUICreate(chatContainer, isPlayerMessage ? playerUI : aiUI, isPlayerMessage ? "PlayerBubble" : "AIBubble", message);
            chatBubbles.Add(bubble);
            bubble.OnResize(UpdateBubblePositions);
            return bubble;
        }

        void ShowLoadedMessages()
        {
            for (int i = 1; i < llmCharacter.chat.Count; i++) AddBubble(llmCharacter.chat[i].content, i % 2 == 1);
        }

        async void onInputFieldSubmit(string newText)
        {
            // When 'return' has been hit, and text is send, 
            // shutdown interaction in the input field
            inputBubble.ActivateInputField();
            if (blockInput || newText.Trim() == "" || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                StartCoroutine(BlockInteraction());
                return;
            }
            blockInput = true;

            // replace vertical_tab
            string message = inputBubble.GetText().Replace("\v", "\n");

            // add a bubble with the prompt send by the user in the input field
            AddBubble(message, true);

            // Create a temporary bubble to show that the LLM is loading its response
            BubbleUICreate aiBubble = AddBubble("thinking...", false);

            // Clear user input bubble
            inputBubble.SetText("");

            // Attempt to execute function 
            (bool, string) applyFunction = await functionHandler.TryExecuteCommand(message);

            if (applyFunction.Item1 == true)
            {
                // Test function call
                aiBubble.SetText(applyFunction.Item2 + " \nReady for next input.");
                AllowInput();

                // Tell LLM what function has been called - not working 
                // Send string to the LLM 
                // SendChatMessage("You have made the following decision: " + applyFunction.Item2 + ". Inform the user what manipulation you have done on the point cloud.");
                return;
            }

            // // if not executing, send message to LLM with context
            // string combinedPrompt = $"{staticContext}\n\nUser: {message}";
            // // Send combined string to the LLM + run async to ensure Unity waits for the response instead of spawning orphaned background tasks.
            // await llmCharacter.Chat(combinedPrompt, aiBubble.SetText, AllowInput);

            // Send string to the LLM 
            Task chatTask = llmCharacter.Chat(message, aiBubble.SetText, AllowInput);
        }
        
        public void WarmUpCallback()
        {
            warmUpDone = true;
            inputBubble.SetPlaceHolderText("Message me");
            AllowInput();
        }

        public void AllowInput()
        {
            blockInput = false;
            inputBubble.ReActivateInputField();
        }     

        public void CancelRequests()
        {
            llmCharacter.CancelRequests();
            AllowInput();
        }

        IEnumerator<string> BlockInteraction()
        {
            // prevent from change until next frame
            inputBubble.setInteractable(false);
            yield return null;
            inputBubble.setInteractable(true);
            // change the caret position to the end of the text
            inputBubble.MoveTextEnd();
        }

        void onValueChanged(string newText)
        {
            // Get rid of newline character added when we press enter
            if (Input.GetKey(KeyCode.Return))
            {
                if (inputBubble.GetText().Trim() == "")
                    inputBubble.SetText("");
            }
        }

        public void UpdateBubblePositions()
        {
            float y = inputBubble.GetSize().y + inputBubble.GetRectTransform().offsetMin.y + bubbleSpacing;
            float containerHeight = chatContainer.GetComponent<RectTransform>().rect.height;
            for (int i = chatBubbles.Count - 1; i >= 0; i--)
            {
                BubbleUICreate bubble = chatBubbles[i];
                RectTransform childRect = bubble.GetRectTransform();
                childRect.anchoredPosition3D = new Vector3(childRect.anchoredPosition3D.x, y, 0);

                // last bubble outside the container
                if (y > containerHeight && lastBubbleOutsideFOV == -1)
                {
                    lastBubbleOutsideFOV = i;
                }
                y += bubble.GetSize().y + bubbleSpacing;
            }
        }

        void Update()
        {
            if (cloudControllerLLM.keyboardShotcutsEnabled == true)
            {
                inputBubble.setInteractable(false);
            }
            else if (cloudControllerLLM.keyboardShotcutsEnabled == false)
            {
                if (!inputBubble.inputFocused() && warmUpDone)
                {
                    inputBubble.ActivateInputField();
                    StartCoroutine(BlockInteraction());
                }
            
                if (lastBubbleOutsideFOV != -1)
                {
                    // destroy bubbles outside the container
                    for (int i = 0; i <= lastBubbleOutsideFOV; i++)
                    {
                        chatBubbles[i].Destroy();
                    }
                    chatBubbles.RemoveRange(0, lastBubbleOutsideFOV + 1);
                    lastBubbleOutsideFOV = -1;
                }
            }
        }

        // Method for mouse click selection to inform llm of user interaction
        public void SendChatMessage(string clickMessage)
        {
            // shutdown interaction in the input field
            if (blockInput || clickMessage.Trim() == "" || Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            {
                StartCoroutine(BlockInteraction());
                return;
            }
            blockInput = true;

            // Create a temporary bubble to show that the LLM is loading its response
            BubbleUICreate aiBubble = AddBubble("thinking...", false);

            // replace vertical_tab
            string message = clickMessage.Replace("\v", "\n");

            // Send string to the LLM 
            Task chatTask = llmCharacter.Chat(message, aiBubble.SetText, AllowInput);
        }

        public void ExitGame()
        {
            Debug.Log("Exit button clicked");
            Application.Quit();
        }

        bool onValidateWarning = true;
        void OnValidate()
        {
            if (onValidateWarning && !llmCharacter.remote && llmCharacter.llm != null && llmCharacter.llm.model == "")
            {
                Debug.LogWarning($"Please select a model in the {llmCharacter.llm.gameObject.name} GameObject!");
                onValidateWarning = false;
            }
        }
    }
}

