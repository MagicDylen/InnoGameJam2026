using UnityEngine;
using UnityEngine.SceneManagement;

public class TriggerWinScreen : MonoBehaviour
{
    DynamicEnemySpawner spawner;
    bool winConditionMet = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TryGetComponent<DynamicEnemySpawner>(out spawner);
    }

    // Update is called once per frame
    void Update()
    {
        if(!spawner) return;
        if(!winConditionMet && spawner.GetProgress01() >= 1.0f )
        {
            var playerAnim = FindAnyObjectByType<PlayerAnimator>();
            playerAnim.animator.SetTrigger("Gold");

            winConditionMet = true;
            ObjectHolder objHolder = FindAnyObjectByType<ObjectHolder>();
            ObjectHolder.Player.TryGetComponent<PlayerStats>(out var stats);
            ObjectHolder.Player.TryGetComponent<Rigidbody2D>(out var rb);
            rb.bodyType = RigidbodyType2D.Static;

            if(stats) stats.IsDead = true;
            SceneManager.LoadScene("GameWonScene", LoadSceneMode.Additive);
            objHolder?.endGame();
        }
    }
}
