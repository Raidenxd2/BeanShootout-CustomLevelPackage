using SerialPackage.Runtime;
using Unity.Netcode;

namespace KillItMyself.Runtime
{
    public class CurrentPlayer : NetworkBehaviour
    {
        public static CurrentPlayer instance;

        public PlayerMovement playerMovement;
        public HealthSystem healthSystem;
        public BulletManager bulletManager;
        public PlayerCam playerCam;

        private void Start()
        {
            if (IsOwner)
            {
                BeanLogger.Log("(Owner) Setting instance of CurrentPlayer.instance to this instance", this);
                instance = this;
            }
        }

        private new void OnDestroy()
        {
            if (IsOwner)
            {
                instance = null;
            }
            
            base.OnDestroy();
        }
    }
}