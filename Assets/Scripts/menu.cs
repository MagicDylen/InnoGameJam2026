using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        Debug.Log("Player wanna play!");
        // SceneManager.LoadScene("StartingScene");
    }
    public void OpenSettings()
    {
        Debug.Log("Player want some advanced QOL features."); 
    }

    public void QuitGame()
    {
        Debug.Log("Player super bored now. Get me out of here!");
        Application.Quit(); 
    }
}