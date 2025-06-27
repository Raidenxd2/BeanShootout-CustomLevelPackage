using UnityEngine;
using UnityEngine.InputSystem;

namespace KillItMyself.Runtime
{
    public class PlayerCam : MonoBehaviour
    {
        public float sensX;
        public float sensY;

        public Transform orientation;
        public Transform playerModel;

        public bool canMoveCamera = true;
        private float xRotation;
        private float yRotation;

        [SerializeField] private PlayerInput playerControls;

        private static bool playerHasJoined;

        private void Start()
        {
            if (!playerHasJoined)
            {
                gameObject.tag = "Player1Camera";
                playerHasJoined = true;
            }

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (playerControls.currentControlScheme == "Gamepad")
            {
                sensX = 225;
                sensY = 225;
            }

            if (playerControls.devices[0].displayName.Contains("Keyboard") || playerControls.devices[0].displayName.Contains("Mouse"))
            {
                sensX = 10;
                sensY = 10;
            }
        }

        private void LateUpdate()
        {
            if (!canMoveCamera)
            {
                return;
            }

            Vector2 rotateDirection = playerControls.actions["Camera"].ReadValue<Vector2>();

            yRotation += rotateDirection.x * sensX * Time.fixedDeltaTime;
            xRotation -= rotateDirection.y * sensY * Time.fixedDeltaTime;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            //Rotate camera and orientation
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
            orientation.rotation = Quaternion.Euler(0, yRotation, 0);

            playerModel.rotation = Quaternion.Euler(0, yRotation, 0);
        }

        public static void ChangePlayerHasJoined()
        {
            playerHasJoined = false;
        }
    }
}