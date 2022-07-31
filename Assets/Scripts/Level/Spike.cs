using UnityEngine;

public class Spike : MonoBehaviour
{
    public int damage;
    public int health;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

        if (player)
        {
            FindObjectOfType<AudioManager>().Play("StabHit");
            player.TakeDamage(damage);
        }
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Destroy(gameObject);
        }
    }
}