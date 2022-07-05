using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int attackDamage = 20;
    public int enragedAttackDamage = 40;
    public float cooldown = 2.0f;

    public Vector3 attackOffset;
    public float attackRange = 1f;
    public LayerMask attackMask;

    public GameObject swordWind;
    public GameObject potion;
    public Transform firePoint;
    public Transform throwPoint;

    public void Slash()
    {
        GetComponent<Boss>().UpdateAttackInfo("Slash");

        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, attackRange, attackMask);
        if (colInfo && colInfo.GetComponent<PlayerHealth>())
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(attackDamage, "Slash");
        }
    }

    public void Fire()
    {
        // Launch sword wind, no damage for the sword cutting in the animati on
        Instantiate(swordWind, firePoint.position, firePoint.rotation);
        GetComponent<Boss>().UpdateAttackInfo("Fire");
    }

    public void ThrowPotion()
    {
        Instantiate(potion, throwPoint.position, throwPoint.rotation);
        GetComponent<Boss>().UpdateAttackInfo("ThrowPotion");
    }

    public void Stab()
    {
        GetComponent<Boss>().UpdateAttackInfo("Stab");

        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;
        pos += transform.right * -0.6f;
        pos += transform.up * -0.15f;

        Collider2D colInfo = Physics2D.OverlapBox(pos, new Vector3(attackRange * 2.1f, 1.5f, 0f), attackMask);
        if (colInfo && colInfo.GetComponent<PlayerHealth>())
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(enragedAttackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        pos += transform.right * attackOffset.x;
        pos += transform.up * attackOffset.y;

        Gizmos.DrawWireSphere(pos, attackRange);

        pos += transform.right * -0.6f;
        pos += transform.up * -0.15f;
        Gizmos.DrawWireCube(pos, new Vector3(attackRange * 2.1f, 1.5f, 0f));
    }

}
