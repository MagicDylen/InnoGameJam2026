using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
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