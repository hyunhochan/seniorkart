using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class tabtoggle : MonoBehaviour
{
    public Toggle[] toggles; // 모든 Toggle 요소를 저장할 배열
    public Sprite selectedSprite; // 선택된 상태의 스프라이트
    public Sprite unselectedSprite; // 선택되지 않은 상태의 스프라이트
    public GameObject optiontab0; // 
    public GameObject optiontab1; // 활성화/비활성화할 오브젝트
    public GameObject optiontab2; // 활성화/비활성화할 오브젝트


    void OnEnable()
    {
        toggles[0].isOn = true;
        ToggleClick();

        // 각 Toggle에 대한 이벤트 리스너 추가
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(delegate {
                ToggleClick();
            });
        }
    }

    public void ToggleClick()
    {
        // 각 탭의 상태에 따라 optiontab을 활성화 또는 비활성화
        optiontab0.SetActive(toggles[0].isOn);
        optiontab1.SetActive(toggles[1].isOn);
        optiontab2.SetActive(toggles[2].isOn);

        // Toggle의 스프라이트와 텍스트 색상 변경
        foreach (var toggle in toggles)
        {
            var textComponent = toggle.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                textComponent.color = toggle.isOn ? new Color(1f, 0.7529412f, 0f, 1f) : new Color(178f / 255f, 178f / 255f, 178f / 255f, 1f);
            }

            toggle.GetComponent<Image>().sprite = toggle.isOn ? selectedSprite : unselectedSprite;
        }
    }




}
