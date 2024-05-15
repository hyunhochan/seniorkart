using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [SerializeField] private Vector3[] spawnPositions; // 플레이어 스폰 위치 목록
    [SerializeField] private GameObject kartPrefab; // 카트 프리팹

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        int i = 0;
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            if (i >= spawnPositions.Length) break;

            var spawnPos = spawnPositions[i++];

            // 클라이언트 별로 카트를 생성하고 네트워크 오브젝트로 스폰
            GameObject kart = Instantiate(kartPrefab, spawnPos, Quaternion.identity);
            NetworkObject kartNetworkObject = kart.GetComponent<NetworkObject>();
            if (kartNetworkObject != null)
            {
                kartNetworkObject.SpawnAsPlayerObject(client.ClientId);
            }

            // Rigidbody 초기화
            var rigidbody = kart.GetComponent<Rigidbody>();
            if (rigidbody != null)
            {
                rigidbody.velocity = Vector3.zero;         // 속도를 0으로 설정
                rigidbody.angularVelocity = Vector3.zero;  // 각속도를 0으로 설정
                rigidbody.isKinematic = false;            // Rigidbody를 운동 상태로 설정
            }
        }
    }
}