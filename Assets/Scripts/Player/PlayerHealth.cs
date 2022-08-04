using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    public int health, maxHealth;
    public float recoverTime;
    public float moveSpeedFactor, jumpForceFactor;

    public GameObject resultMenu;
    public GameObject playerHealthBar;

    public HPDiffObserver observer;

    public Boss boss;
    public Text healthText;
    public TextMeshProUGUI resultText;

    private bool isInvulnerable;

    private void Start()
    {
        maxHealth = health;
        isInvulnerable = false;
    }

    private void Update()
    {
        healthText.GetComponent<Text>().text = health + "/" + maxHealth;
    }

    public void TakeDamage(int damage, string attack = "")
    {
        if (isInvulnerable || InGameMenu.gameEnded)
        {
            return;
        }

        switch (attack)
        {
            case "Slash": boss.UpdateAction("Slash", true); break;
            case "Fire": boss.UpdateAction("Fire", true); break;

            case "ThrowPotion":
                boss.UpdateAction("ThrowPotion", true);
                if (IsInvoking(nameof(GetDetoxified)))
                {
                    CancelInvoke(nameof(GetDetoxified));
                }
                else
                {
                    GetPoisoned();
                }
                Invoke(nameof(GetDetoxified), recoverTime);
                break;

            case "Stab": boss.UpdateAction("Stab", true); break;
            case "Summon": boss.UpdateAction("Summon", true); break;

            default: break;
        }

        health -= damage;

        int value = -damage;

        observer.UpdateSumHPDiff();

        GetComponent<Player>().UpdateReminder("HP " + (value < 0 ? value.ToString() : ("+" + value)));

        if (damage > 0)
        {
            StartCoroutine(DamageAnimation());
        }

        health = health < 0 ? 0 : health;

        if (health == 0)
        {
            Invoke(nameof(Die), 0.1f);
        }
    }

    void GetPoisoned()
    {
        GetComponent<Player>().ChangeJumpForce(jumpForceFactor);
        GetComponent<PlayerMovement>().moveSpeed *= moveSpeedFactor;
    }

    void GetDetoxified()
    {
        GetComponent<Player>().ChangeJumpForce(1f / jumpForceFactor);
        GetComponent<PlayerMovement>().moveSpeed /= moveSpeedFactor;
    }

    void Die()
    {
        Destroy(gameObject);

        InGameMenu.gameEnded = true;
        resultText.text = "YOU DIED";
        resultText.color = Color.red;

        FindObjectOfType<AudioManager>().Stop("Theme");
        FindObjectOfType<AudioManager>().Stop("PlayerRunning");
        FindObjectOfType<AudioManager>().Stop("BossWalking");
        FindObjectOfType<AudioManager>().Stop("BossRunning");

        if (FindObjectOfType<Boss>())
        {
            MainMenu.bossFights[MainMenu.index].loss++;
            observer.UpdateAvgHPDiff();
        }

        resultMenu.SetActive(true);
        Time.timeScale = 0f;
    }

    IEnumerator DamageAnimation()
    {
        SpriteRenderer[] srs = GetComponentsInChildren<SpriteRenderer>();

        isInvulnerable = true;

        for (int i = 0; i < 3; i++)
        {
            foreach (SpriteRenderer sr in srs)
            {
                Color c = sr.color;
                c.a = 0;
                sr.color = c;
            }

            yield return new WaitForSeconds(.1f);

            foreach (SpriteRenderer sr in srs)
            {
                Color c = sr.color;
                c.a = 1;
                sr.color = c;
            }

            yield return new WaitForSeconds(.1f);
        }

        isInvulnerable = false;
    }
}