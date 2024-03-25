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
        // Firebase 초기화
        string databaseUrl = "https://your-database-name.firebaseio.com/";

        // Database의 인스턴스를 생성하면서 URL을 설정합니다.
        DatabaseReference reference = FirebaseDatabase.GetInstance(databaseUrl).RootReference;
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Exception != null)
            {
                Debug.LogError($"Failed to initialize Firebase with {task.Exception}");
                return;
            }

            // 데이터베이스의 루트 참조를 가져옵니다.
            reference = FirebaseDatabase.DefaultInstance.RootReference;

            // 데이터베이스에 데이터 저장
            SaveData();
        });
    }

    void SaveData()
    {
        // 사용자 정보 객체 생성
        User user = new User("John Doe", 30);

        // 데이터베이스에 사용자 정보 저장
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