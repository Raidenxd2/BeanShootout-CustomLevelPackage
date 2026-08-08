using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class BulletMove : NetworkBehaviour
    {
        private bool collisionEnabled;
        [SerializeField] private Rigidbody rb;

        public int damage;
        public bool ShootBackwards;

        private bool HasBeenDestroyed;

        public bool IsClientSide;

        public PlayerMovement from;

        public NetworkVariable<bool> ShootBackwardsOnline = new(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
        public NetworkVariable<ulong> clientIdToNotShowOn = new(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);

        private void Start()
        {
            EnableCollision().Forget();
            DestroyWait().Forget();
        }

        public void FixedUpdate()
        {
            if (IsClientSide)
            {
                if (ShootBackwards)
                {
                    rb.AddForce(-225f * -13 * -transform.forward.normalized, ForceMode.Force);
                }
                else
                {
                    rb.AddForce(225f * 13 * transform.forward.normalized, ForceMode.Force);
                }

                return;
            }

            if (OnlineManager.instance.InOnlineGame && clientIdToNotShowOn.Value == NetworkManager.Singleton.LocalClientId)
            {
                GetComponent<MeshRenderer>().enabled = false;
                GetComponent<BoxCollider>().enabled = false;
            }
            
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

            if (OnlineManager.instance.InOnlineGame && ShootBackwardsOnline.Value)
            {
                rb.AddForce(-225f * -13 * -transform.forward.normalized, ForceMode.Force);
            }
            else if (ShootBackwards || (OnlineManager.instance.InOnlineGame && ShootBackwardsOnline.Value))
            {
                rb.AddForce(-225f * -13 * -transform.forward.normalized, ForceMode.Force);
            }
            else
            {
                rb.AddForce(225f * 13 * transform.forward.normalized, ForceMode.Force);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collisionEnabled)
            {
                return;
            }
            
            if (collision.gameObject.CompareTag("Player"))
            {
                if (collision.gameObject.GetComponent<PlayerMovement>() == from)
                {
                    return;
                }
                
                if (OnlineManager.instance.InOnlineGame)
                {
                    // HitRpc(collision);
                    HasBeenDestroyed = true;

                    if (IsClientSide)
                    {
                        collision.gameObject.GetComponent<HealthSystem>().DamageRpc(damage, NetworkManager.Singleton.LocalClientId);
                        
                        Destroy(gameObject);
                    }
                    else
                    {
                        DespawnObjectRpc();
                    }
                }
                else
                {
                    HasBeenDestroyed = true;
                    collision.gameObject.GetComponent<HealthSystem>().Damage(damage, from);
                    
                    Destroy(gameObject);
                }
            }
#if KILLITMYSELF_FULL
            else if (collision.gameObject.CompareTag("Bossfight/JumbotronScreen"))
            {
                HasBeenDestroyed = true;
                if (OnlineManager.instance.InOnlineGame)
                {
                    if (IsClientSide)
                    {
                        BossfightAttacks.instance.DamageRpc(damage);
                    }
                }
                else
                {
                    BossfightAttacks.instance.Health -= damage;
                }

                if (OnlineManager.instance.InOnlineGame)
                {
                    if (IsClientSide)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        DespawnObjectRpc();
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            }
#endif
            else
            {
                if (OnlineManager.instance.InOnlineGame)
                {
                    HasBeenDestroyed = true;

                    if (IsClientSide)
                    {
                        Destroy(gameObject);
                    }
                    else
                    {
                        DespawnObjectRpc();
                    }
                }
                else
                {
                    HasBeenDestroyed = true;
                    Destroy(gameObject);
                }
            }
        }

        //[Rpc(SendTo.Server)]
        //public void HitRpc(Collision collision)
        //{
        //    Debug.Log("HitRpc");
        //    collision.gameObject.GetComponent<HealthSystem>().DamageRpc(damage);
        //    DespawnObjectRpc();
        //}

        [Rpc(SendTo.Server)]
        public void DespawnObjectRpc()
        {
            GetComponent<NetworkObject>().Despawn();
        }

        private async UniTask DestroyWait()
        {
            await UniTask.WaitForSeconds(2f);
            if (HasBeenDestroyed)
            {
                return;
            }
            
            if (OnlineManager.instance.InOnlineGame)
            {
                if (this)
                {
                    if (IsClientSide)
                    {
                        Destroy(gameObject);
                    }
                    else if (IsSpawned)
                    {
                        GetComponent<NetworkObject>().Despawn();
                    }
                }
            }
            else
            {
                if (this)
                {
                    Destroy(gameObject);
                }
            }
        }

        private async UniTask EnableCollision()
        {
            await UniTask.WaitForSeconds(0.03f);
            collisionEnabled = true;
        }
    }
}