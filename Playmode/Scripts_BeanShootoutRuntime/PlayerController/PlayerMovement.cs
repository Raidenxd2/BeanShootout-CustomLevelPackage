using System;
using Cysharp.Threading.Tasks;
using SerialPackage.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace KillItMyself.Runtime
{
    [RequireComponent(typeof(Rigidbody))]
    public class PlayerMovement : NetworkBehaviour
    {
        private bool readyToJump;

        [Header("Camera")]
        public Camera playerCam;

        [Header("Ground Check")]
        public float playerHeight;
        public LayerMask whatIsGround;
        public bool grounded;

        [Header("Other")]
        [SerializeField] private bool isBot;
#if KILLITMYSELF_FULL
        [SerializeField] private BotPlayer bot;
#endif
        public Transform playerModel;
        [SerializeField] private MeshRenderer playerModelRenderer;
        // public LayerMask dontRenderLayer;
        public LayerMask spinnerLayer;
        public bool canMove = true;
        public bool IsOnKeyboardMouse;
        private float horizontalInput;
        private float verticalInput;
        private Vector3 moveDirection;
        public Rigidbody rb;
        Transform oldParent;
#if KILLITMYSELF_FULL
        public GameObject ShipLevel_OverrideCodeInteractUI;
        public GameObject ShipLevel_OverrideCodeUI;
        public GameObject HotelLevel_LeverInteractUI; 
        public GameObject HotelLevel_CodeInputInteractUI;
        public GameObject HotelLevel_CodeInputUI;
#endif
        [SerializeField] private PlayerInput playerControls;
        private InputAction MovementInput;
        public InputAction JumpInput;
        private InputAction InteractInput;
        private InputAction SprintInput;
        [SerializeField] private Transform ControllerButtonsParent;
        [SerializeField] private GameObject XboxConrollerButtons;
        [SerializeField] private GameObject PlayStationButtons;
        [SerializeField] private GameObject NintendoButtons;
        [SerializeField] private GameObject GenericButtons;
        private GameObject CurrentButtons;
        public DualShockGamepad psGamepad;
        public bool hasPsGamepad;
        [SerializeField] private Transform PlayerLocationCircle;
        [SerializeField] private PlayerFade fade;

        public BulletManager bulletManager;
        public PlayerCam playerCamComponent;
        public PlayerBaseRootLocalUsername username;

        [SerializeField] private GameObject DeviceDisconnectedUI;
        
        private CursorLockMode prevCursorLockMode;
        private bool prevCanMove;
        private bool prevCannotShootNoMatterWhat;
        private bool prevCanShoot;
        private bool prevCanMoveCamera;
        private Vector3 prevVel;

        private bool Respawning;

        public int Kills;
        public NetworkVariable<int> KillsOnline = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        [SerializeField] private GameObject Crosshair;
        [SerializeField] private Transform UIRoot;
        private Vector3 UIRootBasePos;
        
        private InputAction vr_leftPositionInputAction = new(binding: "<XRController>{LeftHand}/{Primary2DAxis}", expectedControlType: "Vector2");
        private InputAction vr_leftTriggerInputAction = new(binding: "<XRController>{LeftHand}/triggerPressed", expectedControlType: "Button");
        private InputAction vr_rightPrimaryButtonInputAction = new(binding: "<XRController>{RightHand}/primaryButton", expectedControlType: "Button");
        private InputAction vr_rightSecondaryButtonInputAction = new(binding: "<XRController>{RightHand}/secondaryButton", expectedControlType: "Button");

        private int baseFov;
        private int sprintFov;

        public bool IsFirstVRPlayer;

        private void Start()
        {
            PlayersJoined.instance.Players.Add(gameObject);
            if (PlayersJoined.instance.Players.Count == 1)
            {
                IsFirstVRPlayer = true;
            }

            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

            UpdateFOV();
            
#if KILLITMYSELF_FULL
            if (VRManager.instance.VREnabled)
            {
                UIRootBasePos = UIRoot.GetComponent<VRCanvas>().newPos;
                playerCam.enabled = false;

                if (!VRManager.instance.FakeVR)
                {
                    vr_leftPositionInputAction.Enable();
                    vr_leftTriggerInputAction.Enable();
                    vr_rightPrimaryButtonInputAction.Enable();
                    vr_rightSecondaryButtonInputAction.Enable();

                    MovementInput = vr_leftPositionInputAction;
                    JumpInput = vr_rightPrimaryButtonInputAction;
                    SprintInput = vr_leftTriggerInputAction;
                    InteractInput = vr_rightSecondaryButtonInputAction;
                
                    Crosshair.SetActive(false);
                }
                
                playerModel.GetComponent<MeshRenderer>().enabled = false;
            }

            if (SceneManager.GetActiveScene().name.Equals(SceneNames.S_BossfightPhase1Level))
            {
                BossfightAttacks.instance.AddHealthForPlayer();
            }
#endif

            oldParent = transform.parent;
            
            rb.freezeRotation = true;

            ResetJump();
            
#if KILLITMYSELF_FULL
            if (isBot)
            {
                bot.Init();
                return;
            }
#endif

            if (OnlineManager.instance.InOnlineGame)
            {
                playerModelRenderer.enabled = false;
            }

            playerControls.deviceRegainedEvent.AddListener(OnDeviceReconnect);

            if (!VRManager.instance.VREnabled)
            {
                OnControlsChanged(playerControls);
                
                if (OnlineManager.instance.InOnlineGame)
                {
                    playerControls.controlsChangedEvent.AddListener(OnControlsChanged);
                }
            }

            if (VRManager.instance.FakeVR)
            {
                OnControlsChanged(playerControls);
            }

#if KILLITMYSELF_FULL
            if (SceneManager.GetActiveScene().name.StartsWith(SceneNames.S_ArcadeLevel) && BetterPrefs.GetBool("TurnOnBrokenArcadeMachine", false))
            {
                GameObject.Find("BrokenArcadeMachineSound").GetComponent<AudioSource>().volume = 0.2f;
            }
#endif
        }

        public void UpdateFOV()
        {
            baseFov = BetterPrefs.GetInt(PrefNames.CameraFOV, 80);
            sprintFov = baseFov + GameSettings.MovementSettings.fovSprintAddition;
            
            playerCam.fieldOfView = baseFov;
        }

        private void FixedUpdate()
        {
            if (Respawning || OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

            if (!OnlineManager.instance.InOnlineGame)
            {
                username.Kills = Kills;
            }

            if (transform.position.y <= -100 && !Respawning)
            {
                Respawning = true;
                Respawn().Forget();
            }

            MovePlayer();
        }

        private string deviceName;
        public void OnDeviceDisconnect(PlayerInput playerInput)
        {
            deviceName = playerInput.devices[0].displayName;
            BeanLogger.LogWarning("Device " + deviceName + " has disconnected.", this);
            DeviceDisconnectedUI.SetActive(true);
        }

        public void OnDeviceReconnect(PlayerInput playerInput)
        {
            BeanLogger.LogWarning("Device " + deviceName + " has reconnected.", this);
            DeviceDisconnectedUI.GetComponent<WindowAnimation>().Close();
        }

        public void OnControlsChanged(PlayerInput playerInput)
        {
            DeviceDisconnectedUI.GetComponent<WindowAnimation>().Close();

            if (BeanLogger.VerboseLogging)
            {
                BeanLogger.Log("OnControlsChanged " + playerInput.devices[0].displayName, this);
            }
            
            try
            {
                if (playerControls.currentControlScheme.Contains("Keyboard") || playerControls.currentControlScheme.Contains("Mouse"))
                {
                    IsOnKeyboardMouse = true;
                }
                else
                {
                    IsOnKeyboardMouse = false;
                }
            }
            catch (Exception e)
            {
                IsOnKeyboardMouse = true;
                Debug.LogException(e);
            }

            try
            {
                if (CommandLineArgs.VerboseLoggingEnabled)
                {
                    BeanLogger.Log("Controller2: " + playerControls.devices[0].displayName, this);
                    BeanLogger.Log(playerControls.devices[0].name, this);
                }

                if (BetterPrefs.GetBool("ControllerSettings_PSLightBar", true))
                {
                    foreach (var device in playerControls.devices)
                    {
                        if (device is DualShockGamepad)
                        {
                            psGamepad = device as DualShockGamepad;
                            hasPsGamepad = true;
                    
                            psGamepad.SetLightBarColor(Color.blue);

                            break;
                        }
                        else
                        {
                            hasPsGamepad = false;
                        }
                    }
                }

                if (CurrentButtons)
                {
                    Destroy(CurrentButtons);
                }
                
                if (playerControls.devices[0].displayName.Contains("Xbox"))
                {
                    CurrentButtons = Instantiate(XboxConrollerButtons, ControllerButtonsParent);
                }
                else if (playerControls.devices[0].displayName.Contains("DualSense") || playerControls.devices[0].displayName.Contains("DualShock") || playerControls.devices[0].name.Contains("DualShock"))
                {
                    CurrentButtons = Instantiate(PlayStationButtons, ControllerButtonsParent);
                }
                else if (playerControls.devices[0].displayName.Contains("Nintendo") || playerControls.devices[0].displayName.Contains("Pro Controller") || playerControls.devices[0].name.Contains("Switch") || playerControls.devices[0].name.Contains("ProController"))
                {
                    CurrentButtons = Instantiate(NintendoButtons, ControllerButtonsParent);
                }
                else if (playerControls.devices[0].name.Contains("Gamepad"))
                {
                    // CurrentButtons = Instantiate(GenericButtons, ControllerButtonsParent);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            
#if KILLITMYSELF_FULL
            if (IsOnKeyboardMouse)
            {
                MovementInput = playerControls.actions["Movement"];
                JumpInput = CurrentBindings.instance.JumpAction;
                InteractInput = CurrentBindings.instance.InteractAction;
                SprintInput = CurrentBindings.instance.SprintAction;
            }
            else
            {
#endif
                MovementInput = playerControls.actions["Movement"];
                JumpInput = playerControls.actions["Jump"];
                InteractInput = playerControls.actions["Interact"];
                SprintInput = playerControls.actions["Sprint"];
#if KILLITMYSELF_FULL
            }
#endif
            
            playerCamComponent.UpdateValues();
            bulletManager.UpdateValues();
        }

        private async UniTask Respawn()
        {
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            fade.gameObject.SetActive(true);
            fade.FadeIn();
            await UniTask.WaitForSeconds(0.5f);

            if (SpawnManager.instance)
            {
                rb.position = SpawnManager.instance.SpawnPoints[Random.Range(0, SpawnManager.instance.SpawnPoints.Length)].position;
            }
            else
            {
                rb.position = Vector3.zero;
            }

            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;

            Respawning = false;
            rb.useGravity = true;
            fade.FadeOut();

            await UniTask.WaitForSeconds(0.5f);
            fade.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (Respawning || OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

#if KILLITMYSELF_FULL
            if (VRManager.instance.VREnabled)
            {
                VRManager.instance.GlobalXROrigin.transform.position = transform.position - new Vector3(0, 1.3f, 0);
                VRManager.instance.GlobalXROrigin.transform.rotation = playerModel.rotation;

                Vector3 newPos = playerModel.position + playerModel.forward + UIRootBasePos * 2;
                Quaternion newRot = Quaternion.Euler(0, playerModel.rotation.eulerAngles.y, 0);
                
                UIRoot.transform.localPosition = newPos;
                UIRoot.transform.rotation = newRot;
                
                VRDDOLCanvases.instance.ChangePosition(newPos);
                VRDDOLCanvases.instance.ChangeRotation(newRot);
                
                VRLevelBaseCanvas.instance.ChangePosition(newPos);
                VRLevelBaseCanvas.instance.ChangeRotation(newRot);

                if (VRMinimapCanvas.instance)
                {
                    VRMinimapCanvas.instance.ChangePosition(newPos);
                    VRMinimapCanvas.instance.ChangeRotation(newRot);
                }
            }
#endif

            // Ground check
            grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

            MyInput();
            SpeedControl();

            // Handle drag
            if (grounded)
            {
                rb.linearDamping = GameSettings.MovementSettings.groundDrag;
            }
            else
            {
                rb.linearDamping = 0;
            }

            PlayerLocationCircle.SetPositionAndRotation(new(1500.2f + transform.position.x, 1337.335f + transform.position.y, 1500 + transform.position.z), Quaternion.Euler(-90, playerCam.transform.rotation.eulerAngles.y - 90, 0));
        }

        private void MyInput()
        {
            if (!canMove)
            {
                horizontalInput = 0;
                verticalInput = 0;
                return;
            }

            Vector2 moveDirection = MovementInput.ReadValue<Vector2>();
            horizontalInput = moveDirection.x;
            verticalInput = moveDirection.y;

            // When to jump
            if (JumpInput.IsPressed() && readyToJump && grounded)
            {
                readyToJump = false;

                Jump();

                Invoke(nameof(ResetJump), GameSettings.MovementSettings.jumpCooldown);
            }

#if KILLITMYSELF_FULL
            if (InteractInput.WasPressedThisFrame() && ShipLevel_OverrideCodeInteractUI.activeSelf && !ShipLevel_OverrideCodeUI.activeSelf)
            {
                PreventPlayerFromDoingAnything();
                Cursor.lockState = CursorLockMode.None;
                
                ShipLevel_OverrideCodeUI.SetActive(true);
            }

            if (InteractInput.WasPressedThisFrame() && HotelLevel_LeverInteractUI.activeSelf)
            {
                HotelLevel_LeverInteract.instance.LeverInteract();
                HotelLevel_LeverInteractUI.SetActive(false);
            }

            if (InteractInput.WasPressedThisFrame() && HotelLevel_CodeInputInteractUI.activeSelf)
            {
                PreventPlayerFromDoingAnything();
                Cursor.lockState = CursorLockMode.None;
                
                HotelLevel_CodeInputUI.SetActive(true);
            }
#endif
        }

        public void CloseShipOverrideCodeUI()
        {
#if KILLITMYSELF_FULL
            LetPlayerDoAnything();

            Cursor.lockState = CursorLockMode.Locked;
            ShipLevel_OverrideCodeUI.SetActive(false);
            HotelLevel_CodeInputUI.SetActive(false);
#endif
        }

        private void MovePlayer()
        {
            bool hit = Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, playerHeight * 0.5f + 0.2f, spinnerLayer);

            if (hit)
            {
                transform.SetParent(hitInfo.transform);
            }
            else
            {
                if (oldParent)
                {
                    transform.SetParent(oldParent);
                }
            }

            // Calculate movement direction
            moveDirection = playerModel.forward * verticalInput + playerModel.right * horizontalInput;

            // On ground
            if (grounded)
            {
                if (SprintInput.IsPressed())
                {
                    playerCam.fieldOfView = sprintFov;
                    rb.AddForce(moveDirection.normalized * (GameSettings.MovementSettings.sprintSpeed * 10f), ForceMode.Force);
                }
                else
                {
                    playerCam.fieldOfView = baseFov;
                    rb.AddForce(moveDirection.normalized * (GameSettings.MovementSettings.moveSpeed * 10f), ForceMode.Force);
                }
            }
            // In air
            else if (!grounded)
            {
                rb.AddForce(moveDirection.normalized * (GameSettings.MovementSettings.moveSpeed * 10f * GameSettings.MovementSettings.airMultiplier), ForceMode.Force);
            }
        }

        private void SpeedControl()
        {
            Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // Limit velocity if needed
            if (flatVel.magnitude > GameSettings.MovementSettings.moveSpeed)
            {
                Vector3 limitedVel = flatVel.normalized * GameSettings.MovementSettings.moveSpeed;
                rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }

        private void Jump()
        {
            //Reset Y velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            rb.AddForce(transform.up * GameSettings.MovementSettings.jumpForce, ForceMode.Impulse);
        }

        public void ResetJump()
        {
            readyToJump = true;
        }

        public void PreventPlayerFromDoingAnything()
        {
            // prevCanMove = canMove;
            // prevVel = GetComponent<Rigidbody>().linearVelocity;
            // prevCanShoot = bulletManager.CanShoot;
            // prevCanMoveCamera = playerCamComponent.canMoveCamera;
            // prevCannotShootNoMatterWhat = bulletManager.CannotShootNoMatterWhat;
            canMove = false;
            rb.linearVelocity = Vector3.zero;
            // bulletManager.CanShoot = false;
            bulletManager.CannotShootNoMatterWhat = true;
            playerCamComponent.canMoveCamera = false;
        }

        public void LetPlayerDoAnything()
        {
            canMove = true;
            // rb.linearVelocity = prevVel;
            // bulletManager.CanShoot = true;
            bulletManager.CannotShootNoMatterWhat = false;
            playerCamComponent.canMoveCamera = true;
        }
    }
}