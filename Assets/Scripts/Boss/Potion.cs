using UnityEngine;

public class Potion : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 10;
    public Rigidbody2D rb;

    void Start()
    {
        rb.velocity = transform.right * speed * -1.0f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Fire Ball") || collision.CompareTag("Tile"))
        {
            Destroy(gameObject);
        }
        else
        {
            PlayerHealth player = collision.gameObject.GetComponent<PlayerHealth>();

            if (player != null)
            {
                player.TakeDamage(damage, "ThrowPotion");
                Destroy(gameObject);
            }
        }
    }
}
