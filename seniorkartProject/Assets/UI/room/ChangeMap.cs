using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMap : MonoBehaviour
{
    public GameObject ChangeMapSequence; // 활성화/비활성화할 오브젝트
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void appearChangeUI()
    {
        ChangeMapSequence.SetActive(!ChangeMapSequence.activeSelf);

    }

    public void disappearChangeUI()
    {
        ChangeMapSequence.SetActive(false);
    }
}
