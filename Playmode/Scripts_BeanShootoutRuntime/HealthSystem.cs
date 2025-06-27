using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    public class HealthSystem : MonoBehaviour
    {
        [SerializeField] private Image HealthBar;
        [SerializeField] private GameObject DeadUI;
        [SerializeField] private GameObject DeadExplosion;
        [SerializeField] private Animator FadeAnim;

        [SerializeField] private GameObject PlayerCamera;

        [SerializeField] private PlayerInput playerInput;

        [SerializeField] private LayerMask layerMask;

        private Rigidbody playerRb;

        private bool Dead;
        
        private PlayerMovement playerMovement;

        public int Health = 100;
        private int PreviousHealth = 100;

        [SerializeField] private TMP_Text DeathQuoteText;
        [SerializeField] private string[] DeathQuotes;

        private bool StopGoingUp;
        private bool CanRespawn;

        private LayerMask OldLayerMask;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();

            playerRb = playerMovement.GetComponent<Rigidbody>();
        }

        private void Update()
        {
            HealthBar.fillAmount = Health / 100f;

            if (Health <= 0)
            {
                if (!Dead)
                {
                    StartCoroutine(DeadWait());
                    DeadUI.SetActive(true);
                    playerMovement.GetComponent<Rigidbody>().detectCollisions = false;
                    Instantiate(DeadExplosion, transform.position, Quaternion.identity);
                }

                Dead = true;

#if UNITY_6000_0_OR_NEWER
                playerRb.linearVelocity = Vector3.zero;
#else
                playerRb.velocity = Vector3.zero;
#endif

                playerRb.position += new Vector3(0, 0.25f, 0);
            }

            if (Dead)
            {
                return;
            }

            if (PreviousHealth != Health)
            {
                PreviousHealth = Health;
                StartCoroutine(DamageEffect());
            }
        }

        private IEnumerator DamageEffect()
        {
            DeadUI.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            DeadUI.SetActive(false);
        }

        public void ReAlive()
        {
            Health = 100;

            if (playerInput.devices[0].displayName.Contains("Mouse"))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            Dead = false;

            PlayerCamera.GetComponent<PlayerCam>().enabled = true;

            PlayerCamera.GetComponent<Camera>().cullingMask = OldLayerMask;

            playerRb.detectCollisions = true;
            playerRb.useGravity = true;

            StopGoingUp = false;
            CanRespawn = false;

            // if (SpawnManager.instance != null)
            // {
            //     playerRb.position = SpawnManager.instance.SpawnPoints[Random.Range(0, SpawnManager.instance.SpawnPoints.Length)].position;
            // }
            // else
            // {
                playerRb.position = Vector3.zero;
            // }

            FadeAnim.Play("PlayerYouWEODied_Reset");
        }

        private IEnumerator DeadWait()
        {
            DeathQuoteText.text = DeathQuotes[Random.Range(0, DeathQuotes.Length)];

            yield return new WaitForSeconds(5f);
            FadeAnim.Play("PlayerYouWEODied");

            yield return new WaitForSeconds(1.5f);
            PlayerCamera.GetComponent<PlayerCam>().enabled = false;

            OldLayerMask = PlayerCamera.GetComponent<Camera>().cullingMask;
            PlayerCamera.GetComponent<Camera>().cullingMask = layerMask;

            playerRb.useGravity = false;

            StopGoingUp = true;
            CanRespawn = true;

            if (playerInput.devices[0].displayName.Contains("Mouse"))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
}