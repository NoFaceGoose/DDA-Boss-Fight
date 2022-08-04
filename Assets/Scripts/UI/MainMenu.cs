using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject toggle;
    public TextMeshProUGUI[] challengingOrders, winLossRatios, bosstitles, AvgHPDiffs;
    public static int index;
    public static string bossTitle;
    public static Color bossColor;
    public static bool skipTutorial;
    public static Dictionary<int, BossFightData> bossFights;

    private void Awake()
    {
        if (bossFights == null)
        {
            bossFights = new Dictionary<int, BossFightData>();

            List<string> bosses = new() { "Grey Knight", "Golden Knight", "Red Knight", "Blue Knight" };
            List<Color> colors = new() { new Color(.7f, .7f, .7f), new Color(1f, .9f, .3f), new Color(.8f, .3f, .3f), new Color(.2f, .6f, 1f) };

            for (int i = 0; i < 4; i++)
            {
                int index = Random.Range(0, bosses.Count);
                bossFights.Add(i, new BossFightData(bosses[index], colors[index]));
                bosses.RemoveAt(index);
                colors.RemoveAt(index);
            }
        }
    }

    private void Start()
    {
        for (int i = 0; i < 4; i++)
        {
            challengingOrders[i].text = bossFights[i].challengingOrder > 0 ? bossFights[i].challengingOrder.ToString() : " ";
            challengingOrders[i].color = bossFights[i].color;

            winLossRatios[i].text = bossFights[i].win + "/" + bossFights[i].loss;
            winLossRatios[i].color = (bossFights[i].win > 0 || bossFights[i].loss > 0) ? bossFights[i].color : Color.white;

            bosstitles[i].text = bossFights[i].isRevealed ? bossFights[i].title : "Unknown Boss";
            bosstitles[i].color = bossFights[i].isRevealed ? bossFights[i].color : Color.white;

            AvgHPDiffs[i].text = bossFights[i].avgHPDiff > 0f ? bossFights[i].avgHPDiff.ToString("P", CultureInfo.InvariantCulture) : " ";
            AvgHPDiffs[i].color = bossFights[i].color;

            if (bossFights[i].isRevealed)
            {
                toggle.SetActive(true);
            }
        }

        toggle.GetComponent<Toggle>().isOn = skipTutorial;
    }

    public void LoadBoss(int id)
    {
        index = id;
        bossTitle = bossFights[id].title;
        bossColor = bossFights[id].color;
        skipTutorial = toggle.GetComponent<Toggle>().isOn;
        FindObjectOfType<AudioManager>().Play("Button");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}