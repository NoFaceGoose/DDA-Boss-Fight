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
        GetComponent<Boss>().tree.Stop();

        Destroy(gameObject);
        Destroy(bossHealthBar);
        Destroy(bossName);

        InGameMenu.gameEnded = true;
        resultText.text = "BOSS FELLED";
        resultText.color = Color.yellow;

        FindObjectOfType<AudioManager>().Stop("Theme");

        resultMenu.SetActive(true);
        Time.timeScale = 0f;
    }
}
