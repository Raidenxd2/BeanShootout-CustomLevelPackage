using Unity.Netcode;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class Recoil : NetworkBehaviour
    {
        [SerializeField] private RectTransform rt;
        [SerializeField] private RectTransform rtCrosshair;

        [SerializeField] private float speed;
        [SerializeField] private Vector3 rot;
        [SerializeField] private Vector3 ReloadingRot;

        [SerializeField] private PlayerAmmo ammo;

        private void Start()
        {
#if KILLITMYSELF_FULL
            if (IsOwner && VRManager.instance.VREnabled)
            {
                rt.parent.SetParent(VRReferences.instance.RightController.transform);
                rt.parent.localPosition = Vector3.zero;
                rt.parent.localRotation = Quaternion.Euler(new(0, -90, -7.31f));
                rt.localPosition = new(360, 298, 0);
                rt.localRotation = Quaternion.identity;

                rtCrosshair.localPosition = new(2500, 560, 0);
                rtCrosshair.localRotation = Quaternion.Euler(0, 90, 0);
                // rt.GetComponent<Image>().enabled = false;
            }
            else if (OnlineManager.instance.InOnlineGame)
            {
                rtCrosshair.gameObject.SetActive(false);
            }
#endif
        }

        private void Update()
        {
            if (VRManager.instance.VREnabled && IsOwner)
            {
                // rt.localPosition = Vector3.zero + new Vector3(0, 0f, -0.2f);
                // rt.parent.localPosition = VRReferences.instance.RightController.transform.localPosition + new Vector3(0, -1.75f, 0f);
                // rt.parent.localRotation = VRReferences.instance.RightController.transform.localRotation;

                // VRGun.localPosition = VRReferences.instance.RightController.transform.localPosition + new Vector3(0, -1.4f, 0);
                // VRGun.localRotation = VRReferences.instance.RightController.transform.localRotation;
            }
            
            if (!GameSettings.SharedAmmo)
            {
                if (ammo.Reloading)
                {
                    rt.localRotation = Quaternion.Euler(rt.localRotation.eulerAngles + ReloadingRot);

                    return;
                }
            }
            else
            {
                if (OnlineManager.instance.InOnlineGame && BulletGlobalOnline.instance.Reloading.Value || BulletGlobal.instance.Reloading)
                {
                    rt.localRotation = Quaternion.Euler(rt.localRotation.eulerAngles + ReloadingRot);

                    return;
                }
            }

            if (VRManager.instance.VREnabled)
            {
                // rt.localRotation = VRReferences.instance.RightController.transform.localRotation;
            }
            else
            {
                rt.localRotation = Quaternion.Slerp(rt.localRotation, Quaternion.Euler(0, -107.78f, 0), speed * Time.deltaTime);
            }
        }

        public void FireRecoil()
        {
            rt.localRotation = Quaternion.Euler(rt.localRotation.eulerAngles + rot);
        }

        [Rpc(SendTo.Everyone)]
        public void FireRecoilRpc()
        {
            if (!VRManager.instance.VREnabled)
            {
                FireRecoil();
            }
        }

        public void UpdateValuesForCurrentGun(GunSO gun)
        {
            speed = gun.RecoilSpeed;
            rot = gun.RecoilRot;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            
            Destroy(rt.parent.gameObject);
        }
    }
}