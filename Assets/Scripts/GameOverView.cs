using UnityEngine;

public class GameOverView : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            // TODO: restart the game
            Debug.Log("Restarting Game");
        }
    }
}
