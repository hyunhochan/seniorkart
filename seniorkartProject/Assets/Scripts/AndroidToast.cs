using UnityEngine;

public class AndroidToast : MonoBehaviour
{
    private static AndroidToast instance;

    public static AndroidToast Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<AndroidToast>();
                if (instance == null)
                {
                    instance = new GameObject("AndroidToast").AddComponent<AndroidToast>();
                }
            }
            return instance;
        }
    }

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public static void Show(string message)
    {
        Instance.ShowMessage(message);
    }

    private void ShowMessage(string message)
    {
        if (Application.platform == RuntimePlatform.Android)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity, message, 3);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
