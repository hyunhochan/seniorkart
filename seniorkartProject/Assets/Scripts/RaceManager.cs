using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    public List<Checkpoint> checkpoints = new List<Checkpoint>();
    private Dictionary<GameObject, int> playerCheckpoints = new Dictionary<GameObject, int>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void PlayerPassedCheckpoint(GameObject player, int checkpointIndex)
    {
        if (!playerCheckpoints.ContainsKey(player))
        {
            playerCheckpoints[player] = -1;
        }

        Debug.Log($"{player.name} current checkpoint: {playerCheckpoints[player]}, trying to pass: {checkpointIndex}");

        // �÷��̾ �������� üũ����Ʈ�� ����ߴ��� Ȯ��
        if (checkpointIndex < playerCheckpoints[player])
        {
            Debug.Log($"{player.name} is going backwards!");
            // Netcode���� ClientId�� ���� Ư�� Ŭ���̾�Ʈ���Ը� �޽����� �����ϴ�.
            var playerNetworkObject = player.GetComponent<NetworkObject>();
            if (playerNetworkObject != null)
            {
                var clientRpcParams = new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new List<ulong> { playerNetworkObject.OwnerClientId }
                    }
                };
                BackWardWarn.Instance.ShowMessageClientRpc("You are going backwards!", clientRpcParams);
            }
        }
        else if (playerCheckpoints[player] + 1 == checkpointIndex)
        {
            playerCheckpoints[player] = checkpointIndex;
            Debug.Log($"{player.name} passed checkpoint {checkpointIndex}");
        }
        else
        {
            Debug.Log($"{player.name} passed checkpoint out of order: {checkpointIndex}");
        }
    }

    public Transform GetPlayerCheckpointTransform(GameObject player)
    {
        if (playerCheckpoints.ContainsKey(player) && playerCheckpoints[player] >= 0)
        {
            return checkpoints[playerCheckpoints[player]].transform;
        }
        return null;
    }
}