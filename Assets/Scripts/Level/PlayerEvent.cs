using UnityEngine;

public class PlayerEvent : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject player = collision.gameObject;
        if (player != null && player.CompareTag("Player"))
        {
            // Select a event by random
            int selector = Random.Range(0, 100);

            if (selector < 20)
            {
                // Change the cool down of player's bullet
                float value = Random.Range(-1f, 1f);
                player.GetComponent<PrefabWeapon>().coolDown += value;
                if (player.GetComponent<PrefabWeapon>().coolDown < 0f)
                {
                    player.GetComponent<PrefabWeapon>().coolDown = 0f;
                }

                player.GetComponent<CharacterController2D>().updateReminder("Fire Cool Down " + (value < 0 ? value.ToString() : ("+" + value)), value <= 0);
            }
            else if (selector >= 20 && selector < 40)
            {
                // Change player's running speed
                float value = Random.Range(-20f, 20f);
                player.GetComponent<PlayerMovement>().runSpeed += value;
                if (player.GetComponent<PlayerMovement>().runSpeed < 0.5f)
                {
                    player.GetComponent<PlayerMovement>().runSpeed = 0.5f;
                }

                player.GetComponent<CharacterController2D>().updateReminder("Run Speed " + (value < 0 ? value.ToString() : ("+" + value)), value >= 0);
            }
            else if (selector >= 40 && selector < 60)
            {
                // Change player's jump force
                float value = Random.Range(-300f, 300f);
                player.GetComponent<CharacterController2D>().changeJumpForce(value);

                player.GetComponent<CharacterController2D>().updateReminder("Jump Force " + (value < 0 ? value.ToString() : ("+" + value)), value >= 0);
            }
            else if (selector >= 60 && selector < 80)
            {
                // Change the speed of player's bullet
                float value = Random.Range(-10f, 10f);
                player.GetComponent<PrefabWeapon>().fireBallSpeed += value;
                if (player.GetComponent<PrefabWeapon>().fireBallSpeed < 0.5f)
                {
                    player.GetComponent<PrefabWeapon>().fireBallSpeed = 0.5f;
                }

                player.GetComponent<CharacterController2D>().updateReminder("Fire Ball Speed " + (value < 0 ? value.ToString() : ("+" + value)), value >= 0);
            }
            else if (selector >= 80 && selector < 100)
            {
                // Change the damage of player's bullet
                int value = Random.Range(-20, 20);
                player.GetComponent<PrefabWeapon>().fireBallDamage += value;
                if (player.GetComponent<PrefabWeapon>().fireBallDamage < 0)
                {
                    player.GetComponent<PrefabWeapon>().fireBallDamage = 0;
                }

                player.GetComponent<CharacterController2D>().updateReminder("Fire Ball Damage " + (value < 0 ? value.ToString() : ("+" + value)), value >= 0);
            }
            Destroy(gameObject);
        }
    }
}
