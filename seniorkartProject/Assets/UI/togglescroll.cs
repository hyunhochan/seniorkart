using UnityEngine;
using UnityEngine.UI;
using TMPro; // TMP 네임스페이스 추가

public class togglescroll : MonoBehaviour
{
    public Scrollbar scrollbar;
    public TextMeshProUGUI[] texts; // TMP 텍스트 배열로 변경
    public Color activeColor = new Color(255f / 255f, 192f / 255f, 0f / 255f, 1f);
    public Color inactiveColor = Color.black; // 비활성화 텍스트 색상

    public float activeFontSize = 20f; // 활성화 텍스트 크기
    public float inactiveFontSize = 12f; // 비활성화 텍스트 크기

    void Update()
    {
        // 스크롤바 값에 따라 가장 가까운 스냅 포인트 결정
        float value = scrollbar.value;
        if (value < 0.33f)
            SetActiveText(0); // 왼쪽
        else if (value < 0.66f)
            SetActiveText(1); // 중앙
        else
            SetActiveText(2); // 오른쪽

        // 선택된 위치로 스냅
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
