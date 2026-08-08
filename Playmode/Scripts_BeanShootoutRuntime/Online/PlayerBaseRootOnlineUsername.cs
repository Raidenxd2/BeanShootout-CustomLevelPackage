using Unity.Collections;
using Unity.Netcode;

namespace KillItMyself.Runtime
{
    public class PlayerBaseRootOnlineUsername : NetworkBehaviour
    {
        public NetworkVariable<FixedString32Bytes> Username = new("", NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> Kills = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<FixedString64Bytes> UniqueIdentifier = new("", NetworkVariableReadPermission.Owner, NetworkVariableWritePermission.Owner);
        public NetworkVariable<NetworkObjectReference> ActualPlayer = new(readPerm: NetworkVariableReadPermission.Everyone, writePerm: NetworkVariableWritePermission.Server);
        
        public NetworkVariable<int> PlayerColorIndex = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
        public NetworkVariable<int> PlayerVisorColorIndex = new(0,  NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<bool> IsVRPlayer = new(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

        public NetworkVariable<FixedString32Bytes> SceneName = new(string.Empty, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        private void Start()
        {
            DontDestroyOnLoad(gameObject);

            if (IsOwner)
            {
                Username.Value = BetterPrefs.GetString("Online_Username", "User");
#if KILLITMYSELF_FULL
                UniqueIdentifier.Value = Constants.GetUnique();
#endif
                
                PlayerColorIndex.Value = BetterPrefs.GetInt("Online_Defaults_PlayerColorIndex", 0);
                PlayerVisorColorIndex.Value = BetterPrefs.GetInt("Online_Defaults_PlayerVisorColorIndex", 0);

                IsVRPlayer.Value = VRManager.instance.VREnabled;
            }
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
            Username.Dispose();
            Kills.Dispose();
            UniqueIdentifier.Dispose();
        }
    }
}