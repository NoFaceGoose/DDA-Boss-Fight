using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BossHealth : MonoBehaviour
{
    public int health, maxHealth;
    public int shield, maxShield;
    public float defenseFactor;

    public GameObject resultMenu;
    public GameObject shieldObj;
    public GameObject bossHealthBar, shieldBar, bossName;

    public Text healthText, shieldText;
    public TextMeshProUGUI resultText;

    public bool isInvulnerable;

    private void Start()
    {
        maxHealth = health;
        maxShield = shield;
    }

    private void Update()
    {
        if (healthText)
        {
            healthText.GetComponent<Text>().text = health + "/" + maxHealth;
        }

        if (shieldText)
        {
            shieldText.GetComponent<Text>().text = shield + "/" + maxShield;
        }

        if (health == 0)
        {
            Die();
        }
    }

    public void TakeDamage(int damage, bool isFireBall = false)
    {
        if (isInvulnerable)
            return;

        if (shield > 0)
        {
            shield -= (int)(damage * defenseFactor);

            if (shield <= 0)
            {
                shield = 0;
                Destroy(shieldObj);
                FindObjectOfType<AudioManager>().Play("PlayerFireHit");
            }
        }
        else
        {
            health -= damage;
            health = health < 0 ? 0 : health;
        }
    }

    public void Intro()
    {
        FindObjectOfType<AudioManager>().Play("BossIntro");
    }

    void Die()
    {
        GetComponent<Boss>().tree.Stop();
        gameObject.SetActive(false);

        InGameMenu.gameEnded = true;
        resultText.text = "BOSS FELLED";
        resultText.color = Color.yellow;

        FindObjectOfType<AudioManager>().Stop("Theme");
        FindObjectOfType<AudioManager>().Stop("BossWalking");
        FindObjectOfType<AudioManager>().Stop("BossRunning");

        resultMenu.SetActive(true);
    }
}
