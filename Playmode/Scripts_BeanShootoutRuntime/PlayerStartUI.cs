using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    public class PlayerStartUI : MonoBehaviour
    {
        [SerializeField] private PlayerInput playerControls;
        [SerializeField] private BulletManager bullet;
        [SerializeField] private Recoil recoil;

        [SerializeField] private GameObject PlayerStartUIRoot;

        [SerializeField] private Image GunImage;
        [SerializeField] private TMP_Text GunNameText;

        [SerializeField] private Image PlayerColorImage;
        [SerializeField] private Image PlayerVisorColorImage;

        [SerializeField] private GameObject XboxControllerIcons;
        [SerializeField] private GameObject UniversalControllerIcons;
        [SerializeField] private Transform ControllerIconsParent;

        [SerializeField] private List<GunSO> guns = new();
        private int currentIndex;

        [SerializeField] private Color[] colors;
        private int PlayerColorCurrentIndex;
        private int PlayerVisorColorCurrentIndex;

        [SerializeField] private MeshRenderer playerRenderer;
        [SerializeField] private MeshRenderer playerVisorRenderer;
        [SerializeField] private MeshRenderer playerLocationRenderer;
        
        private void Start()
        {
            if (playerControls.devices[0].displayName.Contains("Xbox"))
            {
                Instantiate(XboxControllerIcons, ControllerIconsParent);
                Instantiate(UniversalControllerIcons, ControllerIconsParent);
            }
        }

        public void JoinGame()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            
            bullet.gun = guns[currentIndex];
            bullet.BulletManagerInit();
            bullet.CanShoot = true;

            recoil.UpdateValuesForCurrentGun(guns[currentIndex]);

            playerRenderer.material.color = colors[PlayerColorCurrentIndex];
            playerLocationRenderer.material.color = colors[PlayerColorCurrentIndex];
            playerVisorRenderer.material.color = colors[PlayerVisorColorCurrentIndex];

            PlayerStartUIRoot.SetActive(false);
            GetComponent<PlayerStartUI>().enabled = false;
        }

        public void GunUp()
        {
            currentIndex++;
            if (currentIndex >= guns.Count - 1)
            {
                currentIndex = guns.Count - 1;
            }

            GunImage.sprite = guns[currentIndex].Image;
            GunNameText.text = guns[currentIndex].GunName;
        }

        public void GunDown()
        {
            currentIndex--;
            if (currentIndex <= 0)
            {
                currentIndex = 0;
            }

            GunImage.sprite = guns[currentIndex].Image;
            GunNameText.text = guns[currentIndex].GunName;
        }

        public void PlayerColorSelectUp()
        {
            PlayerColorCurrentIndex++;
            if (PlayerColorCurrentIndex >= colors.Length - 1)
            {
                PlayerColorCurrentIndex = colors.Length - 1;
            }

            PlayerColorImage.color = colors[PlayerColorCurrentIndex];
        }

        public void PlayerColorSelectDown()
        {
            PlayerColorCurrentIndex--;
            if (PlayerColorCurrentIndex <= 0)
            {
                PlayerColorCurrentIndex = 0;
            }

            PlayerColorImage.color = colors[PlayerColorCurrentIndex];
        }

        public void PlayerVisorColorSelectUp()
        {
            PlayerVisorColorCurrentIndex++;
            if (PlayerVisorColorCurrentIndex >= colors.Length - 1)
            {
                PlayerVisorColorCurrentIndex = colors.Length - 1;
            }

            PlayerVisorColorImage.color = colors[PlayerVisorColorCurrentIndex];
        }

        public void PlayerVisorColorSelectDown()
        {
            PlayerVisorColorCurrentIndex--;
            if (PlayerVisorColorCurrentIndex <= 0)
            {
                PlayerVisorColorCurrentIndex = 0;
            }

            PlayerVisorColorImage.color = colors[PlayerVisorColorCurrentIndex];
        }

        private void Update()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (playerControls.actions["Jump"].WasPressedThisFrame())
            {
                JoinGame();
            }
            if (playerControls.actions["DPadUp"].WasPressedThisFrame())
            {
                GunUp();
            }
            if (playerControls.actions["DPadDown"].WasPressedThisFrame())
            {
                GunDown();
            }
            if (playerControls.actions["DPadRight"].WasPerformedThisFrame())
            {
                PlayerColorSelectUp();
            }
            if (playerControls.actions["DPadLeft"].WasPressedThisFrame())
            {
                PlayerColorSelectDown();
            }
            if (playerControls.actions["ButtonWest"].WasPressedThisFrame())
            {
                PlayerVisorColorSelectUp();
            }
            if (playerControls.actions["ButtonNorth"].WasPressedThisFrame())
            {
                PlayerVisorColorSelectDown();
            }
        }
    }
}