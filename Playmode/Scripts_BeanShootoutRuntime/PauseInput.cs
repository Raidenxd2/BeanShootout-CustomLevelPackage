using Unity.Netcode;
using UnityEngine.InputSystem;

namespace KillItMyself.Runtime
{
    public class PauseInput : NetworkBehaviour
    {
        public PlayerInput playerInput;
        private InputAction PauseInputA;
        
        private InputAction vr_leftSecondaryButtonInputAction = new(binding: "<XRController>{LeftHand}/secondaryButton", expectedControlType: "Button");

        private void Start()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }
            
            if (VRManager.instance.VREnabled && !VRManager.instance.FakeVR)
            {
                vr_leftSecondaryButtonInputAction.Enable();
                PauseInputA = vr_leftSecondaryButtonInputAction;
            }
            else
            {
                PauseInputA = playerInput.actions["Pause"];
            }
        }

        private void Update()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }
            
            if (PauseInputA.WasPressedThisFrame())
            {
                PauseManager.instance.PauseOrUnpause();
            }
        }
    }
}