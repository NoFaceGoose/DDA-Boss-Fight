using UnityEngine;

public class SwordWind : MonoBehaviour
{
    public float speed;
    public int damage;
    public float lifetime;
    public Rigidbody2D rb;

    // Use this for initialization
    void Start()
    {
        rb.velocity = transform.right * speed * -1.0f;
        Invoke("SelfDestroy", lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<PlayerHealth>())
        {
            hitInfo.GetComponent<PlayerHealth>().TakeDamage(damage, "Fire");
        }
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}