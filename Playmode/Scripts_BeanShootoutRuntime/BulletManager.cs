using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    /// <summary>
    /// BulletManager is used by the player
    /// </summary>
    public class BulletManager : NetworkBehaviour
    {
        [SerializeField] private GameObject BulletPrefab;
        [SerializeField] private Transform BulletParent;
        [SerializeField] private GameObject GunShootParticle;

        [SerializeField] private Transform Player1Transform;
        [SerializeField] private Transform BulletOffset;
        [SerializeField] private Transform BulletOffsetBehind;
        [SerializeField] private Transform GunShootParticleOffset;

        [SerializeField] private PlayerInput playerControls;
        private InputAction ShootInput;
        private InputAction ReloadInput;

        public GunSO gun;
        [SerializeField] private Image gunVisual;

        [SerializeField] private Recoil recoil;

        [SerializeField] private PlayerAmmo playerAmmo;
        [SerializeField] private PlayerMovement playerMovement;
        
        private InputAction vr_rightTriggerInputAction = new(binding: "<XRController>{RightHand}/triggerPressed", expectedControlType: "Button");
        private InputAction vr_reloadInputAction = new(binding: "<XRController>{LeftHand}/secondaryButton", expectedControlType: "Button");

        /// <summary>
        /// CanShoot determines if we are reloading
        /// </summary>
        public bool CanShoot;
        /// <summary>
        /// CannotShootNoMatterWhat determines if we can shoot at all
        /// </summary>
        public bool CannotShootNoMatterWhat;

        private void Start()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }
            
#if KILLITMYSELF_FULL
            if (VRManager.instance.VREnabled && !VRManager.instance.FakeVR)
            {
                vr_rightTriggerInputAction.Enable();
                vr_reloadInputAction.Enable();
                ShootInput = vr_rightTriggerInputAction;
                ReloadInput = vr_reloadInputAction;

                BulletOffset = VRReferences.instance.RightController.transform;
            }
            else
            {
#endif
                UpdateValues();
#if KILLITMYSELF_FULL
            }
#endif
        }

        public void UpdateValues()
        {
#if KILLITMYSELF_FULL
            if (playerMovement.IsOnKeyboardMouse)
            {
                ShootInput = CurrentBindings.instance.ShootAction;
                ReloadInput = CurrentBindings.instance.ReloadAction;
            }
            else
            {
#endif
                ShootInput = playerControls.actions["Shoot"];
                ReloadInput = playerControls.actions["Reload"];
#if KILLITMYSELF_FULL
            }
#endif
        }

        private void Update()
        {
            if (!gun)
            {
                return;
            }
            
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

            if (ReloadInput.WasPressedThisFrame() && !GameSettings.SharedAmmo)
            {
                if (!PauseManager.instance.paused)
                {
                    if (!playerAmmo.Reloading && !CanShoot)
                    {
                        playerAmmo.BulletReload().Forget();
                        playerAmmo.Reloading = true;
                    }
                }
            }

            if (ShootInput.WasPressedThisFrame() && !gun.HoldToShoot || ShootInput.IsPressed() && gun.HoldToShoot)
            {
                Shoot();
            }
            
#if UNITY_EDITOR || KILLITMYSELF_DEBUG
            if (Keyboard.current.f9Key.wasPressedThisFrame)
            {
                Shoot(true);
            }
            if (Keyboard.current.f10Key.wasPressedThisFrame)
            {
                gun.Damage = 999;
            }
#endif
        }

        /// <summary>
        /// Sets the gun image to the one in this.gun
        /// </summary>
        public void BulletManagerInit()
        {
            gunVisual.sprite = gun.Image;
        }

#if UNITY_EDITOR || KILLITMYSELF_DEBUG
        private void Shoot(bool cheat_forceshootatfardistance = false)
#else
        private void Shoot()
