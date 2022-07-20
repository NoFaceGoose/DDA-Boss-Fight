using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{
    public int health, maxHealth;
    public int defense, maxDefense;
    public float defenseFactor;

    public GameObject deathEffect;
    public GameObject shield;

    public GameObject bossHealthBar, shieldBar;

    public Text healthText, defenseText;

    public bool isInvulnerable;

    private void Start()
    {
        maxHealth = health;
        maxDefense = defense;
    }

    private void Update()
    {
        if (healthText)
        {
            healthText.GetComponent<Text>().text = health + "/" + maxHealth;
        }

        if (defenseText)
        {
            defenseText.GetComponent<Text>().text = defense + "/" + maxDefense;
        }
    }

    public void TakeDamage(int damage, bool isFireBall = false)
    {
        if (isInvulnerable)
            return;

        if (defense > 0)
        {
            defense -= (int)(damage * defenseFactor);

            if (defense <= 0)
            {
                defense = 0;
                Destroy(shield);
                Destroy(shieldBar);
            }
        }
        else
        {
            health -= damage;

            if (health <= 0)
            {
                Die();
            }
        }
    }

    void Die()
    {
        Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (GetComponent<Boss>().AI == 0)
        {
            GetComponent<Boss>().tree.Stop();
        }

        Destroy(gameObject);
        Destroy(bossHealthBar);
    }
}
