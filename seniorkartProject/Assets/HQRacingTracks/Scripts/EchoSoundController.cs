using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoSoundController : MonoBehaviour
{
    private AudioEchoFilter audioEchoFilter;
    private int count = 0;

    private void Start()
    {
        audioEchoFilter = GetComponent<AudioEchoFilter>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Echo")
            count++;
        {
            if(count % 2 == 0)
            {
                this.audioEchoFilter.wetMix = 0;
            }
            else if (count % 2 == 1)
            {
                this.audioEchoFilter.wetMix = 0.5f;
            }
        }
    }
}
