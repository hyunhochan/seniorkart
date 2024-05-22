using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    public List<Checkpoint> checkpoints = new List<Checkpoint>();
    private Dictionary<GameObject, int> playerCheckpoints = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, float> playerDistances = new Dictionary<GameObject, float>();
    private Dictionary<GameObject, bool> playerFinished = new Dictionary<GameObject, bool>(); // 추가

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
            playerFinished[player] = false; // 추가
        }

        if (checkpointIndex > playerCheckpoints[player])
        {
            playerCheckpoints[player] = checkpointIndex;

            // 첫번째 체크포인트에 다시 도달하면 완주로 간주
            if (checkpointIndex == 0 && playerCheckpoints[player] != -1)
            {
                playerFinished[player] = true;
                PlayerFinishedRace(player);
            }
        }

        UpdatePlayerRanks();
    }

    public void UpdatePlayerProgress(GameObject player, int checkpointIndex, float distanceToNextCheckpoint)
    {
        if (!playerCheckpoints.ContainsKey(player))
        {
            playerCheckpoints[player] = checkpointIndex;
            playerFinished[player] = false; // 추가
        }

        playerDistances[player] = distanceToNextCheckpoint;

        UpdatePlayerRanks();
    }

    private void PlayerFinishedRace(GameObject player)
    {
        // 완주한 플레이어에게 메시지 보내기
        var networkObject = player.GetComponent<NetworkObject>();
        if (networkObject != null)
        {
            PlayerFinishedClientRpc(networkObject.OwnerClientId);
        }
    }

    [ClientRpc]
    private void PlayerFinishedClientRpc(ulong clientId)
    {
        // 클라이언트에서 완주 메시지 표시하는 로직
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            Debug.Log("You have finished the race!");
            // UI 갱신 로직 추가 가능
        }
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
                return -1; // 완주한 플레이어가 더 높은 순위
            }
            else if (!finished1 && finished2)
            {
                return 1; // 완주하지 않은 플레이어가 더 낮은 순위
            }

            int cp1 = playerCheckpoints.ContainsKey(p1) ? playerCheckpoints[p1] : -1;
            int cp2 = playerCheckpoints.ContainsKey(p2) ? playerCheckpoints[p2] : -1;

            if (cp1 == cp2)
            {
                float dist1 = playerDistances.ContainsKey(p1) ? playerDistances[p1] : float.MaxValue;
                float dist2 = playerDistances.ContainsKey(p2) ? playerDistances[p2] : float.MaxValue;
                return dist1.CompareTo(dist2); // 더 가까운 사람이 높은 순위
            }
            return cp2.CompareTo(cp1); // 더 높은 체크포인트가 높은 순위
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
        // 클라이언트에서 순위를 갱신하는 로직
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