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
        }

        private void Update()
        {
            if (!canMoveCamera)
            {
                return;
            }

            Vector2 rotateDirection = playerControls.actions["Camera"].ReadValue<Vector2>();

            yRotation += rotateDirection.x * sensX * Time.deltaTime;
            xRotation -= rotateDirection.y * sensY * Time.deltaTime;
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