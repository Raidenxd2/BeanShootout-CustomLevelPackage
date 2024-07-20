using UnityEngine;

namespace KillItMyself.Runtime
{
    [AddComponentMenu("Bean Shootout/Audio Type")]
    public class AudioSource_AudioType : MonoBehaviour
    {
        [Tooltip("The setting you set here allows you to change the volume in the game's audio settings")] [SerializeField] private CustomLevelAudioType AudioType;
    }

    public enum CustomLevelAudioType
    {
        Other,
        Music,
        Sound,
        Ambience
    }
}