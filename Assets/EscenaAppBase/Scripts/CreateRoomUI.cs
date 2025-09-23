using UnityEngine;
using UnityEngine.UI;

public class CreateRoomUI : MonoBehaviour
{
    [SerializeField] private Button createButton;
    [SerializeField] private Button[] playerCountButtons;
    [SerializeField] private Button[] durationButtons;

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
            Debug.LogError("❌ NetworkManager no existe!");
            return;
        }

        var code = NetworkManager.Instance.GenerateRoomCode();
        bool ok = await NetworkManager.Instance.CreateRoomAsHost(code, selectedPlayers, selectedRounds);

        if (ok)
        {
            UIManager.Instance?.ShowLobbyMenu();
            UIManager.Instance?.SetLobbyCode(code);
            UIManager.Instance?.SetLobbyRounds(selectedRounds);
            UIManager.Instance?.UpdatePlayersCount();
        }
        else
        {
            Debug.LogError("⚠️ Error creando sala");
        }
    }
}
