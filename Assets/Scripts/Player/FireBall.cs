using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed;
    public float lifetime;

    public int damage;
    private int life = 1;

    public Rigidbody2D rb;

    public GameObject impactEffect;

    void Start()
    {
        rb.velocity = transform.right * speed;
        Invoke(nameof(SelfDestroy), lifetime);
    }

    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (life <= 0)
        {
            return;
        }

        if (hitInfo.GetComponent<BossHealth>())
        {
            hitInfo.GetComponent<BossHealth>().TakeDamage(damage, true);
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
            life--;
        }
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}