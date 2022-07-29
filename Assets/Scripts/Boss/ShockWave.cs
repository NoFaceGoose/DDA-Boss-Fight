using UnityEngine;

public class ShockWave : MonoBehaviour
{
    public float speed;
    public float lifetime;

    public int damage;

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
            FindObjectOfType<AudioManager>().Play("ShockWaveHit");
        }
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}