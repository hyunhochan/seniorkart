using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class RaceManager : NetworkBehaviour
{
    public static RaceManager Instance;

    public List<Checkpoint> checkpoints = new List<Checkpoint>();
    private Dictionary<GameObject, int> playerCheckpoints = new Dictionary<GameObject, int>();
    private Dictionary<GameObject, float> playerDistances = new Dictionary<GameObject, float>();

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
        }

        if (checkpointIndex > playerCheckpoints[player])
        {
            playerCheckpoints[player] = checkpointIndex;
        }

        UpdatePlayerRanks();
    }

    public void UpdatePlayerProgress(GameObject player, int checkpointIndex, float distanceToNextCheckpoint)
    {
        if (!playerCheckpoints.ContainsKey(player))
        {
            playerCheckpoints[player] = checkpointIndex;
        }

        playerDistances[player] = distanceToNextCheckpoint;

        UpdatePlayerRanks();
    }

    private void UpdatePlayerRanks()
    {
        List<GameObject> players = new List<GameObject>(playerCheckpoints.Keys);
        players.Sort((p1, p2) =>
        {
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