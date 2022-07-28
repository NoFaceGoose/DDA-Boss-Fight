using UnityEngine;

public class BossWeapon : MonoBehaviour
{
    public int damageLowerBound, damageUpperBound;

    public int attackDamage, enragedAttackDamage;

    public float cooldown;
    public float orbHeight;

    public float slashRange;
    public Vector3 slashOffset;

    public Vector3 stabOffset, stabSize;

    public LayerMask attackMask;

    public GameObject shockWave, potion, orb;
    public Transform firePoint, throwPoint;

    private void Start()
    {
        int playerMaxHP = GetComponent<Boss>().player.GetComponent<PlayerHealth>().maxHealth;
        damageLowerBound = playerMaxHP / 20;
        damageUpperBound = playerMaxHP / 5;
    }

    public int RandomizeAttackDamage(string name)
    {
        int value = Random.Range(damageLowerBound, damageUpperBound);

        switch (name)
        {
            case "Slash": attackDamage = value; break;
            case "Fire": shockWave.GetComponent<ShockWave>().damage = value; break;
            case "ThrowPotion": potion.GetComponent<Potion>().damage = value; break;
            case "Stab": enragedAttackDamage = value; break;
            case "Summon": orb.GetComponent<Orb>().damage = value; break;
            default: break;
        }

        return value;
    }

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
        // Launch shock wave, no damage for the sword cutting in the animation
        Instantiate(shockWave, firePoint.position, firePoint.rotation);
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

    public void Summon()
    {
        GetComponent<Boss>().UpdateAction("Summon");

        Vector3 pos = GetComponent<Boss>().player.transform.position;
        pos.y = orbHeight;
        Instantiate(orb, pos, GetComponent<Boss>().player.transform.rotation);
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