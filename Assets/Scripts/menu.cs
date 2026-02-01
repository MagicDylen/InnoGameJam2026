using UnityEngine;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        SceneManager.LoadScene("GameScene");
        var am = FindFirstObjectByType<AudioManager>();
        am?.PlayOneShot(am.UiClick, Vector3.zero);
    }
    public void OpenSettings()
    {
        Debug.Log("Player want some advanced QOL features."); 
    }

    public void QuitGame()
    {
        Debug.Log("Player super bored now. Get me out of here!");
        var am = FindFirstObjectByType<AudioManager>();
        am?.PlayOneShot(am.UiSelect, Vector3.zero);
        Application.Quit(); 
    }
}