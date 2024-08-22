using UnityEngine;
using SocketIOClient;

public class GitHubNotifier : MonoBehaviour
{
    private SocketIO client;

    void Start()
    {
        // Initialize the Socket.IO client and connect to the relay server
        client = new SocketIO("http://localhost:3000");
        client.OnConnected += OnConnected;
        client.On("push-event", OnPushEvent);
        client.ConnectAsync();
    }

    private void OnConnected(object sender, System.EventArgs e)
    {
        Debug.Log("Connected to the notification relay server");
    }

    private void OnPushEvent(SocketIOResponse response)
    {
        // Handle the push event
        Debug.Log("New push event received from GitHub");

        // You can display a notification in Unity using UI elements
        ShowNotification("New Push to Repo!", "A new commit has been pushed to the repository.");
    }

    private void ShowNotification(string title, string message)
    {
        // Implement your custom notification UI logic here
        // For example, using UnityEngine.UI.Text to display the message
        Debug.Log($"{title}: {message}");
    }

    void OnDestroy()
    {
        if (client != null)
        {
            client.DisconnectAsync();
        }
    }
}
