using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

namespace KillItMyself.Runtime
{
    [RequireComponent(typeof(Camera))]
    [RequireComponent(typeof(UniversalAdditionalCameraData))]
    [RequireComponent(typeof(AudioListener))]
    public class PlayerCam : NetworkBehaviour
    {
        public float sensX;
        public float sensY;

        public Transform playerModel;

        public bool canMoveCamera = true;
        private float xRotation;
        private float yRotation;

        [SerializeField] private PlayerInput playerControls;
        private InputAction CameraInput;
        
        private static bool playerHasJoined;

        [SerializeField] private Camera Camera;
        [SerializeField] private AudioListener AudioListener;
        [SerializeField] private UniversalAdditionalCameraData URPCamData;
        
        private InputAction vr_rightPositionInputAction = new(binding: "<XRController>{RightHand}/{Primary2DAxis}", expectedControlType: "Vector2");

        private void Start()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                Camera.enabled = false;
                AudioListener.enabled = false;
                return;
            }

            if (VRManager.instance.VREnabled && !VRManager.instance.FakeVR)
            {
                vr_rightPositionInputAction.Enable();
                CameraInput = vr_rightPositionInputAction;
            }
            else
            {
                CameraInput = playerControls.actions["Camera"];
            }

            URPCamData.renderPostProcessing = BetterPrefs.GetBool("PostProcessing", true);

            if (!playerHasJoined)
            {
#if KILLITMYSELF_FULL
                RotateTowardsPlayer1Camera.player = transform;
#endif
                playerHasJoined = true;
            }

            if (BetterPrefs.GetBool("DeferredRendering", false))
            {
                URPCamData.SetRenderer(2);
            }

            if (VRManager.instance.VREnabled && !OnlineManager.instance.InOnlineGame && PlayersJoined.instance.Players.Count > 1)
            {
                Camera.enabled = false;
            }
            
            UpdateValues();
        }

        private void LateUpdate()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner || !canMoveCamera || PauseManager.instance.paused)
            {
                return;
            }

            Vector2 rotateDirection = CameraInput.ReadValue<Vector2>();

            if (VRManager.instance.VREnabled)
            {
                xRotation = 0;
            }
            else
            {
                xRotation -= rotateDirection.y * sensY * Time.fixedDeltaTime;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);
            }
            
            yRotation += rotateDirection.x * sensX * Time.fixedDeltaTime;

            //Rotate camera and playermodel
            transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);

            if (playerModel)
            {
                playerModel.rotation = Quaternion.Euler(0, yRotation, 0);
            }
        }

        public static void ChangePlayerHasJoined()
        {
            playerHasJoined = false;
        }

        public void UpdateValues()
        {
            switch (BetterPrefs.GetInt("AntiAliasing"))
            {
                case 0:
                case 1:
                case 2:
                case 3:
                    URPCamData.antialiasing = AntialiasingMode.None;
                    break;
                case 4:
                    URPCamData.antialiasing = AntialiasingMode.FastApproximateAntialiasing;
                    break;
                case 5:
                    URPCamData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    URPCamData.antialiasingQuality = AntialiasingQuality.Low;
                    break;
                case 6:
                    URPCamData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    URPCamData.antialiasingQuality = AntialiasingQuality.Medium;
                    break;
                case 7:
                    URPCamData.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
                    URPCamData.antialiasingQuality = AntialiasingQuality.High;
                    break;
            }
            
            if (playerControls.currentControlScheme == "Gamepad")
            {
                int BaseSensitivityX = 10;
                int BaseSensitivityY = 10;

                if (BetterPrefs.GetBool("ControllerSettings_InvertCameraX"))
                {
                    BaseSensitivityX = -10;
                }
                
                if (BetterPrefs.GetBool("ControllerSettings_InvertCameraY"))
                {
                    BaseSensitivityY = -10;
                }
                
                sensX = BaseSensitivityX * BetterPrefs.GetInt("ControllerSettings_Sensitivity", 10);
                sensY = BaseSensitivityY * BetterPrefs.GetInt("ControllerSettings_Sensitivity", 10);
            }

            try
            {
                if (playerControls.devices[0].displayName.Contains("Keyboard") || playerControls.devices[0].displayName.Contains("Mouse"))
                {
                    int BaseSensitivityX = 2;
                    int BaseSensitivityY = 2;

                    if (BetterPrefs.GetBool("KeyboardMouseSettings_InvertCameraX"))
                    {
                        BaseSensitivityX = -2;
                    }
                
                    if (BetterPrefs.GetBool("KeyboardMouseSettings_InvertCameraY"))
                    {
                        BaseSensitivityY = -2;
                    }
                    
                    sensX = BaseSensitivityX * BetterPrefs.GetInt("KeyboardMouseSettings_MouseSensitivity", 1);
                    sensY = BaseSensitivityY * BetterPrefs.GetInt("KeyboardMouseSettings_MouseSensitivity", 1);
                }
            }
            catch (Exception e)
            {
                sensX = 2 * BetterPrefs.GetInt("KeyboardMouseSettings_MouseSensitivity", 1);
                sensY = 2 * BetterPrefs.GetInt("KeyboardMouseSettings_MouseSensitivity", 1);
                
                Debug.LogException(e);
            }
            
            if (VRManager.instance.VREnabled && !VRManager.instance.FakeVR)
            {
                int BaseSensitivityX = 15;
                int BaseSensitivityY = 15;

                if (BetterPrefs.GetBool("VRSettings_InvertCameraX"))
                {
                    BaseSensitivityX = -15;
                }
                
                if (BetterPrefs.GetBool("VRSettings_InvertCameraY"))
                {
                    BaseSensitivityY = -15;
                }
                
                sensX = BaseSensitivityX * BetterPrefs.GetInt("ControllerSettings_Sensitivity", 10);
                sensY = BaseSensitivityY * BetterPrefs.GetInt("ControllerSettings_Sensitivity", 10);
            }
        }

        public static void UpdateValuesGlobal()
        {
            if (OnlineManager.instance && OnlineManager.instance.InOnlineGame)
            {
                CurrentPlayer.instance.playerCam.UpdateValues();
            }
            else if (PlayersJoined.instance && PlayersJoined.instance.Players.Count > 0)
            {
                foreach (var player in PlayersJoined.instance.Players)
                {
                    player.GetComponent<PlayerMovement>().playerCamComponent.UpdateValues();
                }
            }
        }

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void ResetValues()
        {
            playerHasJoined = false;
        }
#endif
    }
}