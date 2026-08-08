#if KILLITMYSELF_FULL
using Cysharp.Threading.Tasks;
using UnityEngine.UI;
#endif
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace KillItMyself.Runtime
{
    public class PauseManager : MonoBehaviour
    {
        public bool paused;

        [SerializeField] private GameObject PauseScreen;

        [SerializeField] private GameObject LoadingAsset;

        [SerializeField] private GameObject BotPlayerPrefab;
        
        [SerializeField] private Transform SettingsRootParent;
        private GameObject SettingsRoot;
#if KILLITMYSELF_FULL
        [SerializeField] private AssetReference SettingsRootRef;
        private AsyncOperationHandle<GameObject> SettingsRootHandle;
#endif
        private AssetBundle SettingsRootSharedBundle;
        private AssetBundle SettingsRootBundle;

        [SerializeField] private GameObject ResumeGameButton;
        [SerializeField] private GameObject NoButton;

        public CursorLockMode prevCursorLock;
        private bool prevCanMove;

        public bool CanPause = true;

        public static PauseManager instance;

        private void Awake()
        {
            instance = this;
        }

        public void PauseOrUnpause()
        {
            if (!CanPause)
            {
                return;
            }
            
            paused = !paused;

            if (paused)
            {
                SetTimeScale(0);
                
                if (OnlineManager.instance.InOnlineGame)
                {
                    CurrentPlayer.instance.playerMovement.PreventPlayerFromDoingAnything();
                }

                prevCursorLock = Cursor.lockState;

                Cursor.lockState = CursorLockMode.None;

                PauseScreen.SetActive(true);

                EventSystem.current.SetSelectedGameObject(ResumeGameButton);
            }
            else
            {
                SetTimeScale(1);
                
                if (OnlineManager.instance.InOnlineGame)
                {
                    CurrentPlayer.instance.playerMovement.LetPlayerDoAnything();
                }

                Cursor.lockState = prevCursorLock;

                PauseScreen.SetActive(false);
            }
        }

        public void ShowExitGameScreen()
        {
            EventSystem.current.SetSelectedGameObject(NoButton);
        }

        public void DontShowExitGameScreen()
        {
            EventSystem.current.SetSelectedGameObject(ResumeGameButton);
        }


        public void OpenSettingsMenu()
        {
#if KILLITMYSELF_FULL
            ShowSettingsAsync().Forget();
#elif UNITY_EDITOR
            UnityEditor.EditorDialog.DisplayAlertDialog("The Great Bean Shootout SDK", "You cannot access settings in the Editor. Please open the game and change settings there.", "OK", UnityEditor.DialogIconType.Warning);
#endif
        }

#if KILLITMYSELF_FULL
        private async UniTaskVoid ShowSettingsAsync()
        {
            SavingRootObject.instance.LoadingAssetRoot.SetActive(true);
            
            SettingsRootHandle = Addressables.LoadAssetAsync<GameObject>(SettingsRootRef);
            await SettingsRootHandle;
            SettingsRoot = Instantiate(SettingsRootHandle.Result, SettingsRootParent);

            GameObject.Find("SettingsRoot_BackButton").GetComponent<Button>().onClick.AddListener(DestroySettings);
            
            SavingRootObject.instance.LoadingAssetRoot.SetActive(false);
        }

        private void DestroySettings()
        {
            Destroy(SettingsRoot);

            if (SettingsRootHandle.IsValid())
            {
                Addressables.Release(SettingsRootHandle);
            }
        }
#endif

        private void SetTimeScale(float val)
        {
            if (!OnlineManager.instance.InOnlineGame)
            {
                Time.timeScale = val;
            }
        }

        public void QuitGame()
        {
            Time.timeScale = 1;

            PlayerCam.ChangePlayerHasJoined();

#if KILLITMYSELF_FULL
            if (OnlineManager.instance.InOnlineGame)
            {
                ChatManager.instance.SetAllowChatFocusing(false);
                
                LoadingManagerOnline.ClearCurrentIPPref();

                OnlineManager.instance.InOnlineGame = false;
                OnlineManager.instance.Disconnecting = true;
            
                OnlineManager.instance.DisconnectAsHost();
                OnlineManager.instance.DisconnectAndLoadMainMenu();
                return;
            }

            LoadingManager.instance.LoadAddressableScene(SceneNames.S_MainMenu, SceneRefs.instance.MainMenu);
#elif UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#endif
        }

        public void EmergencyRespawn()
        {
            if (CurrentPlayer.instance.healthSystem.OnlineHealth.Value > 0)
            {
                CurrentPlayer.instance.playerMovement.rb.linearVelocity = Vector3.zero;
                CurrentPlayer.instance.playerMovement.transform.position = Vector3.zero;
                CurrentPlayer.instance.playerMovement.CloseShipOverrideCodeUI();
                CurrentPlayer.instance.healthSystem.OnlineHealth.Value = 0;
            }
        }

        private void Update()
        {
            if (Keyboard.current.f6Key.wasPressedThisFrame)
            {
                Instantiate(BotPlayerPrefab);
            }
        }

        private void OnDestroy()
        {
            if (SettingsRootBundle)
            {
                SettingsRootBundle.Unload(true);
            }
            if (SettingsRootSharedBundle)
            {
                SettingsRootSharedBundle.Unload(true);
            }
            
            instance = null;
        }
    }
}