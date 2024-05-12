using System.Collections;
using UnityEngine;

namespace KillItMyself.Runtime
{
    public class BulletMove : MonoBehaviour
    {
        private bool collisionEnabled;
        private Rigidbody rb;

        public int damage;
        public bool ShootBackwards;

        private void Start()
        {
            rb = gameObject.GetComponent<Rigidbody>();

            StartCoroutine(EnableCollision());
            StartCoroutine(DestroyWait());
        }

        public void FixedUpdate()
        {
            if (ShootBackwards)
            {
                rb.AddForce(-225f * -13 * -transform.forward.normalized, ForceMode.Force);
            }
            else
            {
                rb.AddForce(225f * 13 * transform.forward.normalized, ForceMode.Force);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!collisionEnabled)
            {
                return;
            }
            
            if (collision.gameObject.CompareTag("Player"))
            {
                collision.gameObject.GetComponent<HealthSystem>().Health -= damage;
                Destroy(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator DestroyWait()
        {
            yield return new WaitForSeconds(2f);
            Destroy(gameObject);
        }

        private IEnumerator EnableCollision()
        {
            yield return new WaitForSeconds(0.03f);
            collisionEnabled = true;
        }
    }
}