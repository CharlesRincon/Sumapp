using UnityEngine;
using TMPro;
using System.Threading.Tasks;
using System.Linq;

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

    /// <summary>
    /// Método seguro para salir del lobby y volver al menú principal.
    /// Llama a LeaveRoom() en el NetworkManager antes de mostrar el menú.
    /// </summary>
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

    public void UpdatePlayersCount()
    {
        if (lobbyPlayersText != null && NetworkManager.Instance?.Runner != null)
        {
            int current = NetworkManager.Instance.Runner.ActivePlayers.Count();
            int max = NetworkManager.Instance.Runner.SessionInfo?.MaxPlayers ?? 0;
            lobbyPlayersText.text = $"{current}/{max}";
        }
    }
}
