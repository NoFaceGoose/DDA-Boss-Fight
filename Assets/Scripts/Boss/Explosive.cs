using UnityEngine;

public class Explosive : MonoBehaviour
{
    public float rotateSpeed;
    public float maxScale;
    public float growSpeed;
    public float gravityScale;

    public float explosionYOffset;
    public float explosionScale;
    public float range;
    public int damage;
    public LayerMask attackMask;

    public GameObject impactEffect;

    private bool isReady;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        isReady = false;
    }

    void FixedUpdate()
    {
        transform.Rotate(Vector3.forward * rotateSpeed);

        if (!isReady)
        {
            if (transform.localScale.x < maxScale && transform.localScale.y < maxScale)
            {
                transform.localScale *= growSpeed;
            }
            else
            {
                rb.gravityScale = gravityScale;
                isReady = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Boss Trigger"))
        {
            Destroy(gameObject);
            Quaternion rotation = transform.rotation;
            rotation.eulerAngles = Vector3.zero;
            GameObject explosion = Instantiate(impactEffect, transform.position + new Vector3(0f, explosionYOffset, 0f), rotation);
            explosion.transform.localScale = new Vector3(explosionScale, explosionScale, 0f);

            Collider2D colInfo = Physics2D.OverlapCircle(transform.position, range, attackMask);
            if (colInfo && colInfo.GetComponent<PlayerHealth>())
            {
                colInfo.GetComponent<PlayerHealth>().TakeDamage(damage, "Spell");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(transform.position, range);
    }
}