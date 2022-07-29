using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public Toggle toggle;
    public static int bossAI;
    public static bool skipTutorial;

    private void Start()
    {
        toggle.isOn = skipTutorial;
    }

    public void LoadBoss0()
    {
        bossAI = 0;
        loadBoss();
    }
    public void LoadBoss1()
    {
        bossAI = 1;
        loadBoss();
    }
    public void LoadBoss2()
    {
        bossAI = 2;
        loadBoss();
    }
    public void LoadBoss3()
    {
        bossAI = 3;
        loadBoss();
    }

    private void loadBoss()
    {
        skipTutorial = toggle.isOn;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}