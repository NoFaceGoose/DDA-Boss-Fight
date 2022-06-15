using UnityEngine;
using UnityEngine.UI;

public class BossShieldBar : MonoBehaviour
{
    public BossHealth bossHealth;
    public Slider slider;

    void Start()
    {
        slider.maxValue = bossHealth.defense;
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = bossHealth.defense;
    }
}