using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Linq;

public class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    private NetworkRunner _runner;
    public NetworkRunner Runner => _runner;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public string GenerateRoomCode(int length = 6)
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var rnd = new System.Random();
        var arr = new char[length];
        for (int i = 0; i < length; i++) arr[i] = chars[rnd.Next(chars.Length)];
        return new string(arr);
    }

    public async Task<bool> CreateRoomAsHost(string roomCode, int maxPlayers, int rounds)
    {
        try
        {
            if (_runner != null)
            {
                await _runner.Shutdown();
                _runner = null;
            }

            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true;
            _runner.AddCallbacks(this);

            var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            var startArgs = new StartGameArgs()
            {
                GameMode = GameMode.Host,
                SessionName = roomCode,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = sceneManager,
                PlayerCount = maxPlayers,
                SessionProperties = new Dictionary<string, SessionProperty>
                {
                    { "Rounds", (SessionProperty)rounds }
                }
            };

            var result = await _runner.StartGame(startArgs);

            if (!result.Ok)
            {
                Debug.LogError($"⚠️ StartGame failed: {result.ShutdownReason}");
                return false;
            }

            UIManager.Instance?.UpdatePlayersCount();
            Debug.Log($"✅ Sala creada: {roomCode} ({maxPlayers} jugadores, {rounds} rondas)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Excepción en CreateRoomAsHost: {ex}");
            return false;
        }
    }

    public async Task<bool> JoinRoomByCode(string roomCode)
    {
        try
        {
            if (_runner != null)
            {
                await _runner.Shutdown();
                _runner = null;
            }

            _runner = gameObject.AddComponent<NetworkRunner>();
            _runner.ProvideInput = true; // <- necesario si el cliente va a mandar input
            _runner.AddCallbacks(this);

            var sceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>();

            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = roomCode,
                Scene = SceneRef.FromIndex(SceneManager.GetActiveScene().buildIndex),
                SceneManager = sceneManager
            });

            if (!result.Ok)
            {
                Debug.LogError($"⚠️ Join failed: {result.ShutdownReason}");
                return false;
            }

            UIManager.Instance?.UpdatePlayersCount();
            Debug.Log($"✅ Unido a la sala: {roomCode}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ Excepción en JoinRoomByCode: {ex}");
            return false;
        }
    }

    public async Task LeaveRoom()
    {
        if (_runner != null)
        {
            Debug.Log("🔻 Apagando runner...");
            await _runner.Shutdown();
            _runner = null;
            Debug.Log("✅ Runner apagado correctamente");

            // 🔄 Actualiza la UI al salir
            UIManager.Instance?.UpdatePlayersCount();
        }
    }

    // -------------------------------
    // INetworkRunnerCallbacks
    // -------------------------------
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"👤 Jugador conectado: {player}");
        UIManager.Instance?.UpdatePlayersCount();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"👤 Jugador desconectado: {player}");
        UIManager.Instance?.UpdatePlayersCount();

        if (_runner != null && _runner.IsServer && !_runner.ActivePlayers.Any())
        {
            Debug.Log("❌ No quedan jugadores, cerrando sesión...");
            _ = LeaveRoom();
        }
    }

    // Los demás callbacks pueden quedar vacíos
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player){ }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data){ }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress){ }
}
