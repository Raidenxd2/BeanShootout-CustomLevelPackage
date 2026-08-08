using UnityEngine;

namespace KillItMyself.Runtime
{
    [CreateAssetMenu(fileName = "PlayerStartUI",  menuName = "KillItMyself/PlayerStartUI", order = 1)]
    public class PlayerStartUISO : ScriptableObject
    {
        public Color[] colors;
    }
}