using UnityEngine;
using UnityEngine.UI;

public class PrefabWeapon : MonoBehaviour
{
    public Transform firePoint;
    public GameObject bulletPrefab;
    public Text coolDownText;
    public float coolDown = 0.35f;
    private float timer = 0.35f;

    public float fireBallSpeed = 15f;
    public int fireBallDamage = 30;
    public Text speedText;
    public Text damageText;

    // Update is called once per frame
    void Update()
    {
        coolDownText.GetComponent<Text>().text = "Fire Cool Down: " + coolDown.ToString("#0.00");
        speedText.GetComponent<Text>().text = "Fire Ball Speed: " + fireBallSpeed.ToString("#0.00");
        damageText.GetComponent<Text>().text = "Fire Ball Damage: " + fireBallDamage.ToString();

        timer += Time.deltaTime;
        if (Input.GetButtonDown("Fire1"))
        {
            if (timer >= coolDown)
            {
                Shoot();
                timer = 0;
            }

        }
    }

    void Shoot()
    {
        GameObject fireBall = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        fireBall.GetComponent<Bullet>().speed = fireBallSpeed;
        fireBall.GetComponent<Bullet>().damage = fireBallDamage;
    }
}
