### Step 1: Notification Relay Server

We’ll create a simple Node.js server that will act as a relay between GitHub and your Unity application. The server will receive the webhook from GitHub and notify the Unity application.

#### 1. Create the Relay Server

1. **Initialize a Node.js Project**:
   Open a terminal and create a new directory for your project, then navigate into it:
   ```bash
   mkdir notification-relay-server
   cd notification-relay-server
   npm init -y
   ```

2. **Install Dependencies**:
   Install the required dependencies, namely Express and Socket.IO:
   ```bash
   npm install express socket.io
   ```

3. **Create the Server**:
   Create a file named `server.js` in the project directory:

   ```javascript
   const express = require('express');
   const http = require('http');
   const socketIo = require('socket.io');

   const app = express();
   const server = http.createServer(app);
   const io = socketIo(server);

   // Middleware to parse JSON payloads
   app.use(express.json());

   // Webhook endpoint to receive GitHub push events
   app.post('/webhook', (req, res) => {
       console.log('Push event received:', req.body);

       // Emit the push event to connected clients (Unity)
       io.emit('push-event', req.body);

       // Respond to GitHub
       res.status(200).send('Webhook received');
   });

   // Handle socket connection
   io.on('connection', (socket) => {
       console.log('Unity client connected');

       socket.on('disconnect', () => {
           console.log('Unity client disconnected');
       });
   });

   // Start the server
   const PORT = process.env.PORT || 3000;
   server.listen(PORT, () => {
       console.log(`Notification relay server running on port ${PORT}`);
   });
   ```

4. **Run the Server**:
   Start the server by running:
   ```bash
   node server.js
   ```
   The server will be running on `http://localhost:3000`. You can expose this to the internet using services like [ngrok](https://ngrok.com/) to get a public URL.

#### 2. Expose the Server to the Internet (Using ngrok)

To expose your server to GitHub, you need a public URL. You can use ngrok for this(You can also use cloudflare [ZEROTRUST](https://developers.cloudflare.com/cloudflare-one/connections/connect-networks/get-started/create-local-tunnel/)):

1. **Install and Run ngrok**:
   ```bash
   ngrok http 3000
   ```
   This will give you a public URL like `http://<your-ngrok-id>.ngrok.io`.

2. **Use the ngrok URL as the Payload URL**:
   In your GitHub repository settings, go to Webhooks and set the Payload URL to `http://<your-ngrok-id>.ngrok.io/webhook`. Don't forget to set content type to application/json

### Step 2: Unity Script to Listen for Events

Now, create a Unity script to listen for the push events emitted by the server.

#### 1. Set Up Socket.IO in Unity

You'll need to import a Socket.IO client for Unity. A popular choice is the [Socket.IO Unity package](https://github.com/itisnajim/SocketIOUnity).

1. **Add the Package using package manager**:
   Copy this url: https://github.com/itisnajim/SocketIOUnity.git 
   
   Then in Unity open Window -> Package Manager -> and click (+) add package from git URL... and paste it there.

#### 2. Create the Unity Script

1. **Create a Script**:
   Create a new C# script named `GitHubNotifier` and attach it to a GameObject in your scene.

   ```csharp
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
   ```

2. **Configure and Run Unity**:
   - Ensure the Unity scene has a GameObject with the `GitHubNotifier` script attached.
   - Run the Unity scene, and it should connect to the notification relay server.

### Step 3: Test the Setup

1. **Push Changes to the Repo**:
   - Push a new commit to the GitHub repository where the webhook is configured.

2. **Receive Notification in Unity**:
   - Unity should display a notification (via the `ShowNotification` method) when the push event is received.

### Summary

This setup allows your Unity application to receive real-time notifications of GitHub push events. The Node.js server acts as a relay between GitHub and Unity, using Socket.IO for real-time communication. Unity then handles the event and displays a notification.

If you want