using System.Collections;
using SerialPackage.Runtime;
using TMPro;
using Unity.Netcode;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class BulletGlobalOnline : NetworkBehaviour
    {
        public static BulletGlobalOnline instance;

        public NetworkVariable<int> Bullets = new();
        public NetworkVariable<bool> Reloading = new();

        [SerializeField] private TMP_Text BulletsText;
        [SerializeField] private GameObject BulletsRoot;

        [SerializeField] private GameObject Bullet;

        private void Awake()
        {
            instance = this;

            if (!GameSettings.SharedAmmo)
            {
                BulletsRoot.SetActive(false);
                return;
            }

            if (!IsServer)
            {
                return;
            }

            Bullets.Value = GameSettings.MaxAmmo;
        }

        private void Update()
        {
            if (!GameSettings.SharedAmmo)
            {
                return;
            }

            BulletsText.text = Bullets.Value.ToString();

            if (!IsServer)
            {
                return;
            }

            if (Bullets.Value <= 0 && !Reloading.Value)
            {
                BulletReloadRpc();
            }
        }

        [Rpc(SendTo.Server)]
        private void BulletReloadRpc()
        {
            StartCoroutine(BulletReload());
            Reloading.Value = true;
        }

        [Rpc(SendTo.Server)]
        public void ReduceBulletCountRpc(int val)
        {
            if (!IsServer)
            {
                return;
            }

            Bullets.Value -= val;
        }

        [Rpc(SendTo.Server, AllowTargetOverride = false)]
        public void SpawnBulletRpc(Vector3 Pos, Quaternion Rot, int Damage, bool ShootBackwards, RpcParams rpcParams)
        {
            if (CommandLineArgs.VerboseLoggingEnabled)
            {
                BeanLogger.Log("Client " + rpcParams.Receive.SenderClientId + " spawned a bullet at pos " + Pos + " rot " + Rot.eulerAngles + " damage " + Damage + " backwards " + ShootBackwards, this);
            }
            if (!IsServer)
            {
                return;
            }

            ulong clientId = rpcParams.Receive.SenderClientId;

            if (BetterPrefs.GetBool("HostSettings_CheatDetection", true) && clientId != NetworkManager.Singleton.NetworkConfig.NetworkTransport.ServerClientId)
            {
                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.GetComponent<PlayerBaseRootOnlineUsername>().ActualPlayer.Value.TryGet(out var player);
                
                if (Damage > 35 || Vector3.Distance(Pos, player.transform.GetChild(0).position) > 3)
                {
                    NetworkManager.Singleton.DisconnectClient(clientId, "AntiCheatKick");
                }
            }

            GameObject bulletGO = Instantiate(Bullet, Pos, Rot);

            bulletGO.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);

            bulletGO.GetComponent<BulletMove>().ShootBackwardsOnline.Value = ShootBackwards;
            bulletGO.GetComponent<BulletMove>().clientIdToNotShowOn.Value = clientId;
            // bulletGO.GetComponent<BulletMove>().damageOnline.Value = Damage;
        }

        private IEnumerator BulletReload()
        {
            yield return new WaitForSeconds(5f);
            Bullets.Value = GameSettings.MaxAmmo;
            Reloading.Value = false;
        }

        private new void OnDestroy()
        {
            Bullets.Dispose();
            Reloading.Dispose();

            instance = null;
        }
    }
}