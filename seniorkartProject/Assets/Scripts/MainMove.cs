using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class MainMove : MonoBehaviour
{
    public Button btn1;
    public GameObject mainsequence;
    public RectTransform mainRectTransform;


    // Start is called before the first frame update
    void Start()
    {
        // Button의 RectTransform을 가져옴
        RectTransform rectTransform = btn1.GetComponent<RectTransform>();

        // 높이와 너비를 출력
        float btnwidth = rectTransform.rect.width;
        float btnheight = rectTransform.rect.height;
        mainRectTransform.DOAnchorPosX(btnwidth*2/3, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
