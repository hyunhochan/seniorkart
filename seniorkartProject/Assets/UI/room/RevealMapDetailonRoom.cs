using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using MapData;

public class RevealMapDetailRoom : MonoBehaviour
{
    public Button myButton;
    public Image targetImage;
    public Image miniMap;
    public TextMeshProUGUI thisTrackName; // 그리드 내 해당 트랙의 이름을 표시해야.
    public TextMeshProUGUI targetTrackName;
    public TextMeshProUGUI targetBestRecord;
    public TextMeshProUGUI targetBestKart;


    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(RevealDetail);
            Transform trackImageTransform = transform.Find("Track/TrackImage");
            Transform miniMapTransform = transform.Find("minimap");
            TrackInfo trackInfo = trackImageTransform.GetComponentInParent<TrackInfo>();
            thisTrackName.text = trackInfo.trackName;

        }
        else
        {
            Debug.LogError("Button component not assigned.");
        }
    }

    private void RevealDetail()
    {
        Transform trackImageTransform = transform.Find("Track/TrackImage");
        Transform miniMapTransform = transform.Find("minimap");
        if (trackImageTransform != null)
        {
            Image trackImage = trackImageTransform.GetComponent<Image>();
            Image miniMapImage = miniMapTransform.GetComponent<Image>();
            TrackInfo trackInfo = trackImageTransform.GetComponentInParent<TrackInfo>();
            if (trackImage != null && trackInfo != null)
            {
                targetImage.sprite = trackImage.sprite;
                miniMap.sprite = miniMapImage.sprite;
                targetTrackName.text = trackInfo.trackName; // 트랙 이름 업데이트
                //targetBestRecord.text = trackInfo.BestRecord1st; // 최고 기록 업데이트
                //targetBestKart.text = trackInfo.KartBody1st; // 최고 카트 업데이트
            }
            else
            {
                Debug.Log("No TrackInfo component or Image component found on 'TrackImage'.");
            }
        }
        else
        {
            Debug.Log("No child named 'TrackImage' found.");
        }
    }
}
