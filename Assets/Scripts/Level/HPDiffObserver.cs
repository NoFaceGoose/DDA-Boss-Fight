using UnityEngine;

public class HPDiffObserver : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public BossHealth bossHealth;

    private int count = 0;
    private float sumHPDiff = 0f;

    private void Awake()
    {
        count = 0;
        sumHPDiff = 0f;
    }

    public void UpdateSumHPDiff()
    {
        if (playerHealth && bossHealth && bossHealth.gameObject.activeInHierarchy)
        {
            count++;
            sumHPDiff += Mathf.Abs(playerHealth.health / (float)playerHealth.maxHealth - (bossHealth.health + bossHealth.shield) / (float)(bossHealth.maxHealth + bossHealth.maxShield));
        }
    }

    public void UpdateAvgHPDiff()
    {
        MainMenu.bossFights[MainMenu.index].UpdateAvgHPDiff(sumHPDiff / count);
    }
}