using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeMap : MonoBehaviour
{
    public bool isopen = false;
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
        if (isopen==false) {
        ChangeMapSequence.SetActive(!ChangeMapSequence.activeSelf);
        isopen = true;
        }

    }

    public void disappearChangeUI()
    {
        if (isopen == true)
        {
            ChangeMapSequence.SetActive(false);
            isopen = false;
        }
    }
}
