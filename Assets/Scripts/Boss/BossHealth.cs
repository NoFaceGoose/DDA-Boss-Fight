using UnityEngine;
using UnityEngine.UI;

public class BossHealth : MonoBehaviour
{

    public int health = 500;
    public int defense = 200;
    private int maxHealth = 0;
    private int maxDefense = 0;

    public GameObject deathEffect;
    public GameObject bossHealthBar;
    public GameObject shield;

    public Text healthText;
    public Text defenseText;

    public bool isInvulnerable = false;

    private void Start()
    {
        maxHealth = health;
        maxDefense = defense;
    }

    private void Update()
    {
        healthText.GetComponent<Text>().text = health + "/" + maxHealth;
        defenseText.GetComponent<Text>().text = defense + "/" + maxDefense;
    }

    public void TakeDamage(int damage)
    {
        if (isInvulnerable)
            return;

        if (defense > 0)
        {
            defense -= damage;

            if (defense <= 0)
            {
                Destroy(shield);
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
        GetComponent<Boss>().tree.Stop();
        Destroy(gameObject);
        Destroy(bossHealthBar);
    }
}
