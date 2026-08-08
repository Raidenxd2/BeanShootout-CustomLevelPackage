using SerialPackage.Runtime;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace KillItMyself.Runtime
{
    public class Leaderboard : MonoBehaviour
    {
        [SerializeField] private GameObject LeaderboardRoot;

        [SerializeField] private GameObject Player;
        [SerializeField] private Transform PlayerParent;

        private string KillsString;
        
        private void Start()
        {
#if KILLITMYSELF_FULL
            KillsString = LocalizedStringReferences.instance.Leaderboard_Kills.GetLocalizedString();
#endif
        }

        private void Update()
        {
            // TODO: Support inputs other than the keyboard
            LeaderboardRoot.SetActive(Keyboard.current.tabKey.isPressed);

            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                foreach (Transform item in PlayerParent)
                {
                    Destroy(item.gameObject);
                }

                if (OnlineManager.instance.InOnlineGame)
                {
                    BeanLogger.Log(NetworkManager.Singleton.SpawnManager.PlayerObjects.Count.ToString(), this);
                    foreach (var client in NetworkManager.Singleton.SpawnManager.PlayerObjects)
                    {
                        BeanLogger.Log(client.OwnerClientId.ToString(), this);
                        PlayerBaseRootOnlineUsername player = client.GetComponent<PlayerBaseRootOnlineUsername>();
                        
                        if (CommandLineArgs.VerboseLoggingEnabled)
                        {
                            BeanLogger.Log(player.Username.Value.ToString(), this);
                            BeanLogger.Log(player.Kills.Value.ToString(), this);
                        }
                        
                        LeaderboardPlayerObject playerObject = Instantiate(Player, PlayerParent).GetComponent<LeaderboardPlayerObject>();
                        playerObject.PlayerNameText.text = player.Username.Value.ToString();
                        playerObject.PlayerKillsText.text = string.Format(KillsString, player.Kills.Value);
                    }
                }
                else
                {
                    foreach (var client in PlayersJoined.instance.Players)
                    {
                        PlayerBaseRootLocalUsername player = client.GetComponent<PlayerMovement>().username;
                        
                        if (CommandLineArgs.VerboseLoggingEnabled)
                        {
                            BeanLogger.Log(player.Username, this);
                            BeanLogger.Log(player.Kills.ToString(), this);
                        }
                        
                        LeaderboardPlayerObject playerObject = Instantiate(Player, PlayerParent).GetComponent<LeaderboardPlayerObject>();
                        playerObject.PlayerNameText.text = player.Username;
                        playerObject.PlayerKillsText.text = string.Format(KillsString, player.Kills);
                    }
                }
            }
        }
    }
}