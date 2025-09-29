using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoinRoomUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField codeInput;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text statusText;

    private void Start()
    {
        joinButton.onClick.AddListener(OnJoin);
    }

    private async void OnJoin()
    {
        var code = codeInput.text.Trim();
        if (string.IsNullOrEmpty(code))
        {
            statusText.text = "⚠️ Introduce un código de sala.";
            return;
        }

        if (NetworkManager.Instance == null)
        {
            Debug.LogWarning("⚠️ No había NetworkManager, creando uno nuevo...");
            var go = new GameObject("NetworkManager");
            go.AddComponent<NetworkManager>();
        }

        statusText.text = "Conectando...";
        bool ok = await NetworkManager.Instance.JoinRoomByCode(code);

        if (ok)
        {
            UIManager.Instance?.ShowLobbyMenu();
            UIManager.Instance?.SetLobbyCode(code);

            if (NetworkManager.Instance.Runner != null)
            {
                var session = NetworkManager.Instance.Runner.SessionInfo;
                if (session != null && session.Properties.ContainsKey("Rounds"))
                {
                    int rounds = (int)session.Properties["Rounds"];
                    UIManager.Instance.SetLobbyRounds(rounds);
                }
            }

            UIManager.Instance?.UpdatePlayersCount();
            statusText.text = $"✅ Unido a {code}";
        }
        else
        {
            statusText.text = "❌ No se pudo unir a la sala.";
        }
    }
}
