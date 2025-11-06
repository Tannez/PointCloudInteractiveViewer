using UnityEngine;
using LLMUnity;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
public class PCFunctions : UIInstanceController
{
    // Lazy Loading + Singleton pattern to access and utalise the UI Controller methods withing the LLM Function Caller
    private static UIInstanceController _uIInstanceController;

    private static UIInstanceController uIInstanceController
    {
        get
        {
            // If already cached and still valid, return it
            if (_uIInstanceController != null)
                return _uIInstanceController;

            // Otherwise, find it in the scene and cache it
            _uIInstanceController = Object.FindFirstObjectByType<UIInstanceController>();

            if (_uIInstanceController == null)
                Debug.LogWarning("PointCloudController not found in scene!");

            return _uIInstanceController;
        }
    }

    // Test Methods providing randomly generated results within string reply
    // static System.Random random = new System.Random();
    // public static string PointBudget()
    // {
    //     string[] pointBudget = new string[]{"200.000", "400.000", "750.000", "1.000.000"};
    //     return "The Point Budget of the Point Cloud is " + pointBudget[random.Next(pointBudget.Length)];
    // }
    // public static string PCClassCount()
    // {
    //     return "The Point Cloud contains: " + random.Next(5).ToString("D2") + " Classes, with: " + random.Next(10).ToString("D2") + " Total Instances";
    // }
    // public static string PCColorMode()
    // {
    //     string[] pCColorsMode = new string[] { "RGBA", "Classification", "Intensity" };
    //     return "Current color mode is set to: " + pCColorsMode[random.Next(pCColorsMode.Length)];
    // }

    // Methods with Access to the UI Controller 
    public static string HideTerrain()
    {
        uIInstanceController.classToggles[0].isOn = false;
        return "The Terrain Point Cloud Class has been hidden";
    }
    public static string ShowTerrain()
    {
        uIInstanceController.classToggles[0].isOn = true;
        return "The Terrain Point Cloud Class Is Visible Again";
    }

    public static string FocusOnTech()
    {
        uIInstanceController.ShowClassInstanceUI(4);
        if (uIInstanceController.classSelected[3] == true)
        {
            return "Prioritising the Tech Clouds. Class contains: " + uIInstanceController.availableInstancesInClass + " instances";
        }
        else if (uIInstanceController.classSelected[3] == false)
        {
            return "Unprioritising the Tech Clouds. Instance menu has been hidden, and cloud class point sizes have been reset";
        }
        return "";
    }


}
public class PointCloudAssistant : MonoBehaviour
{
    public LLMCharacter llmCharacter;
    public InputField playerText;
    public Text AIText;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerText.onSubmit.AddListener(onInputFieldSubmit);
        playerText.Select();
        llmCharacter.grammarString = MultipleChoiceGrammar();
    }

    // Get the available functions within the Functions class
    string[] GetFunctionNames()
    {
        List<string> functionNames = new List<string>();
        foreach (var function in typeof(PCFunctions).GetMethods(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)) functionNames.Add(function.Name);
        return functionNames.ToArray();
    }

    // Specfify grammar setup for the LLM (This one is set up for multiple choice responses)
    string MultipleChoiceGrammar()
    {
        return "root ::= (\"" + string.Join("\" | \"", GetFunctionNames()) + "\")";
    }

    // The prompt the LLM returns to the user
    string ConstructPrompt(string message)
    {
        string prompt = "Which of the following choices matches best the input?\n\n";
        prompt += "Input:" + message + "\n\n";
        prompt += "Choices:\n";
        foreach (string functionName in GetFunctionNames()) prompt += $"- {functionName}\n";
        prompt += "\nAnswer directly with the choice";
        return prompt;
    }

    // Function within the function class which the LLM will try and call
    string CallFunction(string functionName)
    {
        return (string)typeof(PCFunctions).GetMethod(functionName).Invoke(null, null);
    }

    // Method for when prompt has been submitted
    async void onInputFieldSubmit(string message)
    {
        playerText.interactable = false;
        string functionName = await llmCharacter.Chat(ConstructPrompt(message));
        string result = CallFunction(functionName);
        AIText.text = $"Calling {functionName}\n{result}";
        playerText.interactable = true;
    }
    
    public void CancelRequests()
    {
        llmCharacter.CancelRequests();
    }

    public void ExitGame()
    {
        Debug.Log("Exit button clicked");
        Application.Quit();
    }

    // Ensure that an LLM Model is selected
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
