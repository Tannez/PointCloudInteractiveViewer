using JetBrains.Annotations;
using NUnit.Framework.Internal;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BAPointCloudRenderer.Controllers {
    /*
     * CameraController for flying-controls
     */
    public class CameraController : MonoBehaviour {

        [SerializeField] PointCloudControls pointCloudControls; // Final Version 
        public bool MouseClickOnScene = true;
        public bool withLLM = true;

        // Current Position
        private Vector3 currentCamPosition;
        private bool movingCamera = false;
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
        [SerializeField] private Vector3 class1CamOrientation = new Vector3(29.5f, 112.75f, 0.0f);

        // Class 2 Top
        [SerializeField] private Vector3 class2CamPosition = new Vector3(2.31f, -0.23f, 10.05f);
        [SerializeField] private Vector3 class2CamOrientation = new Vector3(0.5f, 136.0f, 0.0f);
        
        // Class 3 Walls
        [SerializeField] private Vector3 class3CamPosition = new Vector3(0.27f, -0.46f, 5.58f);
        [SerializeField] private Vector3 class3CamOrientation = new Vector3(12.5f, 65.75f, 0.0f);

        // Class 4 Tech
        [SerializeField] private Vector3 class4CamPosition = new Vector3(1.35f, -0.42f, 10.91f);
        [SerializeField] private Vector3 class4CamOrientation = new Vector3(18.25f, 137.75f, 0.0f);
        
        // Class 5 Bottom
        [SerializeField] private Vector3 class5CamPosition = new Vector3(2.58f, -5.65f, 10.07f);
        [SerializeField] private Vector3 class5CamOrientation = new Vector3(-40.5f, 137.75f, 0.0f);

        // Exploded View
        [SerializeField] private Vector3 ExplodedPosition = new Vector3(-8.85076523f, 9.89685631f, 6.88709688f);
        [SerializeField] private Vector3 ExplodedOrientation = new Vector3(18, 82.75f, 0);

        [Header("Instance Level Positions")]
        // Class 4 instances (Tech)
        [SerializeField] private Vector3 class4Instance1CamPosition = new Vector3(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 class4Instance1CamOrientation = new Vector3(0.0f, 0.0f, 0.0f);  
        [SerializeField] private Vector3 class4Instance2CamPosition = new Vector3(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 class4Instance2CamOrientation = new Vector3(0.0f, 0.0f, 0.0f); 

        // Class 5 instances (Pipes)
        [SerializeField] private Vector3 class5Instance1CamPosition = new Vector3(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 class5Instance1CamOrientation = new Vector3(0.0f, 0.0f, 0.0f);  
        [SerializeField] private Vector3 class5Instance2CamPosition = new Vector3(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 class5Instance2CamOrientation = new Vector3(0.0f, 0.0f, 0.0f);  
        
        // Class 6 instances (Bottom)
        [SerializeField] private Vector3 class6Instance1CamPosition = new Vector3(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 class6Instance1CamOrientation = new Vector3(0.0f, 0.0f, 0.0f); 
        [SerializeField] private Vector3 class6Instance2CamPosition = new Vector3(0.0f, 0.0f, 0.0f);
        [SerializeField] private Vector3 class6Instance2CamOrientation = new Vector3(0.0f, 0.0f, 0.0f);   

        void Start() 
        {
            //Start scene without mouse moving camera
            MouseClickOnScene = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
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
                    pointCloudControls.keyboardShotcutsEnabled = true;
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

            // if ((/*uIInstanceController.LLMMenuActive == false &&*/ Input.GetKeyDown("space") && withLLM == false) || (cloudControllerLLM.keyboardShotcutsEnabled == true && Input.GetKeyDown("space") && withLLM == true)) // Use this if not LLM scene 
            if(Input.GetKeyDown("space") && pointCloudControls.keyboardShotcutsEnabled == true)
            {
                MoveToDefaultPosition();
            }  
        }

        void FixedUpdate()
        {
            //React to controls. (WASD, EQ and Mouse)
            if (movingCamera == false)
            {
                CameraControlMovement();
            }   
        }

        public void CameraControlMovement()
        {
            if (MouseClickOnScene)
            {
                float moveHorizontal = Input.GetAxis("Horizontal");
                float moveVertical = Input.GetAxis("Vertical");
                float moveUp = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;

                float speed = normalSpeed;

                if (Input.mouseScrollDelta.y > 0)
                {
                    transform.Translate(new Vector3(0, 0, 10 * normalSpeed * Time.deltaTime));
                }
                else if (Input.mouseScrollDelta.y < 0)
                {
                    transform.Translate(new Vector3(0, 0, -10 * normalSpeed * Time.deltaTime));
                }

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
        public void CameraClassTranslation(int ZoomToClass)
        {
            if (ZoomToClass == 1) // Terrain
            {
                movingCamera = true;
                Camera.main.transform.position = class1CamPosition;
                currentCamPosition = class1CamPosition;
                pitch = class1CamOrientation.x;
                yaw = class1CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (ZoomToClass == 2) // Top
            {
                movingCamera = true;
                Camera.main.transform.position = class2CamPosition;
                currentCamPosition = class2CamPosition;
                pitch = class2CamOrientation.x;
                yaw = class2CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (ZoomToClass == 3) // Walls
            {
                movingCamera = true;
                Camera.main.transform.position = class3CamPosition;
                currentCamPosition = class3CamPosition;
                pitch = class3CamOrientation.x;
                yaw = class3CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (ZoomToClass == 4) // Bottom
            {
                movingCamera = true;
                Camera.main.transform.position = class4CamPosition;
                currentCamPosition = class4CamPosition;
                pitch = class4CamOrientation.x;
                yaw = class4CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (ZoomToClass == 5) // Tech
            {
                movingCamera = true;
                Camera.main.transform.position = class5CamPosition;
                currentCamPosition = class5CamPosition;
                pitch = class5CamOrientation.x;
                yaw = class5CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (ZoomToClass == 6) // Pipes
            {
                movingCamera = true;
                Camera.main.transform.position = class5CamPosition;
                currentCamPosition = class5CamPosition;
                pitch = class5CamOrientation.x;
                yaw = class5CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (ZoomToClass == 7) // Explode View
            {
                movingCamera = true;
                Camera.main.transform.position = ExplodedPosition;
                currentCamPosition = ExplodedPosition;
                pitch = ExplodedOrientation.x;
                yaw = ExplodedOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
        }

        public void CameraInstanceTranslation(int cloudClass, int ZoomToInstance)
        {
            if (cloudClass < 4)
            {
                return;
            }
            if (cloudClass == 4 && ZoomToInstance == 1) // 04_01
            {
                movingCamera = true;
                Camera.main.transform.position = class4Instance1CamPosition;
                currentCamPosition = class4Instance1CamPosition;
                pitch = class4Instance1CamOrientation.x;
                yaw = class4Instance1CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (cloudClass == 4 && ZoomToInstance == 2) // 04_02
            {
                movingCamera = true;
                Camera.main.transform.position = class4Instance2CamPosition;
                currentCamPosition = class4Instance2CamPosition;
                pitch = class4Instance2CamOrientation.x;
                yaw = class4Instance2CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (cloudClass == 5 && ZoomToInstance == 1) // 05_01
            {
                movingCamera = true;
                Camera.main.transform.position = class5Instance1CamPosition;
                currentCamPosition = class5Instance1CamPosition;
                pitch = class5Instance1CamOrientation.x;
                yaw = class5Instance1CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (cloudClass == 5 && ZoomToInstance == 2) // 05_02
            {
                movingCamera = true;
                Camera.main.transform.position = class5Instance2CamPosition;
                currentCamPosition = class5Instance2CamPosition;
                pitch = class5Instance2CamOrientation.x;
                yaw = class5Instance2CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (cloudClass == 6 && ZoomToInstance == 1) // 06_01
            {
                movingCamera = true;
                Camera.main.transform.position = class6Instance1CamPosition;
                currentCamPosition = class6Instance1CamPosition;
                pitch = class6Instance1CamOrientation.x;
                yaw = class6Instance1CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
            if (cloudClass == 6 && ZoomToInstance == 2) // 06_02
            {
                movingCamera = true;
                Camera.main.transform.position = class6Instance2CamPosition;
                currentCamPosition = class6Instance2CamPosition;
                pitch = class6Instance2CamOrientation.x;
                yaw = class6Instance2CamOrientation.y;
                transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
                movingCamera = false;
                return;
            }
        }

        public void MoveToDefaultPosition()
        {
            movingCamera = true;
            Camera.main.transform.position = setResetPosition;
            currentCamPosition = setResetPosition;
            // Camera.main.transform.eulerAngles = new Vector3(20.75f, 53.5f, 0f);
            pitch = setResetAngle.x;
            yaw = setResetAngle.y;
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
            movingCamera = false;
        }
    }


}

