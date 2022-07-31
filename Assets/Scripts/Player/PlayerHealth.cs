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
        if (isInvulnerable)
        {
            return;
        }

        switch (attack)
        {
            case "Slash": boss.UpdateAction("Slash", true); break;
            case "Fire": boss.UpdateAction("Fire", true); break;

            case "ThrowPotion":
                boss.UpdateAction("ThrowPotion", true);
                if (IsInvoking("GetDetoxified"))
                {
                    CancelInvoke("GetDetoxified");
                }
                else
                {
                    GetPoisoned();
                }
                Invoke("GetDetoxified", recoverTime);
                break;

            case "Stab": boss.UpdateAction("Stab", true); break;
            case "Summon": boss.UpdateAction("Summon", true); break;

            default: break;
        }

        health -= damage;

        int value = -damage;

        GetComponent<Player>().updateReminder("HP " + (value < 0 ? value.ToString() : ("+" + value)));

        if (damage > 0)
        {
            StartCoroutine(DamageAnimation());
        }

        if (health > maxHealth)
        {
            health = maxHealth;
        }

        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    void GetPoisoned()
    {
        GetComponent<Player>().changeJumpForce(jumpForceFactor);
        GetComponent<PlayerMovement>().moveSpeed *= moveSpeedFactor;
    }

    void GetDetoxified()
    {
        GetComponent<Player>().changeJumpForce(1f / jumpForceFactor);
        GetComponent<PlayerMovement>().moveSpeed /= moveSpeedFactor;
    }

    void Die()
    {
        gameObject.SetActive(false);

        InGameMenu.gameEnded = true;
        resultText.text = "YOU DIED";
        resultText.color = Color.red;

        FindObjectOfType<AudioManager>().StopAll();

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