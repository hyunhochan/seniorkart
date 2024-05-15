using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3[] spawnPositions; // �÷��̾� ���� ��ġ ���
    [SerializeField] private GameObject kartPrefab; // īƮ ������

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (i >= spawnPositions.Length) break;

            var spawnPos = spawnPositions[i++];

            // Ŭ���̾�Ʈ ���� īƮ�� �����ϰ� ��Ʈ��ũ ������Ʈ�� ����
            GameObject kart = Instantiate(kartPrefab, spawnPos, Quaternion.identity);
            NetworkObject kartNetworkObject = kart.GetComponent<NetworkObject>();
            if (kartNetworkObject != null)
            {
                kartNetworkObject.SpawnAsPlayerObject(client.ClientId);
            }

            // Rigidbody �ʱ�ȭ
            var rigidbody = kart.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;         // �ӵ��� 0���� ����
                rigidbody.angularVelocity = Vector3.zero;  // ���ӵ��� 0���� ����
                rigidbody.isKinematic = false;            // Rigidbody�� � ���·� ����
            }
        }
    }
}