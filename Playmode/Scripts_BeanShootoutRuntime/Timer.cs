using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using SerialPackage.Runtime;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    public class Timer : NetworkBehaviour
    {
        [SerializeField] private GameObject PlayerInputManagerObject;
        
        [SerializeField] private GameObject AmmoRoot;
        
        [SerializeField] private GameObject TimerRoot;
        [SerializeField] private TMP_Text TimerText;
        private int CurrentTime;
        private NetworkVariable<int> CurrentTimeOnline = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        [SerializeField] private GameObject TimeUpText;
        [SerializeField] private CanvasGroup TimeUpTextCG;

        [SerializeField] private Ease TimeUpEase;
        [SerializeField] private float TimeUpDuration;
        [SerializeField] private float TimeUpWait;
        
        [SerializeField] private AudioClip TimerAlmostUpSound;

        [SerializeField] private CanvasGroup ReturnToMenuButton;
        [SerializeField] private CanvasGroup WaitingForHostText;
        [SerializeField] private LocalizedString ReturnToLobbyText;

        [SerializeField] private GameObject PlayerModel;
        [SerializeField] private Vector3 PlayerModelSpawnLocalPosition;
        [SerializeField] private Vector3 PlayerModelSpawnLocalRotation;

        private WinnersObject winnerObject;

        [SerializeField] private AssetReference WinnersGenericLevelRef;
        [SerializeField] private AssetReferenceGameObject Winners_ArcadeLevel;
        [SerializeField] private AssetReferenceGameObject Winners_HotelLevel;
        [SerializeField] private AssetReferenceGameObject Winners_Level1Test;
        [SerializeField] private AssetReferenceGameObject Winners_MoonbaseBeta;
        [SerializeField] private AssetReferenceGameObject Winners_ShipLevel;
        [SerializeField] private AssetReferenceGameObject Winners_Generic;

        private AsyncOperationHandle<GameObject> winnersAB;
        
        private void Start()
        {
            if (GameSettings.Timer)
            {
                if (OnlineManager.instance.InOnlineGame && IsHost)
                {
                    CurrentTimeOnline.Value = GameSettings.Time;
                }
                
                TimerRoot.SetActive(true);
            }
            else
            {
                TimerRoot.SetActive(false);
                return;
            }
            
            CurrentTime = GameSettings.Time;
            TimeSpan time = TimeSpan.FromSeconds(CurrentTime);
            
            TimerText.text = time.ToString(@"mm\:ss");
            
            TimerAsync().Forget();
        }

        private async UniTaskVoid TimerAsync()
        {
            await UniTask.WaitForSeconds(1);

            if (!TimerText)
            {
                return;
            }

            if (CurrentTime <= 5)
            {
#if KILLITMYSELF_FULL
                SoundManager.PlaySound2(TimerAlmostUpSound);
#endif
            }

            if (CurrentTime <= 0)
            {
                if (OnlineManager.instance.InOnlineGame && !IsHost)
                {
                    return;
                }
                
                if (!OnlineManager.instance.InOnlineGame)
                {
                    foreach (var player in PlayersJoined.instance.Players)
                    {
                        PlayerMovement pm =  player.GetComponent<PlayerMovement>();
                        pm.PreventPlayerFromDoingAnything();
                    }
                }

                if (!OnlineManager.instance.InOnlineGame)
                {
                    await ShowTimerUp();
                    await LoadWinners();
                    await ShowWinners();
                }
                else
                {
                    DoItAllRpc();
                }
                
                return;
            }

            CurrentTime--;

            if (OnlineManager.instance.InOnlineGame && IsHost)
            {
                CurrentTimeOnline.Value--;
            }

            TimeSpan time;
            if (OnlineManager.instance.InOnlineGame)
            {
                time = TimeSpan.FromSeconds(CurrentTimeOnline.Value);
            }
            else
            {
                time = TimeSpan.FromSeconds(CurrentTime);
            }
            
            TimerText.text = time.ToString(@"mm\:ss");
            
            TimerAsync().Forget();
        }

        private void ReturnToMenu()
        {
            if (IsHost)
            {
                return;
            }
            
#if KILLITMYSELF_FULL
            LoadingManager.instance.LoadAddressableScene(SceneNames.S_MainMenu, SceneRefs.instance.MainMenu);
#endif
        }

        [Rpc(SendTo.Everyone)]
        private void DoItAllRpc()
        {
            DoItAllAsync().Forget();
        }

        private async UniTaskVoid DoItAllAsync()
        {
            await ShowTimerUp();
            await LoadWinners();
            await ShowWinners();
        }

        private async UniTask ShowTimerUp()
        {
            PauseManager.instance.CanPause = false;
            
            if (OnlineManager.instance.InOnlineGame)
            {
                CurrentPlayer.instance.playerMovement.PreventPlayerFromDoingAnything();
            }
                
            Vector3 initalScale = new(5, 5, 5);
                
            TimeUpText.SetActive(true);
            TimeUpTextCG.alpha = 0;
            TimeUpText.transform.localScale = initalScale;
            
#pragma warning disable CS4014
            LMotion.Create(initalScale, Vector3.one, TimeUpDuration)
                .WithEase(TimeUpEase)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalScale(TimeUpText.transform);
#pragma warning restore CS4014

            await LMotion.Create(0f, 1f, TimeUpDuration)
                .WithEase(TimeUpEase)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .Bind(x => TimeUpTextCG.alpha = x);

            await UniTask.WaitForSeconds(TimeUpWait);

            Cursor.lockState = CursorLockMode.None;

#if KILLITMYSELF_FULL
            await LoadingManager.instance.ShowLoadingScreen(true);
#endif
                
            TimeUpText.SetActive(false);
            TimerRoot.SetActive(false);
            AmmoRoot.SetActive(false);

            if (PlayerInputManagerObject)
            {
                PlayerInputManagerObject.SetActive(false);
            }

            if (GameObject.Find("LevelBase_LevelIndependent"))
            {
                GameObject.Find("LevelBase_LevelIndependent").SetActive(false);
            }
        }
        
        private async UniTask LoadWinners()
        {
            AssetReferenceGameObject abRef;
            bool loadGenericLevelScene = false;
            
#if KILLITMYSELF_FULL
            if (SceneManager.GetActiveScene().name.StartsWith(SceneNames.S_ArcadeLevel))
            {
                abRef = Winners_ArcadeLevel;
            }
            else if (SceneManager.GetActiveScene().name.StartsWith(SceneNames.S_SkyHotelLevel))
            {
                abRef = Winners_HotelLevel;
            }
            else if (SceneManager.GetActiveScene().name.StartsWith(SceneNames.S_StupidLevel))
            {
                abRef = Winners_Level1Test;
            }
            else if (SceneManager.GetActiveScene().name.StartsWith(SceneNames.S_MoonbaseBetaLevel))
            {
                abRef = Winners_MoonbaseBeta;
            }
            else if (SceneManager.GetActiveScene().name.StartsWith(SceneNames.S_ShipLevel))
            {
                abRef = Winners_ShipLevel;
            }
            else
            {
#endif
                loadGenericLevelScene = true;
                abRef = Winners_Generic;
#if KILLITMYSELF_FULL
            }
#endif

            try
            {
                if (loadGenericLevelScene)
                {
                    await Addressables.LoadSceneAsync(WinnersGenericLevelRef, LoadSceneMode.Additive);
                }
                
                winnersAB = Addressables.LoadAssetAsync<GameObject>(abRef);
                await winnersAB;
                winnerObject = Instantiate(winnersAB.Result).GetComponent<WinnersObject>();
            }
            catch (Exception e)
            {
                BeanLogger.LogError("Failed to load Winners AssetBundles!", this);
                Debug.LogException(e);

#if KILLITMYSELF_FULL
                if (OnlineManager.instance.InOnlineGame)
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.E_FailedToLoadResources.GetLocalizedStringAsync());
                }
                else
                {
                    await DialogManager.instance.ShowDialogAsync(DialogButtonType.OKButton, await LocalizedStringReferences.instance.E_Generic.GetLocalizedStringAsync(), await LocalizedStringReferences.instance.E_FailedToLoadResources.GetLocalizedStringAsync(), () => LoadingManager.instance.LoadAddressableScene(SceneNames.S_MainMenu, SceneRefs.instance.MainMenu));
                }
#endif
            }
        }

        private async UniTask ShowWinners()
        {
            float newY = -1.01f;

            winnerObject.FirstPed.position = new(winnerObject.FirstPed.position.x, newY, winnerObject.FirstPed.position.z);
            winnerObject.SecondPed.position = new(winnerObject.SecondPed.position.x, newY, winnerObject.SecondPed.position.z);
            winnerObject.ThirdPed.position = new(winnerObject.ThirdPed.position.x, newY, winnerObject.ThirdPed.position.z);
            
            List<PlayerMovement> pms = new();
            
            foreach (var player in PlayersJoined.instance.Players)
            {
                pms.Add(player.GetComponent<PlayerMovement>());
            }

            IOrderedEnumerable<PlayerMovement> a;
            if (OnlineManager.instance.InOnlineGame)
            {
                a = pms.OrderByDescending(x => x.KillsOnline.Value);
            }
            else
            {
                a = pms.OrderByDescending(x => x.username.Kills);
            }

            MeshRenderer firstPlayerRenderer = Instantiate(PlayerModel, winnerObject.FirstPed).transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer secondPlayerRenderer = Instantiate(PlayerModel, winnerObject.SecondPed).transform.GetChild(0).GetComponent<MeshRenderer>();
            MeshRenderer thirdPlayerRenderer = Instantiate(PlayerModel, winnerObject.ThirdPed).transform.GetChild(0).GetComponent<MeshRenderer>();

            if (SceneManager.GetSceneByName("WinnersGenericLevelScene").isLoaded)
            {
                firstPlayerRenderer.gameObject.layer = LayerMask.NameToLayer("WinnersGenericLevel");
                secondPlayerRenderer.gameObject.layer = LayerMask.NameToLayer("WinnersGenericLevel");
                thirdPlayerRenderer.gameObject.layer = LayerMask.NameToLayer("WinnersGenericLevel");
            }

            List<PlayerMovement> gos = a.ToList();
            
            MeshRenderer firstPlayerOriginalMR = gos[0].GetComponent<PlayerStartUI>().playerRenderer;

            List<Material> firstPlayerOriginalMats = new();
            firstPlayerOriginalMR.GetMaterials(firstPlayerOriginalMats);
            
            List<Material> firstPlayerMats = new();
            firstPlayerRenderer.GetMaterials(firstPlayerMats);
            
            firstPlayerMats[0].color = firstPlayerOriginalMats[0].color;
            firstPlayerMats[1].color = firstPlayerOriginalMats[1].color;

            firstPlayerRenderer.transform.localPosition = PlayerModelSpawnLocalPosition;
            firstPlayerRenderer.transform.localRotation = Quaternion.Euler(PlayerModelSpawnLocalRotation);

            BeanLogger.Log(gos.Count.ToString(), this);
            
            if (gos.Count >= 2)
            {
                MeshRenderer secondPlayerOriginalMR = gos[1].GetComponent<PlayerStartUI>().playerRenderer;
                
                List<Material> secondPlayerOriginalMats = new();
                secondPlayerOriginalMR.GetMaterials(secondPlayerOriginalMats);
            
                List<Material> secondPlayerMats = new();
                secondPlayerRenderer.GetMaterials(secondPlayerMats);
                
                secondPlayerMats[0].color = secondPlayerOriginalMats[0].color;
                secondPlayerMats[1].color = secondPlayerOriginalMats[1].color;
                
                secondPlayerRenderer.transform.localPosition = PlayerModelSpawnLocalPosition;
                secondPlayerRenderer.transform.localRotation = Quaternion.Euler(PlayerModelSpawnLocalRotation);
            }

            if (gos.Count >= 3)
            {
                MeshRenderer thirdPlayerOriginalMR = gos[2].GetComponent<PlayerStartUI>().playerRenderer;
                
                List<Material> thirdPlayerOriginalMats = new();
                thirdPlayerOriginalMR.GetMaterials(thirdPlayerOriginalMats);
            
                List<Material> thirdPlayerMats = new();
                thirdPlayerRenderer.GetMaterials(thirdPlayerMats);
                
                thirdPlayerMats[0].color = thirdPlayerOriginalMats[0].color;
                thirdPlayerMats[1].color = thirdPlayerOriginalMats[1].color;
                
                thirdPlayerRenderer.transform.localPosition = PlayerModelSpawnLocalPosition;
                thirdPlayerRenderer.transform.localRotation = Quaternion.Euler(PlayerModelSpawnLocalRotation);
            }

            foreach (var player in PlayersJoined.instance.Players)
            {
                player.transform.parent.gameObject.SetActive(false);
            }
            
#if KILLITMYSELF_FULL
            if (VRManager.instance.VREnabled)
            {
                VRManager.instance.GlobalXROrigin.transform.position = winnerObject.Camera.transform.position - new Vector3(0, 1.3f, 0);
                VRManager.instance.GlobalXROrigin.transform.rotation = Quaternion.Euler(0, winnerObject.Camera.transform.rotation.eulerAngles.y, 0);

                Vector3 newPos = winnerObject.Camera.transform.position + winnerObject.Camera.transform.forward + new Vector3(0, 0.25f, 0) * 2;
                Quaternion newRot = Quaternion.Euler(0, winnerObject.Camera.transform.rotation.eulerAngles.y, 0);
                
                VRDDOLCanvases.instance.ChangePosition(newPos);
                VRDDOLCanvases.instance.ChangeRotation(newRot);
                
                VRLevelBaseCanvas.instance.ChangePosition(newPos);
                VRLevelBaseCanvas.instance.ChangeRotation(newRot);
                
                winnerObject.Camera.SetActive(false);
            }

            await LoadingManager.instance.HideLoadingScreen(true);
#endif

            await UniTask.WaitForSeconds(1.42f);

            await LMotion.Create(-1.01f, 0.5f, 0.75f)
                .WithEase(Ease.Linear)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalPositionY(winnerObject.FirstPed);

            await LMotion.Create(-1.01f, 0f, 0.5f)
                .WithEase(Ease.Linear)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalPositionY(winnerObject.SecondPed);

            await LMotion.Create(-1.01f, -0.5f, 0.3f)
                .WithEase(Ease.Linear)
                .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                .BindToLocalPositionY(winnerObject.ThirdPed);

            await UniTask.WaitForSeconds(2f);

            if (OnlineManager.instance.InOnlineGame && !IsHost)
            {
                WaitingForHostText.gameObject.SetActive(true);
            
                await LMotion.Create(0f, 1f, 0.5f)
                    .WithEase(Ease.Linear)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => WaitingForHostText.alpha = x);

                return;
            }

            if (!OnlineManager.instance.InOnlineGame)
            {
                ReturnToMenuButton.gameObject.SetActive(true);
                
                Button button = ReturnToMenuButton.GetComponent<Button>();
                button.onClick.AddListener(ReturnToMenu);
            
                await LMotion.Create(0f, 1f, 0.5f)
                    .WithEase(Ease.Linear)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => ReturnToMenuButton.alpha = x);
            }
            
            if (OnlineManager.instance.InOnlineGame)
            {
                ReturnToMenuButton.gameObject.SetActive(true);

                ReturnToMenuButton.GetComponentInChildren<LocalizeStringEvent>().enabled = false;
                
                Button button = ReturnToMenuButton.GetComponent<Button>();
                button.onClick.AddListener(ReturnToLobby);
                
                ReturnToMenuButton.GetComponentInChildren<TMP_Text>().text = await ReturnToLobbyText.GetLocalizedStringAsync();
                
                await LMotion.Create(0f, 1f, 0.5f)
                    .WithEase(Ease.Linear)
                    .WithScheduler(MotionScheduler.TimeUpdateIgnoreTimeScale)
                    .Bind(x => ReturnToMenuButton.alpha = x);
            }
        }

        private void ReturnToLobby()
        {
            ReturnToLobbyRpc();
            
#if KILLITMYSELF_FULL
            ChatManager.instance.SetAllowChatFocusing(false);
            
            LoadingManagerOnline.ClearCurrentIPPref();
#endif
            
            foreach (var player in PlayersJoined.instance.Players)
            {
                Destroy(player.transform.parent.gameObject);
            }

            OnlineManager.instance.InOnlineGame = false;
            OnlineManager.instance.Host_InGame = false;
            OnlineManager.instance.Disconnecting = true;
            
            // OnlineManager.instance.DisconnectAsHost();
            // OnlineManager.instance.DisconnectAndLoadMainMenu();
            
            NetworkManager.Singleton.Shutdown();

#if KILLITMYSELF_FULL
            MultiplayerManager.StartHostOnStart = true;

            LoadingManager.instance.LoadAddressableScene(SceneNames.S_MainMenu, SceneRefs.instance.MainMenu);
#endif
        }

        [Rpc(SendTo.NotServer)]
        private void ReturnToLobbyRpc()
        {
            OnlineManager.instance.InOnlineGame = false;
            OnlineManager.instance.Disconnecting = true;
            
            NetworkManager.Singleton.Shutdown();

#if KILLITMYSELF_FULL
            MultiplayerManager.StartClientOnStart = true;
            
            LoadingManager.instance.LoadAddressableScene(SceneNames.S_MainMenu, SceneRefs.instance.MainMenu);
#endif
        }
        
#if UNITY_EDITOR || KILLITMYSELF_DEBUG
        private void Update()
        {
            if (Keyboard.current.leftBracketKey.wasPressedThisFrame)
            {
                if (OnlineManager.instance.InOnlineGame)
                {
                    CurrentTimeOnline.Value = 0;
                }
                
                CurrentTime = 0;
            }
        }
#endif
        
        private new void OnDestroy()
        {
            if (WinnersGenericLevelRef != null && WinnersGenericLevelRef.IsValid())
            {
                WinnersGenericLevelRef.UnLoadScene();
            }
            
            base.OnDestroy();
        }
    }
}