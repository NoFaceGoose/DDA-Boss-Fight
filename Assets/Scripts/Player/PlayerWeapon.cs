using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public Transform firePoint;

    public GameObject fireball;

    public float coolDown;
    private float timer;

    public int attackDamage;
    public Vector3 attackOffset;
    public float attackRange;
    public LayerMask attackMask;

    public Animator animator;

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (Input.GetButtonDown("Fire"))
        {
            if (timer >= coolDown)
            {
                Fire();
                timer = 0;
            }
        }

        if (Input.GetButtonDown("Slash"))
        {
            animator.SetTrigger("Slash");
        }
    }

    public void Attack()
    {
        FindObjectOfType<AudioManager>().Play("PlayerSlash");
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        if (colInfo)
        {
            if (colInfo.GetComponent<BossHealth>())
            {
                colInfo.GetComponent<BossHealth>().TakeDamage(attackDamage);
                FindObjectOfType<AudioManager>().Play("PlayerSlashHit");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Gizmos.DrawWireSphere(pos, attackRange);
    }

    void Fire()
    {
        Instantiate(fireball, firePoint.position, firePoint.rotation);
        FindObjectOfType<AudioManager>().Play("PlayerFire");
    }
}