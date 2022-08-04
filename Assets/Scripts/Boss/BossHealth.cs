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

    public HPDiffObserver observer;

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

    public void TakeDamage(int damage, bool isFireball = false)
    {
        if (isInvulnerable)
            return;

        if (shield > 0)
        {
            if (isFireball)
            {
                shield -= (int)(damage * defenseFactor);
            }
            else
            {
                shield -= damage;
            }

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

        observer.UpdateSumHPDiff();

        if (health == 0)
        {
            Invoke(nameof(Die), 0.1f);
        }
    }

    public void Intro()
    {
        FindObjectOfType<AudioManager>().Play("BossIntro");
    }

    void Die()
    {
        GetComponent<Boss>().tree.Stop();
        Destroy(gameObject);

        InGameMenu.gameEnded = true;
        resultText.text = "BOSS FELLED";
        resultText.color = Color.yellow;

        FindObjectOfType<AudioManager>().Stop("Theme");
        FindObjectOfType<AudioManager>().Stop("BossWalking");
        FindObjectOfType<AudioManager>().Stop("BossRunning");

        if (GetComponent<Boss>().player)
        {
            MainMenu.bossFights[MainMenu.index].win++;
            observer.UpdateAvgHPDiff();
        }

        resultMenu.SetActive(true);
    }
}
