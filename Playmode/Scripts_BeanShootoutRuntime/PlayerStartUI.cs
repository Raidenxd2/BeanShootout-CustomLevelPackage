using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace KillItMyself.Runtime
{
    public class PlayerStartUI : NetworkBehaviour
    {
        [SerializeField] private PlayerInput playerControls;
        [SerializeField] private PlayerMovement playerMovement;
        [SerializeField] private BulletManager bullet;
        [SerializeField] private Recoil recoil;

        [SerializeField] private GameObject PlayerStartUIRoot;

        [SerializeField] private Image GunImage;
        [SerializeField] private TMP_Text GunNameText;

        [SerializeField] private Image PlayerColorImage;
        [SerializeField] private Image PlayerVisorColorImage;

        [SerializeField] private GameObject XboxControllerIcons;
        [SerializeField] private GameObject PlayStationControllerIcons;
        [SerializeField] private GameObject UniversalControllerIcons;
        [SerializeField] private GameObject NintendoButtons;
        [SerializeField] private GameObject GenericButtons;
        [SerializeField] private Transform ControllerIconsParent;

        public List<GunSO> guns = new();
        public int currentIndex;

        [SerializeField] private PlayerStartUISO startUI;
        public int PlayerColorCurrentIndex;
        public int PlayerVisorColorCurrentIndex;

        public MeshRenderer playerRenderer;
        [SerializeField] private MeshRenderer playerLocationRenderer;

        private void Start()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }

            GetComponent<Rigidbody>().position = GameObject.Find("PlayerSpawnBox").transform.position;

#if KILLITMYSELF_FULL
            if (CustomGunManager.AreGunModsLoaded)
            {
                guns.AddRange(CustomGunManager.ModSo);
            }
#endif
            
            if (OnlineManager.instance.InOnlineGame)
            {
                currentIndex = BetterPrefs.GetInt("Online_Defaults_GunIndex", 0);
                PlayerColorCurrentIndex = BetterPrefs.GetInt("Online_Defaults_PlayerColorIndex", 0);
                PlayerVisorColorCurrentIndex = BetterPrefs.GetInt("Online_Defaults_PlayerVisorColorIndex", 0);
            
                GunImage.sprite = guns[currentIndex].Image;
                GunNameText.text = guns[currentIndex].GunName;
                PlayerColorImage.color = startUI.colors[PlayerColorCurrentIndex];
                PlayerVisorColorImage.color = startUI.colors[PlayerVisorColorCurrentIndex];
            }

            try
            {
                if (playerControls.devices[0].displayName.Contains("Xbox"))
                {
                    Instantiate(XboxControllerIcons, ControllerIconsParent);
                    Instantiate(UniversalControllerIcons, ControllerIconsParent);
                }
                else if (playerControls.devices[0].displayName.Contains("DualSense"))
                {
                    Instantiate(PlayStationControllerIcons, ControllerIconsParent);
                    Instantiate(UniversalControllerIcons, ControllerIconsParent);
                }
                else if (playerControls.devices[0].displayName.Contains("Nintendo") || playerControls.devices[0].displayName.Contains("Pro Controller") || playerControls.devices[0].name.Contains("Switch") || playerControls.devices[0].name.Contains("ProController"))
                {
                    Instantiate(NintendoButtons, ControllerIconsParent);
                    Instantiate(UniversalControllerIcons, ControllerIconsParent);
                }
                else if (playerControls.devices[0].name.Contains("Gamepad"))
                {
                    // Instantiate(GenericButtons, ControllerIconsParent);
                    Instantiate(UniversalControllerIcons, ControllerIconsParent);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (VRManager.instance.VREnabled)
            {
                VRManager.instance.FixUI().Forget();
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

            List<Material> mats = new();
            playerRenderer.GetMaterials(mats);
            
            mats[0].color = startUI.colors[PlayerColorCurrentIndex];
            mats[1].color = startUI.colors[PlayerVisorColorCurrentIndex];
            playerLocationRenderer.material.color = startUI.colors[PlayerColorCurrentIndex];

            if (OnlineManager.instance.InOnlineGame)
            {
                JoinGameRpc(currentIndex, PlayerColorCurrentIndex, PlayerVisorColorCurrentIndex);

                playerMovement.rb.constraints = RigidbodyConstraints.None;
                playerMovement.rb.freezeRotation = true;
            }

            PlayerStartUIRoot.SetActive(false);
            GetComponent<PlayerStartUI>().enabled = false;

            if (SpawnManager.instance)
            {
                playerMovement.rb.position = SpawnManager.instance.SpawnPoints[Random.Range(0, SpawnManager.instance.SpawnPoints.Length)].position;
            }
            else
            {
                playerMovement.rb.position = Vector3.zero;
            }
        }

        [Rpc(SendTo.NotOwner)]
        private void JoinGameRpc(int index, int playerColorIndex, int playerVisorColorIndex)
        {
            bullet.gun = guns[index];
            bullet.BulletManagerInit();

            List<Material> mats = new();
            playerRenderer.GetMaterials(mats);
            
            mats[0].color = startUI.colors[playerColorIndex];
            mats[1].color = startUI.colors[playerVisorColorIndex];
            playerLocationRenderer.material.color = startUI.colors[playerColorIndex];

            playerMovement.rb.constraints = RigidbodyConstraints.None;
            playerMovement.rb.freezeRotation = true;
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
            if (PlayerColorCurrentIndex >= startUI.colors.Length - 1)
            {
                PlayerColorCurrentIndex = startUI.colors.Length - 1;
            }

            PlayerColorImage.color = startUI.colors[PlayerColorCurrentIndex];
        }

        public void PlayerColorSelectDown()
        {
            PlayerColorCurrentIndex--;
            if (PlayerColorCurrentIndex <= 0)
            {
                PlayerColorCurrentIndex = 0;
            }

            PlayerColorImage.color = startUI.colors[PlayerColorCurrentIndex];
        }

        public void PlayerVisorColorSelectUp()
        {
            PlayerVisorColorCurrentIndex++;
            if (PlayerVisorColorCurrentIndex >= startUI.colors.Length - 1)
            {
                PlayerVisorColorCurrentIndex = startUI.colors.Length - 1;
            }

            PlayerVisorColorImage.color = startUI.colors[PlayerVisorColorCurrentIndex];
        }

        public void PlayerVisorColorSelectDown()
        {
            PlayerVisorColorCurrentIndex--;
            if (PlayerVisorColorCurrentIndex <= 0)
            {
                PlayerVisorColorCurrentIndex = 0;
            }

            PlayerVisorColorImage.color = startUI.colors[PlayerVisorColorCurrentIndex];
        }

        private void Update()
        {
            if (OnlineManager.instance.InOnlineGame && !IsOwner)
            {
                return;
            }
            
            if (playerMovement.IsOnKeyboardMouse)
            {
                Cursor.lockState = CursorLockMode.None;
            }

            if (playerMovement.JumpInput.WasPressedThisFrame())
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