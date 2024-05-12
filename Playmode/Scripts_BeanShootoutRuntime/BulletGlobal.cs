using System.Collections;
using TMPro;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class BulletGlobal : MonoBehaviour
    {
        public static BulletGlobal instance;

        public int Bullets;
        public bool Reloading;

        [SerializeField] private TMP_Text BulletsText;

        private void Awake()
        {
            instance = this;

            Bullets = GameSettings.MaxAmmo;
        }

        private void Update()
        {
            BulletsText.text = Bullets.ToString();

            if (Bullets <= 0 && !Reloading)
            {
                StartCoroutine(BulletReload());
                Reloading = true;
            }
        }

        private IEnumerator BulletReload()
        {
            yield return new WaitForSeconds(5f);
            Bullets = GameSettings.MaxAmmo;
            Reloading = false;
        }
    }
}