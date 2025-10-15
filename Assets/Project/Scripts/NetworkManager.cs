using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Fusion;
using Fusion.Sockets;
using UnityEngine.SceneManagement;
using System.Linq;

public partial class NetworkManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public static NetworkManager Instance;

    private NetworkRunner _runner;
    public NetworkRunner Runner => _runner;

    [Header("Prefabs de red")]
    [SerializeField] private NetworkPrefabRef playerDataPrefab;

    // --- Reconexion automática ---
    private bool wasPaused = false;
    private string lastRoomCode = "";

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

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("Aplicación en pausa. Fusion podría desconectarse.");
            wasPaused = true;
        }
        else if (wasPaused)
        {
            wasPaused = false;
            Debug.Log("Volviendo a la aplicación. Verificando conexión de Fusion...");

            if (_runner == null || !_runner.IsRunning)
                TryReconnect();
        }
    }

    private async void TryReconnect()
    {
        if (string.IsNullOrEmpty(lastRoomCode))
            lastRoomCode = PlayerPrefs.GetString("LastRoomCode", "");

        if (!string.IsNullOrEmpty(lastRoomCode))
        {
            Debug.Log($"Intentando reconectar a la sala {lastRoomCode}...");
            bool success = await JoinRoomByCode(lastRoomCode);
            if (success)
                Debug.Log("Reconectado exitosamente a la sala anterior.");
            else
            {
                Debug.LogWarning("⚠️ No se pudo reconectar. Mostrando menú principal...");
                UIManager.Instance?.ShowMainMenu();
            }
        }
        else
        {
            Debug.Log("⚠️ No hay código de sala guardado, no se puede reconectar.");
            UIManager.Instance?.ShowMainMenu();
        }
    }

    // --- Utilidad: generar código de sala ---
    public string GenerateRoomCode(int length = 6)
    {
        const string chars = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";
        var rnd = new System.Random();
        var arr = new char[length];
        for (int i = 0; i < length; i++) arr[i] = chars[rnd.Next(chars.Length)];
        return new string(arr);
    }

    // --- Crear sala como host ---
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
                Debug.LogError($"StartGame failed: {result.ShutdownReason}");
                return false;
            }

            lastRoomCode = roomCode;
            PlayerPrefs.SetString("LastRoomCode", roomCode);

            UIManager.Instance?.UpdatePlayersCount();
            Debug.Log($"Sala creada: {roomCode} ({maxPlayers} jugadores, {rounds} rondas)");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Excepción en CreateRoomAsHost: {ex}");
            return false;
        }
    }

    // --- Unirse por código ---
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
            _runner.AddCallbacks(this);

            var result = await _runner.StartGame(new StartGameArgs()
            {
                GameMode = GameMode.Client,
                SessionName = roomCode
            });

            if (!result.Ok)
            {
                Debug.LogError($"Join failed: {result.ShutdownReason}");
                return false;
            }

            lastRoomCode = roomCode;
            PlayerPrefs.SetString("LastRoomCode", roomCode);

            UIManager.Instance?.UpdatePlayersCount();
            Debug.Log($"Unido a la sala: {roomCode}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Excepción en JoinRoomByCode: {ex}");
            return false;
        }
    }

    // --- NUEVO: Spawnear datos del jugador ---
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jugador conectado: {player}");
        UIManager.Instance?.UpdatePlayersCount();

        if (runner.IsServer)
        {
            var obj = runner.Spawn(playerDataPrefab, Vector3.zero, Quaternion.identity, player);
            var data = obj.GetComponent<PlayerDataNetwork>();
            data.Owner = player;
        }

        UIManager.Instance?.CharacterSelectUI?.UpdateButtonsState();
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        Debug.Log($"Jugador desconectado: {player}");
        UIManager.Instance?.UpdatePlayersCount();

        if (runner.IsServer)
        {
            foreach (var data in FindObjectsByType<PlayerDataNetwork>(FindObjectsSortMode.None))
            {
                if (data.Owner == player)
                {
                    runner.Despawn(data.Object);
                    break;
                }
            }
        }

        if (_runner != null && _runner.IsServer && !_runner.ActivePlayers.Any())
        {
            Debug.Log("No quedan jugadores, cerrando sesión...");
            _ = LeaveRoom();
        }

        UIManager.Instance?.CharacterSelectUI?.UpdateButtonsState();
    }

    // --- NUEVO: Asignar personaje a un jugador ---
    public void SetCharacterForPlayer(PlayerRef player, int index)
    {
        foreach (var data in FindObjectsByType<PlayerDataNetwork>(FindObjectsSortMode.None))
        {
            if (data.Owner == player)
            {
                data.SetCharacter(index);
                break;
            }
        }
    }

    // --- NUEVO: Saber si un personaje está tomado ---
    public bool IsCharacterTaken(int index)
    {
        foreach (var data in FindObjectsByType<PlayerDataNetwork>(FindObjectsSortMode.None))
        {
            if (data.CharacterIndex == index)
                return true;
        }
        return false;
    }

    public void ClearCharacterSelections()
    {
        var dataNetworks = FindObjectsByType<PlayerDataNetwork>(FindObjectsSortMode.None);

        foreach (var pdn in dataNetworks)
        {
            pdn.SetCharacter(-1); // Reinicia la selección de personaje en red
        }
    }

    // --- Salir de la sala ---
    public async Task LeaveRoom()
    {
        if (_runner != null)
        {
            Debug.Log("Apagando runner...");
            _runner.RemoveCallbacks(this);
            await _runner.Shutdown();

            Destroy(_runner);
            _runner = null;

            Debug.Log("Runner apagado correctamente");
        }

        ClearCharacterSelections();

        if (UIManager.Instance?.CharacterSelectUI != null)
        {
            UIManager.Instance.CharacterSelectUI.ResetSelectionUI();
        }
    }

    // --- Callbacks requeridos por INetworkRunnerCallbacks ---
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public async void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
}
