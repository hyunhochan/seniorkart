using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;  // UI 관련 기능을 사용하기 위해 필요합니다.
using TMPro;  // TMP 텍스트 관련 네임스페이스 추가
using MapData;

public class RevealMapDetailRoom : MonoBehaviour
{
    public Button myButton; // 버튼 컴포넌트에 대한 참조
    public Image targetImage; // 덧씌울 이미지 컴포넌트를 지정할 공개 변수
    public TextMeshProUGUI thisTrackName; // 그리드 내 해당 트랙의 이름을 표시해야.
    public TextMeshProUGUI targetTrackName; // 트랙 이름을 표시할 TMP Text 컴포넌트
    public TextMeshProUGUI targetBestRecord; // 최고 기록을 표시할 TMP Text 컴포넌트
    public TextMeshProUGUI targetBestKart; // 최고 카트를 표시할 TMP Text 컴포넌트


    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(RevealDetail);
            Transform trackImageTransform = transform.Find("Track/TrackImage");
            TrackInfo trackInfo = trackImageTransform.GetComponentInParent<TrackInfo>(); // TrackInfo 컴포넌트를 찾습니다.
            thisTrackName.text = trackInfo.trackName; // 그리드 내 트랙 이름 업데이트

        }
        else
        {
            Debug.LogError("Button component not assigned.");
        }
    }

    private void RevealDetail()
    {
        Transform trackImageTransform = transform.Find("Track/TrackImage");
        if (trackImageTransform != null)
        {
            Image trackImage = trackImageTransform.GetComponent<Image>();
            TrackInfo trackInfo = trackImageTransform.GetComponentInParent<TrackInfo>(); // TrackInfo 컴포넌트를 찾습니다.
            if (trackImage != null && trackInfo != null)
            {
                targetImage.sprite = trackImage.sprite;
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
