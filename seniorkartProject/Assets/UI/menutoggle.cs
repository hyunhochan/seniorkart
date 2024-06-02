using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class menutoggle : MonoBehaviour
{
    public Toggle[] toggles;
    public Sprite selectedSprite;
    public Sprite unselectedSprite;

    void Start()
    {
        foreach (var toggle in toggles)
        {
            toggle.onValueChanged.AddListener(delegate {
                ToggleClick();
            });
        }
    }

    public void ToggleClick()
    {
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
