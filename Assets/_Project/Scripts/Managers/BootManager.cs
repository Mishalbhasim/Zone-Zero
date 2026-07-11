using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Realtime;

public class BootManager : MonoBehaviour
{

    private const string USERNAME_SCENE = "UsernameScreen";
    private const string MAINMENU_SCENE = "MainMenu";

    private const string USERNAME_KEY = "PlayerUsername";

    void Start()
    {
        // load saved username into GameManager
        if (PlayerPrefs.HasKey(USERNAME_KEY))
        {
            string savedName = PlayerPrefs.GetString(USERNAME_KEY);
            GameManager.Instance.LocalPlayerName = savedName;
            GameManager.Instance.LocalPlayerId = savedName;

            // MUST be set before Connect() — UserId is only sent to Photon
            // (and synced to other clients) as part of the connection handshake.
            Photon.Pun.PhotonNetwork.NickName = savedName;
            Photon.Pun.PhotonNetwork.AuthValues = new AuthenticationValues(savedName);

            PhotonNetworkManager.Instance?.Connect();

            Debug.Log($"[BootManager] Username found: {savedName} → loading MainMenu");
            GameManager.Instance.TransitionTo(GameManager.GameState.MainMenu);
            SceneManager.LoadScene(MAINMENU_SCENE);
        }
        else
        {
            Debug.Log("[BootManager] No username found → loading UsernameScreen");
            SceneManager.LoadScene(USERNAME_SCENE);
        }
    }


    public static void SaveUsername(string username)
    {
        PlayerPrefs.SetString(USERNAME_KEY, username);
        PlayerPrefs.Save();
        GameManager.Instance.LocalPlayerName = username;
        GameManager.Instance.LocalPlayerId = username;

        // Set AuthValues BEFORE connecting so UserId is registered with Photon
        // and synced to every other client (Owner.UserId, PlayerList, etc).
        Photon.Pun.PhotonNetwork.NickName = username;
        Photon.Pun.PhotonNetwork.AuthValues = new AuthenticationValues(username);

        PhotonNetworkManager.Instance?.Connect();

        Debug.Log($"[BootManager] Username saved: {username}");
    }
}