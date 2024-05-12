using System.Collections;
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

        private bool Dead;
        
        private PlayerMovement playerMovement;

        public int Health = 100;
        private int PreviousHealth = 100;

        private void Awake()
        {
            playerMovement = GetComponent<PlayerMovement>();
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
                
                playerMovement.GetComponent<Rigidbody>().AddForce(new Vector3(0, playerMovement.transform.position.y + 3, 0), ForceMode.Force);
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

        private IEnumerator DeadWait()
        {
            PlayerPrefs.SetInt("KillABean", 1);

            yield return new WaitForSeconds(5f);
            FadeAnim.Play("PlayerYouWEODied");

            yield return new WaitForSeconds(1.5f);
            PlayerCamera.GetComponent<PlayerCam>().enabled = false;
            PlayerCamera.GetComponent<MoveCamera>().enabled = false;
            PlayerCamera.GetComponent<Camera>().cullingMask = layerMask;
            PlayerCamera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;

            if (playerInput.devices[0].displayName.Contains("Mouse"))
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }

            Destroy(gameObject);
        }
    }
}