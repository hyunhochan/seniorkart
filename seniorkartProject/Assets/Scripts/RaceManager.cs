using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    public List<Checkpoint> checkpoints = new List<Checkpoint>();
    private Dictionary<GameObject, int> playerCheckpoints = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, float> playerDistances = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> playerFinished = new Dictionary<GameObject, bool>();
    private Dictionary<GameObject, HashSet<int>> playerPassedCheckpoints = new Dictionary<GameObject, HashSet<int>>();
    private Dictionary<GameObject, float> playerFinishTimes = new Dictionary<GameObject, float>();

    public GameObject RaceResultUI; // 결과 창 UI
    public TextMeshProUGUI countdownText; // 10초 카운트다운 텍스트

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

    public Transform[] GetCheckpoints()
    {
        Transform[] checkpointTransforms = new Transform[checkpoints.Count];
        for (int i = 0; i < checkpoints.Count; i++)
        {
            checkpointTransforms[i] = checkpoints[i].transform;
        }
        return checkpointTransforms;
    }

    public void PlayerPassedCheckpoint(GameObject player, int checkpointIndex)
    {
        if (!playerCheckpoints.ContainsKey(player))
        {
            playerCheckpoints[player] = -1;
            playerFinished[player] = false;
            playerPassedCheckpoints[player] = new HashSet<int>();
        }

        if (checkpointIndex == 0 && playerCheckpoints[player] == checkpoints.Count - 1)
        {
            playerCheckpoints[player] = 0;
            playerFinished[player] = true;
            float finishTime = Time.timeSinceLevelLoad;
            playerFinishTimes[player] = finishTime;
            PlayerFinishedRace(player, finishTime);
        }
        else if (checkpointIndex == playerCheckpoints[player] + 1)
        {
            playerCheckpoints[player] = checkpointIndex;
            playerPassedCheckpoints[player].Add(checkpointIndex);
        }

        UpdatePlayerRanks();
    }

    public void UpdatePlayerProgress(GameObject player, int checkpointIndex, float distanceToNextCheckpoint)
    {
        if (!playerCheckpoints.ContainsKey(player))
        {
            playerCheckpoints[player] = checkpointIndex;
            playerFinished[player] = false;
            playerPassedCheckpoints[player] = new HashSet<int>();
        }

        playerDistances[player] = distanceToNextCheckpoint;

        UpdatePlayerRanks();
    }

    private void PlayerFinishedRace(GameObject player, float finishTime)
    {
        var networkObject = player.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            PlayerFinishedServerRpc(networkObject.OwnerClientId, finishTime); // 수정: 서버 RPC 호출
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerFinishedServerRpc(ulong clientId, float finishTime)
    {
        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        playerFinishTimes[player] = finishTime;
        playerFinished[player] = true;

        PlayerFinishedClientRpc(clientId, finishTime);

        if (AllPlayersFinished())
        {
            StartCoroutine(ShowRaceResultAfterDelay(10f));
        }
    }

    [ClientRpc]
    private void PlayerFinishedClientRpc(ulong clientId, float finishTime)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"You have finished the race! Your time: {finishTime:F2} seconds");
            CharacterSpawner.Instance.UpdateFinishTimeUI(finishTime);
        }
    }

    private IEnumerator ShowRaceResultAfterDelay(float delay)
    {
        float countdown = delay;
        while (countdown > 0)
        {
            countdownText.text = $"Results in {countdown:F0} seconds";
            yield return new WaitForSeconds(1f);
            countdown -= 1f;
        }

        RaceResultUI.SetActive(true);
        countdownText.gameObject.SetActive(false);
    }

    private bool AllPlayersFinished()
    {
        foreach (var finished in playerFinished.Values)
        {
            if (!finished)
            {
                return false;
            }
        }
        return true;
    }

    private void UpdatePlayerRanks()
    {
        List<GameObject> players = new List<GameObject>(playerCheckpoints.Keys);
        players.Sort((p1, p2) =>
        {
            bool finished1 = playerFinished.ContainsKey(p1) && playerFinished[p1];
            bool finished2 = playerFinished.ContainsKey(p2) && playerFinished[p2];

            if (finished1 && !finished2)
            {
                return -1;
            }
            else if (!finished1 && finished2)
            {
                return 1;
            }

            int cp1 = playerCheckpoints.ContainsKey(p1) ? playerCheckpoints[p1] : -1;
            int cp2 = playerCheckpoints.ContainsKey(p2) ? playerCheckpoints[p2] : -1;

            if (cp1 == cp2)
            {
                float dist1 = playerDistances.ContainsKey(p1) ? playerDistances[p1] : float.MaxValue;
                float dist2 = playerDistances.ContainsKey(p2) ? playerDistances[p2] : float.MaxValue;
                return dist1.CompareTo(dist2);
            }
            return cp2.CompareTo(cp1);
        });

        List<PlayerRankInfo> playerRankInfos = new List<PlayerRankInfo>();
        for (int i = 0; i < players.Count; i++)
        {
            var networkObject = players[i].GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                playerRankInfos.Add(new PlayerRankInfo
                {
                    ClientId = networkObject.OwnerClientId,
                    PlayerName = players[i].name,
                    Rank = i + 1
                });
            }
        }

        UpdatePlayerRanksClientRpc(playerRankInfos.ToArray());
    }

    [ClientRpc]
    private void UpdatePlayerRanksClientRpc(PlayerRankInfo[] playerRankInfos)
    {
        PlayerRankInGame.Instance.UpdateRanks(playerRankInfos);
    }

    public List<GameObject> GetRankedPlayers()
    {
        List<GameObject> players = new List<GameObject>(playerCheckpoints.Keys);
        players.Sort((p1, p2) =>
        {
            bool finished1 = playerFinished.ContainsKey(p1) && playerFinished[p1];
            bool finished2 = playerFinished.ContainsKey(p2) && playerFinished[p2];

            if (finished1 && !finished2)
            {
                return -1;
            }
            else if (!finished1 && finished2)
            {
                return 1;
            }

            int cp1 = playerCheckpoints.ContainsKey(p1) ? playerCheckpoints[p1] : -1;
            int cp2 = playerCheckpoints.ContainsKey(p2) ? playerCheckpoints[p2] : -1;

            if (cp1 == cp2)
            {
                float dist1 = playerDistances.ContainsKey(p1) ? playerDistances[p1] : float.MaxValue;
                float dist2 = playerDistances.ContainsKey(p2) ? playerDistances[p2] : float.MaxValue;
                return dist1.CompareTo(dist2);
            }
            return cp2.CompareTo(cp1);
        });
        return players;
    }

    public Transform GetPlayerCheckpointTransform(GameObject player)
    {
        if (playerCheckpoints.ContainsKey(player) && playerCheckpoints[player] >= 0)
        {
            return checkpoints[playerCheckpoints[player]].transform;
        }
        return null;
    }

    [System.Serializable]
    public struct PlayerRankInfo : INetworkSerializable
    {
        public ulong ClientId;
        public string PlayerName;
        public int Rank;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref ClientId);
            serializer.SerializeValue(ref PlayerName);
            serializer.SerializeValue(ref Rank);
        }
    }
}