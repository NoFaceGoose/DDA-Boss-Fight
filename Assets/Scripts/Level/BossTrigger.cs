using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public GameObject boss;
    public GameObject bossBars;

    // Tigger boss fight when player lands on the stage
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && boss && bossBars)
        {
            boss.SetActive(true);
            bossBars.SetActive(true);
        }
    }
}