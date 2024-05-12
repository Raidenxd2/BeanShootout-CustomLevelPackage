using UnityEngine;
using UnityEngine.InputSystem;

namespace KillItMyself.Runtime
{
    public class PauseInput : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerInput;

        private void Update()
        {
            if (playerInput.actions["Pause"].WasPressedThisFrame())
            {
                PauseManager.instance.PauseOrUnpause();
            }
        }
    }
}