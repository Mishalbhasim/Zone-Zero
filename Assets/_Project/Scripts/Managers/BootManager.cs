using UnityEngine;
using UnityEngine.SceneManagement;

public class BootManager : MonoBehaviour
{

    private const string USERNAME_SCENE = "UsernameScreen";
    private const string MAINMENU_SCENE = "MainMenu";

    private const string USERNAME_KEY = "PlayerUsername";

    void Start()
    {
       


        PhotonNetworkManager.Instance?.Connect();

        // load saved username into GameManager
        if (PlayerPrefs.HasKey(USERNAME_KEY))
        {
            string savedName = PlayerPrefs.GetString(USERNAME_KEY);
            GameManager.Instance.LocalPlayerName = savedName;
            GameManager.Instance.LocalPlayerId = savedName;

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
        Debug.Log($"[BootManager] Username saved: {username}");
    }


    public static void ClearUsername()
    {
        PlayerPrefs.DeleteKey(USERNAME_KEY);
        PlayerPrefs.Save();
        Debug.Log("[BootManager] Username cleared");
    }
}