using Fusion;
using UnityEngine;

public class PlayerDataNetwork : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public int CharacterIndex { get; set; } = -1;

    public static PlayerDataNetwork Local { get; private set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            Local = this;
        }

        Debug.Log($"Spawned PlayerDataNetwork for {Owner}");
        UIManager.Instance?.CharacterSelectUI?.UpdateButtonsState();
    }

    public void SetCharacter(int index)
    {
        if (Object.HasStateAuthority)
        {
            CharacterIndex = index;
            UIManager.Instance?.CharacterSelectUI?.UpdateButtonsState();
        }
        else
        {
            RPC_RequestSetCharacter(index);
        }
    }

    [Rpc(RpcSources.InputAuthority, RpcTargets.StateAuthority)]
    private void RPC_RequestSetCharacter(int index)
    {
        CharacterIndex = index;
        UIManager.Instance?.CharacterSelectUI?.UpdateButtonsState();
    }
}
