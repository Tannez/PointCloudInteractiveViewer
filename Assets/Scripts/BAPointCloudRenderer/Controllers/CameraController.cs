using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BAPointCloudRenderer.Controllers {
    /*
     * CameraController for flying-controls
     */
    public class CameraController : MonoBehaviour {

        [SerializeField] UIInstanceController uIInstanceController;
        public bool MouseClickOnScene = true;

        //Current yaw
        private float yaw = 0.0f;
        //Current pitch
        private float pitch = 0.0f;

        public float normalSpeed = 10;

        [Header("Reset Camera Position")]
        [SerializeField] private Vector3 setResetPosition;
        [SerializeField] private Vector3 setResetAngle;

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
                }
            }

            // NEW MOUSE RELEASE TO GET OUT OF CAMERA CONTROL
            if (Input.GetMouseButtonUp(1) && MouseClickOnScene == true)
            {
                MouseClickOnScene = false;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }   

            if (uIInstanceController.LLMMenuActive == false && Input.GetKeyDown("space"))
                {
                    Camera.main.transform.position = setResetPosition;
                    // Camera.main.transform.eulerAngles = new Vector3(20.75f, 53.5f, 0f);
                    pitch = setResetAngle.x;
                    yaw = setResetAngle.y; 
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
    }

}
