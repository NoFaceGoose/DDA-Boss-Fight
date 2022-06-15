using UnityEngine;

public class SwordWind : MonoBehaviour
{

    public float speed = 10f;
    public int damage = 15;
    public Rigidbody2D rb;

    // Use this for initialization
    void Start()
    {
        rb.velocity = transform.right * speed * -1.0f;
        Invoke("SelfDestroy", 2.0f);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        PlayerHealth player = hitInfo.GetComponent<PlayerHealth>();

        if (player != null)
        {
            player.TakeDamage(damage);
        }

        Destroy(gameObject);
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
