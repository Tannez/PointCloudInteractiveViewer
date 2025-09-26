using UnityEngine;
using UnityEngine.EventSystems;

namespace BAPointCloudRenderer.Controllers {
    /*
     * CameraController for flying-controls
     */
    public class CameraController : MonoBehaviour {

        private bool MouseClickOnScene = true;

        //Current yaw
        private float yaw = 0.0f;
        //Current pitch
        private float pitch = 0.0f;

        public float normalSpeed = 10;

        void Start() {
            //Hide the cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void Update() {
            if (Input.GetKey(KeyCode.Escape) && MouseClickOnScene == true)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                MouseClickOnScene = false;
            }

            else if (Input.GetKey(KeyCode.Escape) && MouseClickOnScene == false)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                MouseClickOnScene = true;
            }

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
                    
        }

        void FixedUpdate() {
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
