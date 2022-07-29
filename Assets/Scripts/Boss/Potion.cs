using UnityEngine;

public class Potion : MonoBehaviour
{
    public float speed;
    public int damage;
    public Rigidbody2D rb;

    void Start()
    {
        rb.velocity = transform.right * speed * -1.0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fire Ball") || collision.CompareTag("Tile"))
        {
            FindObjectOfType<AudioManager>().Play("PotionHit");
            Destroy(gameObject);
        }
        else
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

            if (player)
            {
                player.TakeDamage(damage, "ThrowPotion");
                FindObjectOfType<AudioManager>().Play("PotionHit");
                Destroy(gameObject);
            }
        }
    }
}