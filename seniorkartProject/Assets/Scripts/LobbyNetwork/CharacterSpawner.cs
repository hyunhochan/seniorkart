using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using TMPro;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3[] spawnPositions; // ?? ?? ??
    [SerializeField] private GameObject kartPrefab; // ?? ???
    public TextMeshProUGUI speedText; // ?? ??? TMP ???
    private List<GameObject> karts = new List<GameObject>(); // ??? ??? ??? ???

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (i >= spawnPositions.Length) break;

            var spawnPos = spawnPositions[i++];

            // ?? ???? ?? ??? ??
            GameObject kart = Instantiate(kartPrefab, spawnPos, Quaternion.identity);
            NetworkObject kartNetworkObject = kart.GetComponent<NetworkObject>();
            if (kartNetworkObject != null)
            {
                kartNetworkObject.SpawnAsPlayerObject(client.ClientId);
                karts.Add(kart); // ??? ??? ???? ??
            }

            // Rigidbody ???
            var rigidbody = kart.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;         // ?? 0?? ???
                rigidbody.angularVelocity = Vector3.zero;  // ??? 0?? ???
                rigidbody.isKinematic = false;             // Rigidbody? ??? ?? ???
            }
        }
    }

    [ClientRpc]
    private void UpdateSpeedTextClientRpc(float speed)
    {
        if (speedText != null)
        {
            speedText.text = $"{speed:F1}";
        }
    }

    private void Update()
    {
        if (!IsServer) return; // ????? ????

        foreach (var kart in karts)
        {
            var rigidbody = kart.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                // Rigidbody? ??? km/h ??? ??
                float speed = rigidbody.velocity.magnitude * 3.6f;

                // ????? RPC? ?? ?? ?????? ?? ?? ??
                UpdateSpeedTextClientRpc(speed);
            }
        }
    }
}
