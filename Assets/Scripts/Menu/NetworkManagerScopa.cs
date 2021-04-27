using UnityEngine;
using System.Collections;
using Mirror;
using System;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
public class NetworkManagerScopa : NetworkManager
{
    [SerializeField] private int minPlayers = 2;
    [Scene] [SerializeField] private string menuScene = string.Empty;
    [Scene] [SerializeField] private string gameScene = string.Empty;

    [Header("Room")]
    [SerializeField] private NetworkRoomPlayerScopa roomPlayerPrefab = null;

    [Header("Game")]
    [SerializeField] private PlayerManager playerManager = null;


    public static event Action OnClientConnected;
    public static event Action OnClientDisconnected;
    public static event Action<NetworkConnection> OnServerReadied;

    [SerializeField] GameObject playerDisconnectedMenu = null;
    [SerializeField] TextMeshProUGUI disconnetedText = null;

    public List<NetworkRoomPlayerScopa> RoomPlayers { get; } = new List<NetworkRoomPlayerScopa>();

    public List<PlayerManager> PlayerManagers { get; } = new List<PlayerManager>();

    public override void OnStartServer()
    {
        spawnPrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs").ToList();
    }

    public override void OnStartClient()
    {
        var spawnablePrefabs = Resources.LoadAll<GameObject>("SpawnablePrefabs");

        foreach (var prefab in spawnablePrefabs)
        {
            ClientScene.RegisterPrefab(prefab);
        }
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        OnClientConnected?.Invoke();
    }

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        OnClientDisconnected?.Invoke();

        playerDisconnectedMenu.SetActive(true);

        GameObject destructionObject = new GameObject();
        this.gameObject.transform.parent = destructionObject.transform;

        if (SceneManager.GetActiveScene().path == menuScene)
        {
            disconnetedText.text = "Game Cancelled";
        }
        else
        {
            disconnetedText.text = "Opponent Has Disconnected";
        }
    }

    public override void OnServerConnect(NetworkConnection conn)
    {
        if(numPlayers >= maxConnections)
        {
            conn.Disconnect();
            return;
        }

        if(SceneManager.GetActiveScene().name == menuScene)
        {
            conn.Disconnect();
            return;
        }
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        if(SceneManager.GetActiveScene().path == menuScene)
        {
            bool isLeader = RoomPlayers.Count == 0;

            NetworkRoomPlayerScopa roomPlayerInstance = Instantiate(roomPlayerPrefab);

            roomPlayerInstance.IsLeader = isLeader;

            NetworkServer.AddPlayerForConnection(conn, roomPlayerInstance.gameObject);
        }
    }

    public override void OnServerDisconnect(NetworkConnection conn)
    {
        if (conn.identity != null)
        {
            var player = conn.identity.GetComponent<NetworkRoomPlayerScopa>();

            RoomPlayers.Remove(player);

            NotifyPlayersOfReadyState();

            playerDisconnectedMenu.SetActive(true);
            GameObject destructionObject = new GameObject();
            this.gameObject.transform.parent = destructionObject.transform;
            if(SceneManager.GetActiveScene().path == menuScene)
            {
                disconnetedText.text = "Game Cancelled";
            }
            else
            {
                disconnetedText.text = "Opponent Has Disconnected";
            }
        }

        base.OnServerDisconnect(conn); 
    }

    public override void OnStopServer()
    {
        RoomPlayers.Clear();
    }

    public void NotifyPlayersOfReadyState()
    {
        foreach (var player in RoomPlayers)
        {
            player.HandleReadyToStart(IsReadyToStart());
        }
    }

    private bool IsReadyToStart()
    {
        if (numPlayers < minPlayers) { return false; }

        foreach (var player in RoomPlayers)
        {
            if (!player.IsReady) { return false; }
        }

        return true;
    }
    
    public void StartGame()
    {
        if(SceneManager.GetActiveScene().path == menuScene)
        {
            if (!IsReadyToStart()) { return; }

            ServerChangeScene("Game");
        }
    }

    public override void ServerChangeScene(string newSceneName)
    {
        for(int i = RoomPlayers.Count - 1; i >= 0; i--)
        {
            var conn = RoomPlayers[i].connectionToClient;
            var gameplayerInstance = Instantiate(playerManager);
            gameplayerInstance.SetDisplayName(RoomPlayers[i].DisplayName);

            NetworkServer.ReplacePlayerForConnection(conn, gameplayerInstance.gameObject);
        }

        base.ServerChangeScene(newSceneName);
    }

    public override void OnServerReady(NetworkConnection conn)
    {
        base.OnServerReady(conn);

        OnServerReadied?.Invoke(conn);
    }

    public override void OnServerSceneChanged(string sceneName)
    {
        if(sceneName == "Game")
        {
            foreach (var playerManager in PlayerManagers)
            {
                playerManager.GetComponent<PlayerManager>().GetObjects();
            }
        }
    }
}
