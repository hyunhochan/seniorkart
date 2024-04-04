using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;

public class FetchUserNickname : MonoBehaviour
{
    private FirebaseAuth auth;
    private FirebaseFirestore firestore;

    void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        firestore = FirebaseFirestore.DefaultInstance;

        FetchNickname();
    }

    void FetchNickname()
    {
        FirebaseUser user = auth.CurrentUser;

        if (user != null)
        {
            DocumentReference docRef = firestore.Collection("users").Document(user.UserId);
            docRef.GetSnapshotAsync().ContinueWith(task =>
            {
                if (task.IsCompleted && !task.IsFaulted && !task.IsCanceled)
                {
                    DocumentSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        Debug.Log("�г���: " + snapshot.GetValue<string>("nickname"));
                    }
                    else
                    {
                        Debug.Log("����� ������ Firestore�� �������� �ʽ��ϴ�.");
                    }
                }
                else
                {
                    Debug.LogError("Firestore���� ����� ������ �������� �� �����߽��ϴ�.");
                }
            });
        }
        else
        {
            Debug.Log("����ڰ� �α��εǾ� ���� �ʽ��ϴ�.");
        }
    }
}