using UnityEngine;
using UnityEngine.UI;

namespace KillItMyself.Runtime
{
    /// <summary>
    /// Used for changing the in-game crosshair depending on the Crosshair setting
    /// </summary>
    public class CrosshairManager : MonoBehaviour
    {
        [SerializeField] private Image Crosshair;
        [SerializeField] private Sprite ModernCrosshair;
        [SerializeField] private Sprite OriginalCrosshair;

        private void Start()
        {
            switch (BetterPrefs.GetInt(PrefNames.Crosshair, 0))
            {
                case 0:
                    Crosshair.sprite = ModernCrosshair;
                    break;
                case 1:
                    Crosshair.sprite = OriginalCrosshair;
                    break;
            }
        }
    }
}