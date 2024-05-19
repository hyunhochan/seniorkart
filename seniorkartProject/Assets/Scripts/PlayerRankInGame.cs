using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerRankInGame : MonoBehaviour
{
    public static PlayerRankInGame Instance;
    public TMP_Text rankText;

    private ulong localClientId;

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

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            localClientId = NetworkManager.Singleton.LocalClientId;
        }
    }

    public void UpdateRanks(RaceManager.PlayerRankInfo[] playerRankInfos)
    {
        rankText.text = "Rankings:\n";
        int playerRank = -1;

        foreach (var playerRankInfo in playerRankInfos)
        {
            if (playerRankInfo.ClientId == localClientId)
            {
                playerRank = playerRankInfo.Rank;
                rankText.text += $"<b>{playerRankInfo.Rank}. {playerRankInfo.PlayerName} (You)</b>\n";
            }
            else
            {
                rankText.text += $"{playerRankInfo.Rank}. {playerRankInfo.PlayerName}\n";
            }
        }

        if (playerRank > 0)
        {
            Debug.Log($"Your Rank: {playerRank}");
        }
    }
}