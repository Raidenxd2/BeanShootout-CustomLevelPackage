using UnityEngine;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    public class CanvasScale : MonoBehaviour
    {
        [SerializeField] private CanvasScaler cs;
        private void Start()
        {
            float val = BetterPrefs.GetFloat(PrefNames.UIScale, 1);
            
            cs.referenceResolution = new Vector2(1280 / val, 720 / val);
        }
    }
}