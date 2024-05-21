using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerRankInGame : MonoBehaviour
{
    public static PlayerRankInGame Instance;
    public GameObject playerBackgroundPrefab; // ?????? ????
    public Transform rankingUIParent; // RankingUI ???? ????????

    private ulong localClientId;
    private Dictionary<ulong, GameObject> playerRankUIs = new Dictionary<ulong, GameObject>();

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

        // Ensure the parent has the necessary components
        if (rankingUIParent != null)
        {
            var verticalLayoutGroup = rankingUIParent.GetComponent<VerticalLayoutGroup>();
            if (verticalLayoutGroup == null)
            {
                verticalLayoutGroup = rankingUIParent.gameObject.AddComponent<VerticalLayoutGroup>();
                verticalLayoutGroup.childForceExpandHeight = false;
                verticalLayoutGroup.childForceExpandWidth = false;
                verticalLayoutGroup.childControlHeight = true;
                verticalLayoutGroup.childControlWidth = true;
            }

            var contentSizeFitter = rankingUIParent.GetComponent<ContentSizeFitter>();
            if (contentSizeFitter == null)
            {
                contentSizeFitter = rankingUIParent.gameObject.AddComponent<ContentSizeFitter>();
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                contentSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            }
        }
    }

    public void UpdateRanks(RaceManager.PlayerRankInfo[] playerRankInfos)
    {
        // ?? ?? ?? ??
        Dictionary<ulong, int> currentRanks = new Dictionary<ulong, int>();
        foreach (var playerRankInfo in playerRankInfos)
        {
            currentRanks[playerRankInfo.ClientId] = playerRankInfo.Rank;
        }

        // ?? UI ???
        foreach (Transform child in rankingUIParent)
        {
            Destroy(child.gameObject);
        }
        playerRankUIs.Clear();

        // ??? UI ?? ? ????
        foreach (var playerRankInfo in playerRankInfos)
        {
            GameObject playerRankUI = Instantiate(playerBackgroundPrefab, rankingUIParent);
            TMP_Text rankText = playerRankUI.transform.Find("Rank").GetComponent<TMP_Text>();
            TMP_Text nameText = playerRankUI.transform.Find("Name").GetComponent<TMP_Text>();

            if (rankText != null)
            {
                rankText.text = $"{playerRankInfo.Rank}"; // Rank ???
            }

            if (nameText != null)
            {
                if (playerRankInfo.ClientId == localClientId)
                {
                    nameText.text = $"Player{playerRankInfo.ClientId} (You)"; // ?? ????? ??
                }
                else
                {
                    nameText.text = $"Player{playerRankInfo.ClientId}"; // ?? ????? ??
                }
            }

            playerRankUIs[playerRankInfo.ClientId] = playerRankUI;
        }


        Debug.Log($"Your Rank: {GetLocalPlayerRank(playerRankInfos)}");
    }

    private int GetLocalPlayerRank(RaceManager.PlayerRankInfo[] playerRankInfos)
    {
        foreach (var playerRankInfo in playerRankInfos)
        {
            if (playerRankInfo.ClientId == localClientId)
            {
                return playerRankInfo.Rank;
            }
        }
        return -1;
    }
}