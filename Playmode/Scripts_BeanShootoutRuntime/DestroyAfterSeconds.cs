using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class DestroyAfterSeconds : MonoBehaviour
    {
        [SerializeField] private float timeToDestroy;
        [SerializeField] private bool allowOnline;

        private void Start()
        {
            if (OnlineManager.instance.InOnlineGame && allowOnline && NetworkManager.Singleton.IsServer)
            {
                StartAsync().Forget();
            }
            else
            {
                Destroy(gameObject, timeToDestroy);
            }
        }

        private async UniTaskVoid StartAsync()
        {
            await UniTask.WaitForSeconds(timeToDestroy);
            GetComponent<NetworkObject>().Despawn();
        }
    }
}