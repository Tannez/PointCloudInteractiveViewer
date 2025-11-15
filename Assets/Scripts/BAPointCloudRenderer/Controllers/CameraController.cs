using JetBrains.Annotations;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BAPointCloudRenderer.Controllers {
    /*
     * CameraController for flying-controls
     */
    public class CameraController : MonoBehaviour {

        [SerializeField] UIInstanceController uIInstanceController; // Use this if not LLM scene 
        [SerializeField] CloudControllerLLM cloudControllerLLM; // Use this if LLM scene 
        public bool MouseClickOnScene = true;

        private bool testCamPosTrans = true;

        //Current yaw
        private float yaw = 0.0f;
        //Current pitch
        private float pitch = 0.0f;

        public float normalSpeed = 10;

        [Header("Reset Camera Position")]
        [SerializeField] private Vector3 setResetPosition;
        [SerializeField] private Vector3 setResetAngle;

        [Header("Class Camera Positions")]
        // Class 1 Terrain
        [SerializeField] private Vector3 class1CamPosition = new Vector3(-1.18f, 5.32f, 11.12f);
        [SerializeField] private Vector2 class1CamOrientation = new Vector2(29.5f, 112.75f);

        // Class 2 Top
        [SerializeField] private Vector3 class2CamPosition = new Vector3(2.31f, -0.23f, 10.05f);
        [SerializeField] private Vector2 class2CamOrientation = new Vector2(0.5f, 136.0f);
        
        // Class 3 Walls
        [SerializeField] private Vector3 class3CamPosition = new Vector3(0.27f, -0.46f, 5.58f);
        [SerializeField] private Vector2 class3CamOrientation = new Vector2(12.5f, 65.75f);

        // Class 4 Tech
        [SerializeField] private Vector3 class4CamPosition = new Vector3(1.35f, -0.42f, 10.91f);
        [SerializeField] private Vector2 class4CamOrientation = new Vector2(18.25f, 137.75f);
        
        // Class 5 Bottom
        [SerializeField] private Vector3 class5CamPosition = new Vector3(2.58f, -5.65f, 10.07f);
        [SerializeField] private Vector2 class5CamOrientation = new Vector2(-40.5f, 137.75f);
        


        void Start() {
            //Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update()
        {
            // OLD KEY PRESS TO GET OUT OF CAMERA CONTROL
            // if (Input.GetKey(KeyCode.Escape) && MouseClickOnScene == true)
            // {
            //     MouseClickOnScene = false;
            //     Cursor.lockState = CursorLockMode.None;
            //     Cursor.visible = true;
            // }


            if (Input.GetMouseButtonDown(1))
            {
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    MouseClickOnScene = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    MouseClickOnScene = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    //cloudControllerLLM.keyboardShotcutsEnabled = true;
                }
            }

            // NEW MOUSE RELEASE TO GET OUT OF CAMERA CONTROL
            if (Input.GetMouseButtonUp(1) && MouseClickOnScene == true)
            {
                MouseClickOnScene = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            //if (uIInstanceController.LLMMenuActive == false && Input.GetKeyDown("space")) // Use this if not LLM scene 
            if (cloudControllerLLM.keyboardShotcutsEnabled == true && Input.GetKeyDown("space")) // Use this if LLM scene 
            {
                Camera.main.transform.position = setResetPosition;
                // Camera.main.transform.eulerAngles = new Vector3(20.75f, 53.5f, 0f);
                pitch = setResetAngle.x;
                yaw = setResetAngle.y;
            }     

            if (testCamPosTrans)
            {
                CameraClassTranslationTest();
            }
        }

        void FixedUpdate()
        {
            //React to controls. (WASD, EQ and Mouse)
            if (MouseClickOnScene)
            {
                float moveHorizontal = Input.GetAxis("Horizontal");
                float moveVertical = Input.GetAxis("Vertical");
                float moveUp = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

                float speed = normalSpeed;

                if (Input.GetKey(KeyCode.C))
                {
                    speed /= 10; ;
                }
                else if (Input.GetKey(KeyCode.LeftShift))
                {
                    speed *= 5;
                }

                transform.Translate(new Vector3(moveHorizontal * speed * Time.deltaTime, moveUp * speed * Time.deltaTime, moveVertical * speed * Time.deltaTime));

                yaw += 5 * Input.GetAxis("Mouse X");
                pitch -= 5 * Input.GetAxis("Mouse Y");

                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            }
        }

        public void CameraClassTranslationTest()
        {
            if (Input.GetKeyDown(KeyCode.Y))
            {
                Camera.main.transform.position = class1CamPosition;
                pitch = class1CamOrientation.x;
                yaw = class1CamOrientation.y;
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                Camera.main.transform.position = class2CamPosition;
                pitch = class2CamOrientation.x;
                yaw = class2CamOrientation.y;
            }
            if (Input.GetKeyDown(KeyCode.I))
            {
                Camera.main.transform.position = class3CamPosition;
                pitch = class3CamOrientation.x;
                yaw = class3CamOrientation.y;
            }
            if (Input.GetKeyDown(KeyCode.O))
            {
                Camera.main.transform.position = class4CamPosition;
                pitch = class4CamOrientation.x;
                yaw = class4CamOrientation.y;
            }
            if (Input.GetKeyDown(KeyCode.P))
            {
                Camera.main.transform.position = class5CamPosition;
                pitch = class5CamOrientation.x;
                yaw = class5CamOrientation.y;
            }
            
        }
    }


}

