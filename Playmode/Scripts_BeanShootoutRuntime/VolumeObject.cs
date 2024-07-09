using UnityEngine;

namespace KillItMyself.Runtime
{
    public class VolumeObject : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(BetterPrefs.GetBool("PostProcessing", true));
        }
    }
}