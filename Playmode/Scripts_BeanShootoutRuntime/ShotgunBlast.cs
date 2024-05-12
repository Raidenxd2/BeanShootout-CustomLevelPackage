using UnityEngine;

namespace KillItMyself.Runtime
{
    public class ShotgunBlast : MonoBehaviour
    {
        private void OnEnable()
        {
            ParticleSystem VFX = gameObject.GetComponent<ParticleSystem>();
            float totalDuration = VFX.duration + VFX.startLifetime;
            Destroy(gameObject, totalDuration);
        }
    }
}