using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    public int health;
    public int maxHealth;
    public float recoverTime;
    public float moveSpeedFactor;
    public float jumpForceFactor;

    public GameObject deathEffect;
    public Boss boss;
    public Text healthText;

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
            case "Slash": boss.UpdateActionData("Slash", true); break;
            case "Fire": boss.UpdateActionData("Fire", true); break;

            case "ThrowPotion":
                boss.UpdateActionData("ThrowPotion", true);
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

            case "Stab": boss.UpdateActionData("Stab", true); break;
            case "Spell": boss.UpdateActionData("Spell", true); break;

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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