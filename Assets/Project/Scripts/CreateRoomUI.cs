using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button[] playerCountButtons;
    [SerializeField] private Button[] durationButtons;
    [SerializeField] private TMP_Text statusText;

    private int selectedPlayers = 0;
    private int selectedRounds = 0;

    private void Start()
    {
        createButton.interactable = false;

        for (int i = 0; i < playerCountButtons.Length; i++)
        {
            int count = i + 2;
            playerCountButtons[i].onClick.AddListener(() => OnSelectPlayers(count));
        }

        durationButtons[0].onClick.AddListener(() => OnSelectRounds(10));
        durationButtons[1].onClick.AddListener(() => OnSelectRounds(15));
        durationButtons[2].onClick.AddListener(() => OnSelectRounds(20));

        createButton.onClick.AddListener(OnCreateButton);
    }

    private void OnSelectPlayers(int count)
    {
        selectedPlayers = count;
        UpdateCreateButtonState();
    }

    private void OnSelectRounds(int rounds)
    {
        selectedRounds = rounds;
        UpdateCreateButtonState();
    }

    private void UpdateCreateButtonState()
    {
        createButton.interactable = (selectedPlayers > 0 && selectedRounds > 0);
    }

    private async void OnCreateButton()
    {
        if (selectedPlayers <= 0 || selectedRounds <= 0) return;

        if (NetworkManager.Instance == null)
        {
            Debug.LogWarning("No había NetworkManager, creando uno nuevo...");
            var go = new GameObject("NetworkManager");
            go.AddComponent<NetworkManager>();
        }

        var code = NetworkManager.Instance.GenerateRoomCode();
        statusText.text = "Creando sala...";

        bool ok = await NetworkManager.Instance.CreateRoomAsHost(code, selectedPlayers, selectedRounds);

        if (ok)
        {
            UIManager.Instance?.ShowCharacterSelectMenu();
            UIManager.Instance?.SetLobbyCode(code);
            UIManager.Instance?.SetLobbyRounds(selectedRounds);
            UIManager.Instance?.UpdatePlayersCount();
            statusText.text = $"Sala creada: {code}";
        }
        else
        {
            statusText.text = "Error creando sala";
        }
    }

    public void ResetUI()
    {
        selectedPlayers = 0;
        selectedRounds = 0;
        statusText.text = "";
        createButton.interactable = false;
    }
}
