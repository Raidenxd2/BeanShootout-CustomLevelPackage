using System;
using Cysharp.Threading.Tasks;
using SerialPackage.Runtime;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class OnlineManager : MonoBehaviour
    {
        public static OnlineManager instance;

        public bool InOnlineGame;
        public bool Host_InGame;

        public bool Connecting;
        public bool Disconnecting;
        private bool Quitting;

        public bool IgnoreErrors;

        private void Awake()
        {
            instance = this;

            Application.wantsToQuit += WantsToQuit;
        }

        private bool WantsToQuit()
        {
            if (Application.isEditor)
            {
                return true;
            }
            
            if (Quitting)
            {
                return false;
            }
            
            if (InOnlineGame && NetworkManager.Singleton.IsServer)
            {
                Quitting = true;

#if KILLITMYSELF_FULL
                WantsToQuitAsync().Forget();
#endif
                
                return false;
            }
            else
            {
                return true;
            }
        }

#if KILLITMYSELF_FULL
        private async UniTaskVoid WantsToQuitAsync()
        {
            try
            {
                await LoadingManager.instance.ShowLoadingScreen(true);

                for (int i = 0; i < 15; i++)
                {
                    DisconnectPlayersRpc();
                    await UniTask.WaitForSeconds(0.25f);
                    
                    BeanLogger.Log(NetworkManager.Singleton.ConnectedClients.Count.ToString(), this);

                    if (NetworkManager.Singleton.ConnectedClients.Count < 2)
                    {
                        break;
                    }
                }
                
                NetworkManager.Singleton.Shutdown();

                Quitting = false;
                Disconnecting = true;
                Connecting = false;
                Host_InGame = false;

                Application.Quit();
            }
            catch (Exception ex)
            {
                BeanLogger.LogError("Failed to properly disconnect all players!", this);
                Debug.LogException(ex);

                Quitting = false;
                InOnlineGame = false;
                
                Application.Quit();
            }
        }

        public void InitNetworking()
        {
            NetworkManager.Singleton.OnTransportFailure += OnTransportFailure;
            NetworkManager.Singleton.OnConnectionEvent += OnConnectionEvent;

            NetworkManager.Singleton.OnClientStarted += OnClientStarted;
            NetworkManager.Singleton.OnClientStopped += OnClientStopped;

            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        }

        private void OnClientStarted()
        {
            NetworkManager.Singleton.ConnectionManager.OnDisconnect2 += OnDisconnect;
        }

        private void OnClientStopped(bool obj)
        {
            NetworkManager.Singleton.ConnectionManager.OnDisconnect2 -= OnDisconnect;
        }

        private void OnTransportFailure()
        {
            if (IgnoreErrors)
            {
                return;
            }

            OnTransportFailureAsync().Forget();
        }

        private async UniTaskVoid OnTransportFailureAsync()
        {
            Destroy(GameObject.Find("PlayerInput(Clone)"));

            string errorString = await LocalizedStringReferences.instance.E_Online_Transport.GetLocalizedStringAsync();

            if (!string.IsNullOrEmpty(UnityTransportPrevLog.prevLog))
            {
                errorString += "\n<size=25>" + UnityTransportPrevLog.prevLog + "</size>";
                UnityTransportPrevLog.prevLog = null;
            }
#if !UNITY_WEBGL
            if (!string.IsNullOrEmpty(DebugLogPrev.prevLog))
            {
                errorString += "\n<size=25>" + DebugLogPrev.prevLog + "</size>";
                DebugLogPrev.prevLog = null;
            }
#endif
            
            Connecting = false;

            NetworkErrorManager.instance.ShowErrorAndDisconnect(errorString);
        }

        public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            Uri uri = new("http://" + NetworkManager.Singleton.GetComponent<UnityTransport>().GetEndpoint(request.ClientNetworkId).Address);
            
            if (CommandLineArgs.VerboseLoggingEnabled)
            {
                BeanLogger.Log("Client IP " + uri.Host + " is connecting!", this);
            }

            foreach (var player in CurrentBannedPlayers.current.players)
            {
                if (player.unique == request.Payload.ToString() || player.ip == uri.Host)
                {
                    response.Approved = false;
                    response.Reason = "Banned";
                    return;
                }
            }

            if (Host_InGame)
            {
                response.Approved = false;
                response.Reason = "InProgress";
                return;
            }

            response.CreatePlayerObject = true;
            response.PlayerPrefabHash = null;
            response.Position = Vector3.zero;
            response.Rotation = Quaternion.identity;
            response.Approved = true;
            response.Pending = false;
        }

        public void DisconnectAsHost()
        {
            if (!NetworkManager.Singleton.IsServer)
            {
                return;
            }

            DisconnectPlayersRpc();
        }

        [Rpc(SendTo.Everyone)]
        private void DisconnectPlayersRpc()
        {
            if (NetworkManager.Singleton.IsServer)
            {
                return;
            }

            BeanLogger.LogWarning("Host left.", this);

            Destroy(GameObject.Find("PlayerInput(Clone)"));

            HostLeftDialogAsync(true).Forget();
        }

        public void DisconnectAndLoadMainMenu()
        {
            Disconnecting = true;
            Connecting = false;
            Host_InGame = false;

            NetworkManager.Singleton.Shutdown();

            Destroy(GameObject.Find("PlayerInput(Clone)"));

            LoadingManager.instance.LoadAddressableScene(SceneNames.S_MainMenu, SceneRefs.instance.MainMenu);
        }

        private void OnConnectionEvent(NetworkManager manager, ConnectionEventData data)
        {
            if (IgnoreErrors)
            {
                return;
            }

            if (Disconnecting)
            {
                Disconnecting = false;
                return;
            }
            
            BeanLogger.Log("OnConnectionEvent for " + data.ClientId + " (" + data.EventType + ")", this);

            if (data.EventType == ConnectionEvent.ClientDisconnected && NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                if (data.ClientId == 0)
                {
                    BeanLogger.LogWarning("Host left.", this);

                    HostLeftDialogAsync().Forget();
                }
            }
        }

        private void OnDisconnect(ulong arg0)
        {
            if (IgnoreErrors)
            {
                return;
            }

            if (Disconnecting)
            {
                Disconnecting = false;
                return;
            }
            
            if (NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                if (arg0 == 0)
                {
                    BeanLogger.LogWarning("Host left.", this);

                    HostLeftDialogAsync().Forget();
                }
            }
        }

        private async UniTaskVoid HostLeftDialogAsync(bool forceDisconnect = false)
        {
            if (IgnoreErrors && !forceDisconnect)
            {
                return;
            }

            if (ServerTick.instance)
            {
                ServerTick.instance.Stop();
            }

            Destroy(GameObject.Find("PlayerInput(Clone)"));
            
            BeanLogger.Log(NetworkManager.Singleton.DisconnectReason, this);
            
            if (!string.IsNullOrEmpty(NetworkManager.Singleton.DisconnectReason))
            {
                if (NetworkManager.Singleton.DisconnectReason.Equals("SyncError"))
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.Online_FailedToSyncData.GetLocalizedStringAsync());
                }
                else if (NetworkManager.Singleton.DisconnectReason.Equals("AntiCheatKick"))
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.Online_AntiCheatKick.GetLocalizedStringAsync());
                }
                else if (NetworkManager.Singleton.DisconnectReason.Equals("VRNotAllowed"))
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.Online_VRNotAllowed.GetLocalizedStringAsync());
                }
                else if (NetworkManager.Singleton.DisconnectReason.Equals("Banned"))
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.Online_Banned.GetLocalizedStringAsync());
                }
                else
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(NetworkManager.Singleton.DisconnectReason + "\n\nReturning to main menu...");
                }
            }
            else
            {
                if (Connecting)
                {
                    NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.Online_FailedToConnect.GetLocalizedStringAsync());
                    return;
                }
                
                NetworkErrorManager.instance.ShowErrorAndDisconnect(await LocalizedStringReferences.instance.Online_HostLeft.GetLocalizedStringAsync());
            }
        }
#endif

        private void OnDestroy()
        {
#if KILLITMYSELF_FULL
            if (NetworkManager.Singleton)
            {
                NetworkManager.Singleton.OnTransportFailure -= OnTransportFailure;
                NetworkManager.Singleton.OnConnectionEvent -= OnConnectionEvent;

                NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
                NetworkManager.Singleton.OnClientStopped -= OnClientStopped;

                NetworkManager.Singleton.ConnectionApprovalCallback = null;
            }
#endif

            instance = null;
        }
    }
}