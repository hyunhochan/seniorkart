using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerRankInGame : MonoBehaviour
{
    public static PlayerRankInGame Instance;
    public GameObject playerBackgroundPrefab; // 프리팹 참조
    public Transform rankingUIParent; // RankingUI 부모 트랜스폼

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
        // 기존 랭킹 UI 삭제
        foreach (Transform child in rankingUIParent)
        {
            Destroy(child.gameObject);
        }
        playerRankUIs.Clear();

        // 랭킹 정보 업데이트
        foreach (var playerRankInfo in playerRankInfos)
        {
            GameObject playerRankUI = Instantiate(playerBackgroundPrefab, rankingUIParent);
            TMP_Text[] texts = playerRankUI.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 2)
            {
                texts[0].text = $"{playerRankInfo.Rank}"; // Rank 텍스트
                if (playerRankInfo.ClientId == localClientId)
                {
                    texts[1].text = $"Player{playerRankInfo.ClientId} (You)"; // 플레이어 이름 텍스트
                }
                else
                {
                    texts[1].text = $"Player{playerRankInfo.ClientId}"; // 플레이어 이름 텍스트
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