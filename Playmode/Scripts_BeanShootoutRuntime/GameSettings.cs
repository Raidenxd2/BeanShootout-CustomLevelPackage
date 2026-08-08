using UnityEngine;

namespace KillItMyself.Runtime
{
    public static class GameSettings
    {
        public static int MaxAmmo = 75;
        public static bool ShowMinimap = true;

        public static int MaxPlayers = 4;
        public static bool FullscreenNoOtherPlayers;

        public static bool SharedAmmo;
        public static bool AllowRespawning = true;

        public static bool Timer;
        public static int Time = 60;
        
        // Online secrets
        public static bool AllowSecretInteractions;
        public static bool Hotel_MassiveDoor;
        public static bool Hotel_Elevator;
        public static bool MoonbaseBeta_SlidingDoor;

        public static PlayerMovementSettingsSO MovementSettings;
        public static int MovementSettingsIndex;

#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod]
        public static void ResetValues()
        {
            MaxAmmo = 75;
            ShowMinimap = true;
            MaxPlayers = 4;
            FullscreenNoOtherPlayers = false;
            SharedAmmo = false;
            AllowRespawning = true;
            MovementSettings = null;
            MovementSettingsIndex = 0;
            Timer = false;
            Time = 60;
        }
#endif
    }
}