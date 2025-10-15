using UnityEngine;
using TMPro;
using Vuforia;
using Fusion;
using System.Linq;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvases")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject crearSalaCanvas;
    [SerializeField] private GameObject entrarSalaCanvas;
    [SerializeField] private GameObject lobbyMenuCanvas;
    [SerializeField] private GameObject ARMenuCanvas;
    [SerializeField] private GameObject characterSelectMenuCanvas;
    public CharacterSelectUI CharacterSelectUI { get; private set; }

    [Header("Lobby UI Texts")]
    [SerializeField] private TMP_Text lobbyCodeText;
    [SerializeField] private TMP_Text lobbyRoundsText;
    [SerializeField] private TMP_Text lobbyPlayersText;
    [SerializeField] private TMP_Text lobbyCharacterText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ShowCanvas(mainMenuCanvas);
        CharacterSelectUI = characterSelectMenuCanvas.GetComponent<CharacterSelectUI>();
    }

    // ==== Manejo de Canvases ====
    private void ShowCanvas(GameObject canvas)
    {
        mainMenuCanvas.SetActive(false);
        crearSalaCanvas.SetActive(false);
        entrarSalaCanvas.SetActive(false);
        lobbyMenuCanvas.SetActive(false);
        characterSelectMenuCanvas.SetActive(false);
        ARMenuCanvas.SetActive(false);

        canvas.SetActive(true);

        // 🔁 Reiniciar UI según el canvas mostrado
        if (canvas == crearSalaCanvas)
            crearSalaCanvas.GetComponent<CreateRoomUI>()?.ResetUI();

        if (canvas == entrarSalaCanvas)
            entrarSalaCanvas.GetComponent<JoinRoomUI>()?.ResetUI();

        // Cámara AR
        if (canvas == ARMenuCanvas)
        {
            Debug.Log("Activando cámara AR...");
            if (VuforiaBehaviour.Instance != null)
                VuforiaBehaviour.Instance.enabled = true;
        }
        else
        {
            Debug.Log("Apagando cámara AR...");
            if (VuforiaBehaviour.Instance != null)
                VuforiaBehaviour.Instance.enabled = false;
        }
    }

    public void ShowMainMenu() => ShowCanvas(mainMenuCanvas);
    public void ShowCrearSala() => ShowCanvas(crearSalaCanvas);
    public void ShowEntrarSala() => ShowCanvas(entrarSalaCanvas);
    public void ShowLobbyMenu() => ShowCanvas(lobbyMenuCanvas);
    public void ShowARMenu() => ShowCanvas(ARMenuCanvas);
    public void ShowCharacterSelectMenu() => ShowCanvas(characterSelectMenuCanvas);

    public async void BackToMainMenu()
    {
        if (NetworkManager.Instance != null && NetworkManager.Instance.Runner != null)
        {
            Debug.Log("🔻 Cerrando sesión antes de volver al menú...");
            await NetworkManager.Instance.LeaveRoom();
        }

        ShowMainMenu();
    }

    // ==== Manejo de Textos del Lobby ====
    public void SetLobbyCode(string code)
    {
        if (lobbyCodeText != null)
            lobbyCodeText.text = code;
    }

    public void SetLobbyRounds(int rounds)
    {
        if (lobbyRoundsText != null)
            lobbyRoundsText.text = $"{rounds} Rondas";
    }

    /// <summary>
    /// Actualiza el texto de jugadores en tiempo real.
    /// </summary>
    public void UpdatePlayersCount()
    {
        if (lobbyPlayersText == null)
        {
            Debug.LogWarning("No se ha asignado el texto de jugadores en el inspector.");
            return;
        }

        var runner = NetworkManager.Instance?.Runner;
        if (runner == null || !runner.IsRunning)
        {
            lobbyPlayersText.text = "Jugadores: 0";
            return;
        }

        int current = runner.ActivePlayers.Count(); // ✅ más confiable que SessionInfo.PlayerCount
        int max = runner.SessionInfo != null ? runner.SessionInfo.MaxPlayers : 0;

        lobbyPlayersText.text = $"Jugadores: {current}/{max}";
        Debug.Log($"👥 Jugadores actualizados: {current}/{max}");
    }

    public void SetLobbyCharacterName(string name)
    {
        if (lobbyCharacterText != null)
            lobbyCharacterText.text = name;
    }
}
