using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public int damage = 30;
    public Rigidbody2D rb;
    public GameObject impactEffect;

    // Use this for initialization
    void Start()
    {
        rb.velocity = transform.right * speed;
        Invoke("SelfDestroy", 2.0f);
    }


    void OnTriggerEnter2D(Collider2D hitInfo)
    {
        if (hitInfo.GetComponent<BossHealth>() != null)
        {
            hitInfo.GetComponent<BossHealth>().TakeDamage(damage);
        }
        else if (hitInfo.GetComponent<Spike>() != null)
        {
            hitInfo.GetComponent<Spike>().TakeDamage(damage);
        }

        if (!hitInfo.CompareTag("Player"))
        {
            Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void SelfDestroy()
    {
        Destroy(gameObject);
    }
}
