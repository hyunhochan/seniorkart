using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;

public class FirebaseDatabaseExample : MonoBehaviour
{
    DatabaseReference reference;

    // Start is called before the first frame update
    void Start()
    {
        // Firebase �ʱ�ȭ
        string databaseUrl = "https://your-database-name.firebaseio.com/";

        // Database�� �ν��Ͻ��� �����ϸ鼭 URL�� �����մϴ�.
        DatabaseReference reference = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            // �����ͺ��̽��� ��Ʈ ������ �����ɴϴ�.
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            // �����ͺ��̽��� ������ ����
            SaveData();
        });
    }

    void SaveData()
    {
        // ����� ���� ��ü ����
        User user = new User("John Doe", 30);

        // �����ͺ��̽��� ����� ���� ����
        string json = JsonUtility.ToJson(user);
        reference.Child("users").Child(user.name).SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
            {
                Debug.Log("Data saved successfully.");
            }
            else
            {
                Debug.LogError("Failed to save data.");
            }
        });
    }
}

[System.Serializable]
public class User
{
    public string name;
    public int age;

    public User(string name, int age)
    {
        this.name = name;
        this.age = age;
    }
}