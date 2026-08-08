using System;
using System.Collections;
using Cysharp.Threading.Tasks;
using SerialPackage.Runtime;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KillItMyself.Runtime
{
    public class HealthSystem : NetworkBehaviour
    {
        [SerializeField] private Image HealthBar;
        [SerializeField] private Image ClientHealthBar;
        [SerializeField] private GameObject DeadUI;
        [SerializeField] private GameObject DeadExplosion;
        [SerializeField] private Animator FadeAnim;

        [SerializeField] private GameObject PlayerCamera;
        [SerializeField] private GameObject PlayerModelDamage;

        [SerializeField] private PlayerInput playerInput;

        [SerializeField] private LayerMask layerMask;

        private bool Dead;
        
        private PlayerMovement playerMovement;
        private Rigidbody playerRb;

        public int Health = 100;
        private int PreviousHealth = 100;

        [SerializeField] private TMP_Text DeathQuoteText;
        [SerializeField] private string[] DeathQuotes;

        private bool StopGoingUp;
        private bool CanRespawn;

        [SerializeField] private PlayerInput playerControls;

        [SerializeField] private Transform ControllerButtonsParent;
        [SerializeField] private GameObject XboxControllerButtons;
        [SerializeField] private GameObject PlayStationControllerButtons;

        private LayerMask OldLayerMask;

        public NetworkVariable<int> OnlineHealth = new(100, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        private bool DoBusDamage = true;

        private bool GodMode;

#if KILLITMYSELF_FULL
        [SerializeField] private AchievementSO KillABeanAchievement;
        [SerializeField] private AchievementSO GenocideAchievement;
#endif

        [SerializeField] private GameObject SpectateRoot;
        [SerializeField] private GameObject UIRoot;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        private void Start()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

            playerRb = playerMovement.GetComponent<Rigidbody>();

            try
            {
                if (playerControls.devices[0].displayName.Contains("Xbox"))
                {
                    Instantiate(XboxControllerButtons, ControllerButtonsParent);
                }
                if (playerControls.devices[0].displayName.Contains("DualSense") || playerControls.devices[0].displayName.Contains("DualShock"))
                {
                    Instantiate(PlayStationControllerButtons, ControllerButtonsParent);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void Update()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                ClientHealthBar.fillAmount = OnlineHealth.Value / 100f;
                return;
            }
            
            if (OnlineManager.instance.InOnlineGame)
            {
                HealthBar.fillAmount = OnlineHealth.Value / 100f;

                if (OnlineHealth.Value <= 0)
                {
                    playerMovement.bulletManager.CannotShootNoMatterWhat = true;
                    
                    if (!Dead)
                    {
                        StartCoroutine(DeadWait());
                        playerMovement.GetComponent<Rigidbody>().detectCollisions = false;
                        SpawnDeadExplosionRpc();
                        
                        if (CurrentPlayer.instance.playerMovement.hasPsGamepad)
                        {
                            CurrentPlayer.instance.playerMovement.psGamepad.SetLightBarColor(Color.red);
                        }
                    }

                    DeadUI.SetActive(true);

                    Dead = true;
                    
                    if (playerMovement.JumpInput.WasPressedThisFrame() && CanRespawn)
                    {
                        ReAlive();
                    }

                    if (!StopGoingUp)
                    {
                        playerRb.linearVelocity = Vector3.zero;

                        playerRb.position += new Vector3(0, 40f * Time.deltaTime, 0);
                    }
                }

                if (Dead)
                {
                    return;
                }

                if (PreviousHealth != OnlineHealth.Value)
                {
                    PreviousHealth = OnlineHealth.Value;
                    DamageEffectRpc();
                }
            }
            else
            {
                HealthBar.fillAmount = Health / 100f;

                if (Health <= 0)
                {
                    if (!Dead)
                    {
                        StartCoroutine(DeadWait());
                        playerMovement.GetComponent<Rigidbody>().detectCollisions = false;
                        Instantiate(DeadExplosion, transform.position, Quaternion.identity);
                        
                        if (playerMovement.hasPsGamepad)
                        {
                            playerMovement.psGamepad.SetLightBarColor(Color.red);
                        }
                    }

                    DeadUI.SetActive(true);

                    Dead = true;

                    if (playerMovement.JumpInput.WasPressedThisFrame() && CanRespawn)
                    {
                        ReAlive();
                    }
                    
                    if (StopGoingUp == false)
                    {
                        playerRb.linearVelocity = Vector3.zero;

                        playerRb.position += new Vector3(0, 40f * Time.deltaTime, 0);
                    }
                }

                if (Dead)
                {
                    return;
                }

                if (PreviousHealth != Health)
                {
                    PreviousHealth = Health;
                    DamageEffect().Forget();
                }
            }
#if UNITY_EDITOR || KILLITMYSELF_DEBUG
            if (Keyboard.current.semicolonKey.wasPressedThisFrame)
            {
                GodMode = !GodMode;
                if (GodMode)
                {
                    Health = 999999999;
                }
                else
                {
                    Health = 100;
                }
            }
#endif
        }

        [Rpc(SendTo.Server)]
        private void SpawnDeadExplosionRpc()
        {
            Instantiate(DeadExplosion, transform.position, Quaternion.identity).GetComponent<NetworkObject>().Spawn();
        }

        [Rpc(SendTo.Owner)]
        public void DamageRpc(int damageVal, ulong fromId)
        {
            if (Dead)
            {
                return;
            }
            
            OnlineHealth.Value -= damageVal;

            if (OnlineHealth.Value <= 0)
            {
                KillRpc(RpcTarget.Single(fromId, RpcTargetUse.Temp));
            }
        }

        public void Damage(int damageVal, PlayerMovement from)
        {
            if (Dead)
            {
                return;
            }

            Health -= damageVal;

            if (Health <= 0)
            {
                Kill(from);
            }
        }

        [Rpc(SendTo.SpecifiedInParams)]
        private void KillRpc(RpcParams rpcParams)
        {
            if (CommandLineArgs.VerboseLoggingEnabled)
            {
                BeanLogger.Log("Killed player", this);
            }
            
#if KILLITMYSELF_FULL
            AchievementManager.instance.GrantAchievement(KillABeanAchievement);

            BetterPrefs.SetInt("Kills", BetterPrefs.GetInt("Kills", 0) + 1);

            if (BetterPrefs.GetInt("Kills", 0) >= 50 && !BetterPrefs.GetBool("Genocide", false))
            {
                AchievementManager.instance.GrantAchievement(GenocideAchievement);
                BetterPrefs.Save();
            }
            
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerBaseRootOnlineUsername>().Kills.Value++;
            CurrentPlayer.instance.playerMovement.KillsOnline.Value++;
#endif
        }

        private void Kill(PlayerMovement from)
        {
            if (from == playerMovement)
            {
                return;
            }
            
            from.Kills++;
        }

        public void BusDamage()
        {
            if (DoBusDamage)
            {
                DoBusDamage = false;
                Health -= 80;
            }
        }

        public async UniTaskVoid EnableBusDamage()
        {
            await UniTask.WaitForSeconds(5);
            
            DoBusDamage = true;
        }

        public async UniTaskVoid DamageEffect()
        {
            if (OnlineManager.instance.InOnlineGame && IsOwner)
            {
                if (CurrentPlayer.instance.playerMovement.hasPsGamepad)
                {
                    CurrentPlayer.instance.playerMovement.psGamepad.SetLightBarColor(Color.red);
                }
            }
            else
            {
                if (playerMovement.hasPsGamepad)
                {
                    playerMovement.psGamepad.SetLightBarColor(Color.red);
                }
            }
            
            DeadUI.SetActive(true);
            if (!IsOwner)
            {
                PlayerModelDamage.SetActive(true);
            }
            await UniTask.WaitForSeconds(0.1f);
            if (!IsOwner)
            {
                PlayerModelDamage.SetActive(false);
            }
            DeadUI.SetActive(false);

            if (OnlineManager.instance.InOnlineGame && IsOwner)
            {
                if (CurrentPlayer.instance.playerMovement.hasPsGamepad && Health > 0)
                {
                    CurrentPlayer.instance.playerMovement.psGamepad.SetLightBarColor(Color.blue);
                }
            }
            else
            {
                if (playerMovement.hasPsGamepad && Health > 0)
                {
                    playerMovement.psGamepad.SetLightBarColor(Color.blue);
                }
            }
        }

        [Rpc(SendTo.Everyone)]
        private void DamageEffectRpc()
        {
            DamageEffect().Forget();
        }

        public void ReAlive()
        {
            if (OnlineManager.instance.InOnlineGame && IsOwner)
            {
                if (CurrentPlayer.instance.playerMovement.hasPsGamepad)
                {
                    CurrentPlayer.instance.playerMovement.psGamepad.SetLightBarColor(Color.blue);
                }
            }
            else
            {
                if (playerMovement.hasPsGamepad)
                {
                    playerMovement.psGamepad.SetLightBarColor(Color.blue);
                }
            }
            
            Dead = false;
            
            if (OnlineManager.instance.InOnlineGame)
            {
                OnlineHealth.Value = 100;
            }
            else
            {
                Health = 100;
            }

            playerMovement.LetPlayerDoAnything();

            if (playerMovement.IsOnKeyboardMouse)
            {
                Cursor.lockState = CursorLockMode.Locked;
            }

            PauseManager.instance.prevCursorLock = CursorLockMode.Locked;

            PlayerCamera.GetComponent<Camera>().cullingMask = OldLayerMask;

            playerRb.detectCollisions = true;
            playerRb.useGravity = true;

            StopGoingUp = false;
            CanRespawn = false;

            if (SpawnManager.instance)
            {
                playerRb.position = SpawnManager.instance.SpawnPoints[Random.Range(0, SpawnManager.instance.SpawnPoints.Length)].position;
            }
            else
            {
                playerRb.position = Vector3.zero;
            }

            FadeAnim.Play("PlayerYouWEODied_Reset");
            
            FadeAnim.gameObject.SetActive(false);
            PlayerModelDamage.SetActive(false);
            DeadUI.SetActive(false);
        }

        private IEnumerator DeadWait()
        {
#if KILLITMYSELF_FULL
            if (!OnlineManager.instance.InOnlineGame)
            {
                AchievementManager.instance.GrantAchievement(KillABeanAchievement);

                BetterPrefs.SetInt("Kills", BetterPrefs.GetInt("Kills", 0) + 1);

                if (BetterPrefs.GetInt("Kills", 0) >= 50 && !BetterPrefs.GetBool("Genocide", false))
                {
                    AchievementManager.instance.GrantAchievement(GenocideAchievement);
                    BetterPrefs.Save();
                }
            }
#endif

            DeathQuoteText.text = DeathQuotes[Random.Range(0, DeathQuotes.Length)];

            yield return new WaitForSeconds(5f);
            FadeAnim.gameObject.SetActive(true);
            FadeAnim.Play("PlayerYouWEODied");

            yield return new WaitForSeconds(1.5f);
            playerMovement.PreventPlayerFromDoingAnything();

            PauseManager.instance.prevCursorLock = CursorLockMode.None;

            if (!GameSettings.AllowRespawning)
            {
                UIRoot.SetActive(false);
                SpectateRoot.SetActive(true);
                yield return null;
            }
            
            OldLayerMask = PlayerCamera.GetComponent<Camera>().cullingMask;
            PlayerCamera.GetComponent<Camera>().cullingMask = layerMask;

            playerRb.useGravity = false;

            StopGoingUp = true;
            CanRespawn = true;

            if (playerMovement.IsOnKeyboardMouse)
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }

        private new void OnDestroy()
        {
            if (OnlineManager.instance.InOnlineGame)
            {
                OnlineHealth.Dispose();
            }
        }
    }
}