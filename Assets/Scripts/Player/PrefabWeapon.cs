using UnityEngine;

public class PrefabWeapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;

    public float coolDown = 0.35f;
    private float timer = 0.35f;

    public float fireBallSpeed = 15f;
    public int fireBallDamage = 30;

    public int attackDamage = 50;
    public Vector3 attackOffset;
    public float attackRange = 1f;
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

        if (Input.GetButtonDown("Attack"))
        {
            animator.SetTrigger("Attack");
        }
    }

    public void Attack()
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        if (colInfo != null)
        {
            colInfo.GetComponent<BossHealth>().TakeDamage(attackDamage);
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
        GameObject fireBall = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        fireBall.GetComponent<FireBall>().speed = fireBallSpeed;
        fireBall.GetComponent<FireBall>().damage = fireBallDamage;
    }
}