using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    public class BulletManager : MonoBehaviour
    {
        [SerializeField] private GameObject BulletPrefab;
        [SerializeField] private Transform BulletParent;
        [SerializeField] private GameObject GunShootParticle;

        [SerializeField] private Transform Player1Transform;
        [SerializeField] private Transform BulletOffset;
        [SerializeField] private Transform BulletOffsetBehind;
        [SerializeField] private Transform GunShootParticleOffset;

        [SerializeField] private PlayerInput playerControls;

        public GunSO gun;
        [SerializeField] private Image gunVisual;

        [SerializeField] private Recoil recoil;

        public bool CanShoot;

        private void Update()
        {
            try
            {
                if (playerControls.actions["Shoot"].WasPressedThisFrame() && !gun.HoldToShoot)
                {
                    Shoot();
                }
                else if (playerControls.actions["Shoot"].IsPressed() && gun.HoldToShoot)
                {
                    Shoot();
                }
            }
            catch
            {

            }
        }

        public void BulletManagerInit()
        {
            gunVisual.sprite = gun.Image;
        }

        private void Shoot()
        {
            if (BulletGlobal.instance.Reloading || !CanShoot)
            {
                return;
            }

            BulletGlobal.instance.Bullets -= gun.BulletsThatAreUsed;

            for (int i = 0; i < gun.AmountOfBulletsToShoot; i++)
            {
                if (gun.Delay > 0)
                {
                    StartCoroutine(DelayShot());
                }

                recoil.FireRecoil();

                GameObject bullet;

                if (gun.ShootBackwards)
                {
                    bullet = Instantiate(BulletPrefab, BulletOffsetBehind.position, Quaternion.Euler(new Vector3(Player1Transform.eulerAngles.x, Player1Transform.eulerAngles.y, Player1Transform.eulerAngles.z)), BulletParent);
                    Instantiate(GunShootParticle, GunShootParticleOffset.position, Quaternion.Euler(new Vector3(0, 0, 0)), BulletParent);
                }
                else
                {
                    bullet = Instantiate(BulletPrefab, BulletOffset.position, Quaternion.Euler(new Vector3(Player1Transform.eulerAngles.x, Player1Transform.eulerAngles.y, Player1Transform.eulerAngles.z)), BulletParent);
                    Instantiate(GunShootParticle, GunShootParticleOffset.position, Quaternion.Euler(new Vector3(0, 0, 0)), BulletParent);
                }
                
                bullet.GetComponent<BulletMove>().damage = gun.Damage;

                if (gun.ShootBackwards)
                {
                    bullet.GetComponent<BulletMove>().ShootBackwards = true;
                }
            }
        }
        
        private IEnumerator DelayShot()
        {
            CanShoot = false;
            yield return new WaitForSeconds(gun.Delay);
            CanShoot = true;
        }
    }
}