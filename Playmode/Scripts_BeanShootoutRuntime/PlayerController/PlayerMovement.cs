using UnityEngine;
using UnityEngine.InputSystem;

namespace KillItMyself.Runtime
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Movement")]
        public float moveSpeed;
        public float sprintSpeed;

        public float groundDrag;

        public float jumpForce;
        public float jumpCooldown;
        public float airMultiplier;
        public bool readyToJump;

        [Header("Camera")]
        public Camera playerCam;
        public float fovNormal = 75f;
        public float fovSprint = 80f;

        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        public bool grounded;

        [Header("Other")]
        public Transform orientation;
        public GameObject playerModel;
        public LayerMask dontRenderLayer;
        public bool canMove = true;
        private float horizontalInput;
        private float verticalInput;
        private Vector3 moveDirection;
        private Rigidbody rb;
        [SerializeField] private PlayerInput playerControls;
        [SerializeField] private Transform ControllerButtonsParent;
        [SerializeField] private GameObject XboxConrollerButtons;

        [Header("Mobile")]
        public bool movingForward;
        public float horizontalInputMobile;
        public float verticalInputMobile;

        private void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            ResetJump();
            
            Debug.Log("(PlayerMovement) Controller2: " + playerControls.devices[0].displayName);

            if (playerControls.devices[0].displayName.Contains("Xbox"))
            {
                Instantiate(XboxConrollerButtons, ControllerButtonsParent);
            }
        }

        private void FixedUpdate()
        {
            MovePlayer();
        }

        private void Update()
        {
            // Ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            if (!canMove)
            {
                return;
            }

            MyInput();
            SpeedControl();

            // Handle drag
            if (grounded)
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearDamping = groundDrag;
#else
                rb.drag = groundDrag;
#endif
            }
            else
            {
#if UNITY_6000_0_OR_NEWER
                rb.linearDamping = 0;
#else
                rb.drag = 0;
#endif
            }
        }

        private void MyInput()
        {
            Vector2 moveDirection = playerControls.actions["Movement"].ReadValue<Vector2>();
            horizontalInput = moveDirection.x;
            verticalInput = moveDirection.y;

            // When to jump
            if (playerControls.actions["Jump"].IsPressed() && readyToJump && grounded)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), jumpCooldown);
            }
        }

        private void MovePlayer()
        {
            // Calculate movement direction
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

            if (movingForward)
            {
                moveDirection = orientation.forward * verticalInputMobile + orientation.right * horizontalInputMobile;
                if (!grounded)
                {
                    rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
                }
                else
                {
                    rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
                }
                playerCam.fieldOfView = fovNormal;
                
            }

            // On ground
            if (grounded)
            {
                if (playerControls.actions["Sprint"].IsPressed())
                {
                    playerCam.fieldOfView = fovSprint;
                    rb.AddForce(moveDirection.normalized * sprintSpeed * 10f, ForceMode.Force);
                }
                else
                {
                    playerCam.fieldOfView = fovNormal;
                    rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
                }
            }
            // In air
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            }
        }

        private void SpeedControl()
        {
#if UNITY_6000_0_OR_NEWER
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
#else
            Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
#endif

            // Limit velocity if needed
            if (flatVel.magnitude > moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * moveSpeed;
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
#else
                rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
#endif
            }
        }

        public void Jump()
        {
            //Reset Y velocity
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
#else
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
#endif

            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
        }

        public void ResetJump()
        {
            readyToJump = true;
        }
    }
}