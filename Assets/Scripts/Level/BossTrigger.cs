using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public GameObject boss, bossBars, bossName;

    // Tigger boss fight when player lands on the stage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && boss && bossBars)
        {
            boss.SetActive(true);
            bossBars.SetActive(true);
            bossName.SetActive(true);
        }
    }
}