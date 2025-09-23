using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Canvases")]
    [SerializeField] private GameObject mainMenuCanvas;
    [SerializeField] private GameObject crearSalaCanvas;
    [SerializeField] private GameObject entrarSalaCanvas;
    [SerializeField] private GameObject lobbyMenuCanvas;

    [Header("Lobby UI Texts")]
    [SerializeField] private TMP_Text lobbyCodeText;
    [SerializeField] private TMP_Text lobbyRoundsText;
    [SerializeField] private TMP_Text lobbyPlayersText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        ShowCanvas(mainMenuCanvas);
    }

    // ==== Manejo de Canvases ====
    private void ShowCanvas(GameObject canvas)
    {
        mainMenuCanvas.SetActive(false);
        crearSalaCanvas.SetActive(false);
        entrarSalaCanvas.SetActive(false);
        lobbyMenuCanvas.SetActive(false);

        canvas.SetActive(true);
    }

    public void ShowMainMenu() => ShowCanvas(mainMenuCanvas);
    public void ShowCrearSala() => ShowCanvas(crearSalaCanvas);
    public void ShowEntrarSala() => ShowCanvas(entrarSalaCanvas);
    public void ShowLobbyMenu() => ShowCanvas(lobbyMenuCanvas);

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

    public void UpdatePlayersCount()
    {
        if (lobbyPlayersText != null && NetworkManager.Instance?.Runner?.SessionInfo != null)
        {
            int current = NetworkManager.Instance.Runner.SessionInfo.PlayerCount;
            int max = NetworkManager.Instance.Runner.SessionInfo.MaxPlayers;
            lobbyPlayersText.text = $"{current}/{max}";
        }
    }
}
