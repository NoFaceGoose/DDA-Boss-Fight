using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthBar : MonoBehaviour
{
    public PlayerHealth playerHealth;
    public Slider slider;

    void Start()
    {
        slider.maxValue = playerHealth.health;
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = playerHealth.health;
    }
}