using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using TMPro;

public class PlayerRankInGame : MonoBehaviour
{
    public static PlayerRankInGame Instance;
    public GameObject playerBackgroundPrefab; // ������ ����
    public Transform rankingUIParent; // RankingUI �θ� Ʈ������

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
        // ���� ��ŷ UI ����
        foreach (Transform child in rankingUIParent)
        {
            Destroy(child.gameObject);
        }
        playerRankUIs.Clear();

        // ��ŷ ���� ������Ʈ
        foreach (var playerRankInfo in playerRankInfos)
        {
            GameObject playerRankUI = Instantiate(playerBackgroundPrefab, rankingUIParent);
            TMP_Text[] texts = playerRankUI.GetComponentsInChildren<TMP_Text>();

            if (texts.Length >= 2)
            {
                texts[0].text = $"{playerRankInfo.Rank}"; // Rank �ؽ�Ʈ
                if (playerRankInfo.ClientId == localClientId)
                {
                    texts[1].text = $"Player{playerRankInfo.ClientId} (You)"; // �÷��̾� �̸� �ؽ�Ʈ
                }
                else
                {
                    texts[1].text = $"Player{playerRankInfo.ClientId}"; // �÷��̾� �̸� �ؽ�Ʈ
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