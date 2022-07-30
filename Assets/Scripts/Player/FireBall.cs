using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float speed;
    public float lifetime;

    public int damage;

    public Rigidbody2D rb;

    public GameObject impactEffect;

    void Start()
    {
        rb.velocity = transform.right * speed;
        Invoke("SelfDestroy", lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<BossHealth>())
        {
            hitInfo.GetComponent<BossHealth>().TakeDamage(damage);
        }
        else if (hitInfo.GetComponent<Spike>())
        {
            hitInfo.GetComponent<Spike>().TakeDamage(damage);
        }

        if (!hitInfo.CompareTag("Player"))
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
            FindObjectOfType<AudioManager>().Play("PlayerFireHit");
            Destroy(gameObject);
        }
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
