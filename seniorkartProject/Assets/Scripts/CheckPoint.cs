using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int checkpointIndex;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger entered by: {other.name}");
        if (other.CompareTag("Player"))
        {
            Debug.Log($"Player passed checkpoint {checkpointIndex}");
            RaceManager.Instance.PlayerPassedCheckpoint(other.gameObject, checkpointIndex);
        }
    }
}