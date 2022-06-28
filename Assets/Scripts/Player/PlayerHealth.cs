using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{

    public int health = 100;
    private int maxHealth = 0;
    private bool isInvulnerable = false;

    public GameObject deathEffect;
    public Text healthText;

    private void Start()
    {
        maxHealth = health;
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
            case "Attack": GetComponent<CharacterController2D>().boss.GetComponent<Boss>().UpdateAttackInfo("Attack", true); break;
            case "Fire": GetComponent<CharacterController2D>().boss.GetComponent<Boss>().UpdateAttackInfo("Fire", true); break;
            case "ThrowPotion": GetComponent<CharacterController2D>().boss.GetComponent<Boss>().UpdateAttackInfo("ThrowPotion", true); break;
            default: break;
        }

        health -= damage;

        int value = -damage;

        GetComponent<CharacterController2D>().updateReminder("HP " + (value < 0 ? value.ToString() : ("+" + value)), value >= 0);

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
