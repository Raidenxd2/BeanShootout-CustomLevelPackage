using UnityEngine;
using UnityEngine.InputSystem;

namespace KillItMyself.Runtime
{
    public class PauseManager : MonoBehaviour
    {
        private bool paused;

        [SerializeField] private GameObject PauseScreen;

        [SerializeField] private PlayerInput playerInput;

        public static PauseManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void PauseOrUnpause()
        {
            paused = !paused;

            if (paused)
            {
                Time.timeScale = 0;

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                PauseScreen.SetActive(true);
            }
            else
            {
                Time.timeScale = 1;

                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;

                PauseScreen.SetActive(false);
            }
        }

        public void ResumeGame()
        {
            paused = false;

            Time.timeScale = 1;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            PauseScreen.SetActive(false);
        }

        public void QuitGame()
        {
            Debug.Log("(PauseManager) Player 1 requested quit.");

            Time.timeScale = 1;

            PlayerCam.ChangePlayerHasJoined();

            #if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
            #endif
        }
    }
}