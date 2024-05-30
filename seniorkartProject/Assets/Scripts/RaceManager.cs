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

    public GameObject PlayerResultPrefab; // 플레이어 결과 프리팹
    public Transform ResultsContainer; // 결과를 담을 컨테이너

    private bool IsCountdownStarted = false; // 카운트다운 시작 여부

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
            Debug.Log($"Player {networkObject.OwnerClientId} finished the race with time {finishTime}");
            PlayerFinishedServerRpc(networkObject.OwnerClientId, finishTime);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PlayerFinishedServerRpc(ulong clientId, float finishTime)
    {
        Debug.Log($"Server received finish time from client {clientId}: {finishTime}");
        GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
        playerFinishTimes[player] = finishTime;
        playerFinished[player] = true;

        PlayerFinishedClientRpc(clientId, finishTime);

        // 첫 번째 플레이어가 결승점을 통과할 때 카운트다운 시작
        if (!IsCountdownStarted)
        {
            IsCountdownStarted = true;
            StartCoroutine(ShowRaceResultAfterDelay(10f));
            StartCountdownClientRpc(10f);
        }
    }

    [ClientRpc]
    private void PlayerFinishedClientRpc(ulong clientId, float finishTime)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log($"Client {clientId} has finished the race with time {finishTime}");
            CharacterSpawner.Instance.UpdateFinishTimeUI(finishTime);
        }
    }

    [ClientRpc]
    private void StartCountdownClientRpc(float delay)
    {
        Debug.Log("Starting countdown on client");
        StartCoroutine(ShowRaceResultAfterDelay(delay));
    }

    private IEnumerator ShowRaceResultAfterDelay(float delay)
    {
        Debug.Log("Starting countdown to show race results");
        float countdown = delay;
        while (countdown > 0)
        {
            Debug.Log($"Countdown: {countdown}");
            countdownText.text = $"Results in {countdown:F0} seconds";
            yield return new WaitForSeconds(1f);
            countdown -= 1f;
        }

        Debug.Log("Showing race results");
        ShowRaceResults(); // 결과창 업데이트
        RaceResultUI.SetActive(true);
        countdownText.gameObject.SetActive(false);
    }

    private void ShowRaceResults()
    {
        // 기존 결과 클리어
        foreach (Transform child in ResultsContainer)
        {
            Destroy(child.gameObject);
        }

        List<KeyValuePair<GameObject, float>> sortedPlayers = new List<KeyValuePair<GameObject, float>>(playerFinishTimes);
        sortedPlayers.Sort((pair1, pair2) => pair1.Value.CompareTo(pair2.Value));

        foreach (var playerRecord in sortedPlayers)
        {
            GameObject player = playerRecord.Key;
            float finishTime = playerRecord.Value;

            GameObject resultEntry = Instantiate(PlayerResultPrefab, ResultsContainer);
            TextMeshProUGUI playerNameText = resultEntry.transform.Find("CharName").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI playerTimeText = resultEntry.transform.Find("FinalRecord").GetComponent<TextMeshProUGUI>();

            var networkObject = player.GetComponent<NetworkObject>();
            if (networkObject != null)
            {
                playerNameText.text = "Player " + networkObject.OwnerClientId;
            }

            if (playerFinished[player])
            {
                playerTimeText.text = $"{finishTime:F2} seconds";
            }
            else
            {
                playerTimeText.text = "탈락";
            }
        }
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

    [ServerRpc(RequireOwnership = false)]
    public void ResetPlayerToCheckpointServerRpc(ulong clientId)
    {
        if (IsServer)
        {
            GameObject player = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject.gameObject;
            Transform checkpointTransform = GetPlayerCheckpointTransform(player);

            if (checkpointTransform != null)
            {
                ResetPlayerToCheckpointClientRpc(clientId, checkpointTransform.position, checkpointTransform.rotation);
            }
        }
    }

    [ClientRpc]
    private void ResetPlayerToCheckpointClientRpc(ulong clientId, Vector3 position, Quaternion rotation)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameObject player = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId).gameObject;
            player.transform.position = position;
            player.transform.rotation = rotation;
            Rigidbody rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
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