using UnityEngine;
using System.Collections.Generic;

public class BossWeapon : MonoBehaviour
{
    public int damageLowerBound, damageUpperBound;

    public static bool firstGame = true;
    public static List<int> damageValues;

    private readonly string[] attacks = { "Slash", "Fire", "ThrowPotion", "Stab", "Summon" };

    public int attackDamage, enragedAttackDamage;

    public float cooldown;
    public float orbHeight;

    public float slashRange;
    public Vector3 slashOffset;

    public Vector3 stabOffset, stabSize;

    public LayerMask attackMask;

    public GameObject shockWave, potion, orb;
    public Transform firePoint, throwPoint;

    // randomise attack values
    private void Awake()
    {
        int playerMaxHP = GetComponent<Boss>().player.GetComponent<PlayerHealth>().maxHealth;
        damageLowerBound = playerMaxHP / 20;
        damageUpperBound = playerMaxHP / 5;

        if (firstGame)
        {
            damageValues = new List<int>();
            firstGame = false;

            foreach (string attack in attacks)
            {
                int value = Random.Range(damageLowerBound, damageUpperBound);
                damageValues.Add(value);

                switch (attack)
                {
                    case "Slash": attackDamage = value; break;
                    case "Fire": shockWave.GetComponent<ShockWave>().damage = value; break;
                    case "ThrowPotion": potion.GetComponent<Potion>().damage = value; break;
                    case "Stab": enragedAttackDamage = value; break;
                    case "Summon": orb.GetComponent<Orb>().damage = value; break;
                    default: break;
                }
            }
        }
        else
        {
            foreach (string attack in attacks)
            {
                switch (attack)
                {
                    case "Slash": attackDamage = damageValues[0]; break;
                    case "Fire": shockWave.GetComponent<ShockWave>().damage = damageValues[1]; break;
                    case "ThrowPotion": potion.GetComponent<Potion>().damage = damageValues[2]; break;
                    case "Stab": enragedAttackDamage = damageValues[3]; break;
                    case "Summon": orb.GetComponent<Orb>().damage = damageValues[4]; break;
                    default: break;
                }
            }
        }
    }

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