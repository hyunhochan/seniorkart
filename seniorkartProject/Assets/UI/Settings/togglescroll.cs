using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class togglescroll : MonoBehaviour
{
    public Scrollbar scrollbar;
    public TextMeshProUGUI[] texts;
    public Color activeColor = new Color(255f / 255f, 192f / 255f, 0f / 255f, 1f);
    public Color inactiveColor = Color.black;

    public float activeFontSize = 20f;
    public float inactiveFontSize = 12f;

    void Update()
    {
        float value = scrollbar.value;
        if (value < 0.33f)
            SetActiveText(0);
        else if (value < 0.66f)
            SetActiveText(1);
        else
            SetActiveText(2);

        scrollbar.value = Mathf.Lerp(scrollbar.value, Mathf.Round(scrollbar.value * 3) / 3, Time.deltaTime * 10f);
    }

    void SetActiveText(int index)
    {
        for (int i = 0; i < texts.Length; i++)
        {
            texts[i].color = i == index ? activeColor : inactiveColor;
            texts[i].fontSize = i == index ? activeFontSize : inactiveFontSize;
        }
    }
}
