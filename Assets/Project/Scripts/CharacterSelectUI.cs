using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Fusion;

public class CharacterSelectUI : MonoBehaviour
{
    [Header("Botones de personajes")]
    [SerializeField] private Button[] characterButtons;

    [Header("Nombres de personajes")]
    [SerializeField] private string[] characterNames;

    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button confirmButton;

    [SerializeField]private int selectedCharacter = -1;

    private void Start()
    {
        confirmButton.interactable = false;
        confirmButton.onClick.AddListener(OnConfirmSelection);

        for (int i = 0; i < characterButtons.Length; i++)
        {
            int index = i;
            characterButtons[i].onClick.AddListener(() => OnSelectCharacter(index));
        }

        UpdateButtonsState();
    }

    private void OnEnable()
    {
        UpdateButtonsState();
    }

    public void OnSelectCharacter(int index)
    {
        if (NetworkManager.Instance.IsCharacterTaken(index))
        {
            statusText.text = "Ese personaje ya fue elegido.";
            return;
        }

        selectedCharacter = index;
        string name = (index >= 0 && index < characterNames.Length) ? characterNames[index] : $"Personaje {index + 1}";
        statusText.text = $"Seleccionaste a {name}";
        confirmButton.interactable = true;
    }

    private void OnConfirmSelection()
    {
        if (selectedCharacter < 0)
        {
            statusText.text = "Elige un personaje antes de confirmar.";
            return;
        }

        var player = NetworkManager.Instance.Runner.LocalPlayer;
        NetworkManager.Instance.SetCharacterForPlayer(player, selectedCharacter);

        string name = characterNames[selectedCharacter];
        confirmButton.interactable = false;

        UIManager.Instance?.SetLobbyCharacterName(name);
        UIManager.Instance?.ShowLobbyMenu();
    }

    public void UpdateButtonsState()
    {
        if (NetworkManager.Instance == null || NetworkManager.Instance.Runner == null)
            return;

        for (int i = 0; i < characterButtons.Length; i++)
        {
            bool taken = NetworkManager.Instance.IsCharacterTaken(i);
            characterButtons[i].interactable = !taken;
        }
    }

    public string[] CharacterNames => characterNames;

    public void ResetSelectionUI()
    {
        selectedCharacter = -1;
        statusText.text = "";
        confirmButton.interactable = false;
        UpdateButtonsState();
    }
}
