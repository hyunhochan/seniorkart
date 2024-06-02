using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using MapData;

public class RevealMapDetail : MonoBehaviour
{
    public Button myButton;
    public Image targetImage;
    public TextMeshProUGUI thisTrackName; 
    public TextMeshProUGUI targetTrackName; 
    public TextMeshProUGUI targetBestRecord; 
    public TextMeshProUGUI targetBestKart;
    

    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(RevealDetail);
            Transform trackImageTransform = transform.Find("Track/TrackImage");
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
        if (trackImageTransform != null)
        {
            Image trackImage = trackImageTransform.GetComponent<Image>();
            TrackInfo trackInfo = trackImageTransform.GetComponentInParent<TrackInfo>();
            if (trackImage != null && trackInfo != null)
            {
                targetImage.sprite = trackImage.sprite;
                targetTrackName.text = trackInfo.trackName;
                targetBestRecord.text = trackInfo.BestRecord1st; 
                targetBestKart.text = trackInfo.KartBody1st;
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
