using UnityEngine;

namespace KillItMyself.Runtime
{
    public class Recoil : MonoBehaviour
    {
        [SerializeField] private RectTransform rt;

        [SerializeField] private float speed;
        [SerializeField] private Vector3 rot;

        private void Update()
        {
            rt.localRotation = Quaternion.Slerp(rt.localRotation, Quaternion.Euler(0, -107.78f, 0), Time.fixedDeltaTime * speed);
        }

        public void FireRecoil()
        {
            rt.localRotation = Quaternion.Euler(rt.localRotation.eulerAngles + rot);
        }

        public void UpdateValuesForCurrentGun(GunSO gun)
        {
            speed = gun.RecoilSpeed;
            rot = gun.RecoilRot;
        }
    }
}