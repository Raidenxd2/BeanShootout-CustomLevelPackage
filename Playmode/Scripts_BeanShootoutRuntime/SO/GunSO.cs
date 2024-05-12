using UnityEngine;

namespace KillItMyself.Runtime
{
    [CreateAssetMenu(fileName = "New Gun", menuName = "KillItMyself/Gun", order = 1)]
    public class GunSO : ScriptableObject
    {
        [Header("Gun Information")]
        public string GunName;
        public Sprite Image;

        [Header("Gun Stats")]
        public int AmountOfBulletsToShoot;
        public int BulletsThatAreUsed;
        public int Damage;
        public bool HoldToShoot;
        public float Delay;
        public bool ShootBackwards;
        public float RecoilSpeed = 3;
        public Vector3 RecoilRot = new(0, 0, 25);
    }
}