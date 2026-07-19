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
        if (PlayerPrefs.HasKey(USERNAME_KEY))
        {
            Debug.Log("[BootManager] Previous username found → loading Login screen (pre-filled)");
        }
        else
        {
            Debug.Log("[BootManager] No previous username → loading Register screen");
        }

        SceneManager.LoadScene(USERNAME_SCENE);
    }


    public static void SaveUsername(string username)
    {
        PlayerPrefs.SetString(USERNAME_KEY, username);
        PlayerPrefs.Save();
        GameManager.Instance.LocalPlayerName = username;
        GameManager.Instance.LocalPlayerId = username;

        
        Photon.Pun.PhotonNetwork.NickName = username;
        Photon.Pun.PhotonNetwork.AuthValues = new AuthenticationValues(username);

        

        Debug.Log($"[BootManager] Username saved: {username}");
    }
}