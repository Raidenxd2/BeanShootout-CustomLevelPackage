using UnityEngine;

namespace KillItMyself.Runtime
{
    public class PlayerBaseRootLocalUsername : MonoBehaviour
    {
        public string Username = "Player";
        public int Kills;

        private void Start()
        {
            Username = BetterPrefs.GetString("Online_Username", "User");
        }
    }
}