#endif
        {
            if (PauseManager.instance.paused)
            {
                return;
            }

            // If were in an online game, check the online BulletGlobal if were reloading, else check the local BulletGlobal
            if (OnlineManager.instance.InOnlineGame && GameSettings.SharedAmmo)
            {
                if (BulletGlobalOnline.instance.Reloading.Value || !CanShoot)
                {
#if KILLITMYSELF_FULL
                    if (!gun.HoldToShoot)
                    {
                        SoundManager.PlaySound2(ResourcesReferences.NoAmmo);
                    }
#endif
                    
                    return;
                }
            }
            else if (!GameSettings.SharedAmmo)
            {
                if (playerAmmo.Reloading || !CanShoot)
                {
#if KILLITMYSELF_FULL
                    if (!gun.HoldToShoot)
                    {
                        SoundManager.PlaySound2(ResourcesReferences.NoAmmo);
                    }
#endif

                    return;
                }
            }
            else if (BulletGlobal.instance.Reloading || !CanShoot)
            {
#if KILLITMYSELF_FULL
                if (!gun.HoldToShoot)
                {
                    SoundManager.PlaySound2(ResourcesReferences.NoAmmo);
                }
#endif

                return;
            }

            if (CannotShootNoMatterWhat)
            {
                return;
            }

            // If were in an online game, Reduce the amount of bullets for the online BulletGlobal, else reduce the amont of bullets for the local BulletGlobal
            if (OnlineManager.instance.InOnlineGame && GameSettings.SharedAmmo)
            {
                BulletGlobalOnline.instance.ReduceBulletCountRpc(gun.BulletsThatAreUsed);
            }
            else if (GameSettings.SharedAmmo)
            {
                BulletGlobal.instance.Bullets -= gun.BulletsThatAreUsed;
            }
            else
            {
                playerAmmo.ammo -= gun.BulletsThatAreUsed;
            }

            if (gun.Delay > 0)
            {
                DelayShot().Forget();
            }

#if KILLITMYSELF_FULL
            switch (gun.SoundType)
            {
                case GunSoundType.Shotgun:
                    SoundManager.PlaySound2(ResourcesReferences.ShotgunShoot[Random.Range(0, ResourcesReferences.ShotgunShoot.Count)]);
                    break;
                case GunSoundType.Sniper:
                    SoundManager.PlaySound2(ResourcesReferences.SniperShoot[Random.Range(0, ResourcesReferences.SniperShoot.Count)]);
                    break;
                case GunSoundType.Pistol:
                    SoundManager.PlaySound2(ResourcesReferences.PistolShoot[Random.Range(0, ResourcesReferences.PistolShoot.Count)]);
                    break;
                case GunSoundType.Rifle:
                    SoundManager.PlaySound2(ResourcesReferences.RifleShoot[Random.Range(0, ResourcesReferences.RifleShoot.Count)]);
                    break;
                case GunSoundType.Karma:
                    break;
            }
#endif
            
            if (OnlineManager.instance.InOnlineGame)
            {
                recoil.FireRecoilRpc();
            }
            else
            {
                recoil.FireRecoil();
            }

            Quaternion rot;
#if KILLITMYSELF_FULL
            if (VRManager.instance.VREnabled)
            {
                rot = Quaternion.Euler(new Vector3(VRReferences.instance.RightController.transform.eulerAngles.x, VRReferences.instance.RightController.transform.eulerAngles.y, VRReferences.instance.RightController.transform.eulerAngles.z));
            }
            else
            {
#endif
                rot = Quaternion.Euler(new Vector3(Player1Transform.eulerAngles.x, Player1Transform.eulerAngles.y, Player1Transform.eulerAngles.z));
#if KILLITMYSELF_FULL
            }
#endif

            Instantiate(GunShootParticle, GunShootParticleOffset.position, Quaternion.Euler(new Vector3(0, 0, 0)), BulletParent);

            for (int i = 0; i < gun.AmountOfBulletsToShoot; i++)
            {
                BulletMove bullet = null;

                // Spawn bullet and if the gun shoots backwards, face the opossite direction of the players camera, else face the players camera
                if (gun.ShootBackwards)
                {
                    bullet = Instantiate(BulletPrefab, BulletOffsetBehind.position, rot, BulletParent).GetComponent<BulletMove>();
                }
                else
                {
                    bullet = Instantiate(BulletPrefab, BulletOffset.position, rot, BulletParent).GetComponent<BulletMove>();
                }
                
                bullet.damage = gun.Damage;

                if (gun.ShootBackwards)
                {
                    bullet.ShootBackwards = true;
                }

                bullet.from = playerMovement;

                if (OnlineManager.instance.InOnlineGame)
                {
                    bullet.IsClientSide = true;
                    
#if UNITY_EDITOR || KILLITMYSELF_DEBUG
                    if (cheat_forceshootatfardistance)
                    {
                        BulletGlobalOnline.instance.SpawnBulletRpc(new(100, 100, 100), rot, gun.Damage, gun.ShootBackwards, null);
                    }
                    else
                    {
#endif
                        BulletGlobalOnline.instance.SpawnBulletRpc(BulletOffset.position, rot, gun.Damage, gun.ShootBackwards, null);
#if UNITY_EDITOR || KILLITMYSELF_DEBUG
                    }
#endif
                }
                
                // If were not in an online game, set the bullets damage and set shoot backwards
                if (!OnlineManager.instance.InOnlineGame)
                {
                    bullet.damage = gun.Damage;

                    if (gun.ShootBackwards)
                    {
                        bullet.ShootBackwards = true;
                    }

                    bullet.from = playerMovement;
                }
            }
        }
        
        private async UniTask DelayShot()
        {
            CanShoot = false;
            await UniTask.WaitForSeconds(gun.Delay);
            CanShoot = true;
        }
    }
}