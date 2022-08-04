using UnityEngine;
using System.Collections.Generic;

public class BossWeapon : MonoBehaviour
{
    public static bool firstGame = true;
    public static List<int> damageValues;

    public int attackDamage, enragedAttackDamage;

    public float cooldown;
    public float orbHeight;

    public float slashRange;
    public Vector3 slashOffset;

    public Vector3 stabOffset, stabSize;

    public LayerMask attackMask;

    public GameObject shockWave, potion, orb;
    public Transform firePoint, throwPoint;

    public void Slash()
    {
        FindObjectOfType<AudioManager>().Play("BossSlash");

        GetComponent<Boss>().UpdateAction("Slash");

        Vector3 pos = transform.position;
        pos += transform.right * slashOffset.x;
        pos += transform.up * slashOffset.y;

        Collider2D colInfo = Physics2D.OverlapCircle(pos, slashRange, attackMask);
        if (colInfo && colInfo.GetComponent<PlayerHealth>())
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(attackDamage, "Slash");
            FindObjectOfType<AudioManager>().Play("BossSlashHit");
        }
    }

    public void Fire()
    {
        // Launch shock wave, no damage for the sword cutting in the animation
        Instantiate(shockWave, firePoint.position, firePoint.rotation);
        FindObjectOfType<AudioManager>().Play("BossFire");
        GetComponent<Boss>().UpdateAction("Fire");
    }

    public void ThrowPotion()
    {
        Instantiate(potion, throwPoint.position, throwPoint.rotation);
        FindObjectOfType<AudioManager>().Play("BossThrowPotion");
        GetComponent<Boss>().UpdateAction("ThrowPotion");
    }

    public void Stab()
    {
        GetComponent<Boss>().UpdateAction("Stab");

        FindObjectOfType<AudioManager>().Play("BossSlash");

        Vector3 pos = transform.position;
        pos += transform.right * stabOffset.x;
        pos += transform.up * stabOffset.y;

        Collider2D colInfo = Physics2D.OverlapBox(pos, stabSize, 0f, attackMask);

        if (colInfo && colInfo.GetComponent<PlayerHealth>())
        {
            colInfo.GetComponent<PlayerHealth>().TakeDamage(enragedAttackDamage, "Stab");
            FindObjectOfType<AudioManager>().Play("StabHit");
        }
    }

    public void Summon()
    {
        GetComponent<Boss>().UpdateAction("Summon");

        Vector3 pos = GetComponent<Boss>().player.transform.position;
        pos.y = orbHeight;
        Instantiate(orb, pos, GetComponent<Boss>().player.transform.rotation);

        FindObjectOfType<AudioManager>().Play("BossSummon");
    }

    public void TurnVulnerable()
    {
        GetComponent<BossHealth>().isInvulnerable = false;
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