using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int attackDamage, enragedAttackDamage;

    public float cooldown;
    public float spellHeight;

    public float slashRange;
    public Vector3 slashOffset;

    public Vector3 stabOffset, stabSize;

    public LayerMask attackMask;

    public GameObject swordWind, potion, explosive;
    public Transform firePoint, throwPoint;

    public void Slash()
    {
        GetComponent<Boss>().UpdateAction("Slash");

        Vector3 pos = transform.position;
        pos += transform.right * slashOffset.x;
        pos += transform.up * slashOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, slashRange, attackMask);
        if (colInfo && colInfo.GetComponent<PlayerHealth>())
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(attackDamage, "Slash");
        }
    }

    public void Fire()
    {
        // Launch sword wind, no damage for the sword cutting in the animati on
        Instantiate(swordWind, firePoint.position, firePoint.rotation);
        GetComponent<Boss>().UpdateAction("Fire");
    }

    public void ThrowPotion()
    {
        Instantiate(potion, throwPoint.position, throwPoint.rotation);
        GetComponent<Boss>().UpdateAction("ThrowPotion");
    }

    public void Stab()
    {
        GetComponent<Boss>().UpdateAction("Stab");

        Vector3 pos = transform.position;
        pos += transform.right * stabOffset.x;
        pos += transform.up * stabOffset.y;

        Collider2D colInfo = Physics2D.OverlapBox(pos, stabSize, attackMask);
        if (colInfo && colInfo.GetComponent<PlayerHealth>())
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(enragedAttackDamage);
        }
    }

    public void Spell()
    {
        GetComponent<Boss>().UpdateAction("Spell");

        Vector3 pos = GetComponent<Boss>().player.transform.position;
        pos.y = spellHeight;
        Instantiate(explosive, pos, GetComponent<Boss>().player.transform.rotation);
    }

    void OnDrawGizmosSelected()
    {
        Vector3 pos = transform.position;
        pos += transform.right * slashOffset.x;
        pos += transform.up * slashOffset.y;
        Gizmos.DrawWireSphere(pos, slashRange);

        pos = transform.position;
        pos += transform.right * stabOffset.x;
        pos += transform.up * stabOffset.y;
        Gizmos.DrawWireCube(pos, stabSize);
    }
}