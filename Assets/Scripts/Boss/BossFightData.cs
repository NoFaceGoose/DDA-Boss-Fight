using UnityEngine;

public class BossFightData
{
    public string title;
    public bool isRevealed;
    public int challengingOrder;
    public int win, loss;
    public float avgHPDiff;
    public Color color;

    public BossFightData(string bossTitle, Color bossColor)
    {
        title = bossTitle;
        color = bossColor;
        isRevealed = false;
        challengingOrder = 0;
        win = 0;
        loss = 0;
        avgHPDiff = 0f;
    }

    public void UpdateAvgHPDiff(float value)
    {
        avgHPDiff = (avgHPDiff * (win + loss - 1) + value) / (win + loss);
    }
}