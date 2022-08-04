using UnityEngine;

public class ActionData
{
    public int count, hit, damage;
    public float expectedDamage, fitness;
    public bool isValid;

    public ActionData(int damage)
    {
        count = 0;
        hit = 0;
        this.damage = damage;
        expectedDamage = 0f;
        fitness = 0;
        isValid = true;
    }

    // update the action's expected damage
    public void UpdateExpectedDamage()
    {
        expectedDamage = (float)hit / count * damage;
    }

    // calculate and return the action's fitness
    public float UpdateFitness(PlayerHealth playerHealth, BossHealth bossHealth)
    {
        float playerExpectedHP = playerHealth.health - expectedDamage >= 0 ? (float)(playerHealth.health - expectedDamage) : 0f;
        fitness = 1f - Mathf.Sqrt(Mathf.Abs(playerExpectedHP / playerHealth.maxHealth - (bossHealth.health + bossHealth.shield) / (float)(bossHealth.maxHealth + bossHealth.maxShield)));
        return fitness;
    }
}