using System;
using UnityEngine;
using Mirror;

public class NetworkedCharacter: NetworkBehaviour {
    // -- fields --
    [Header("Children")]
    [SerializeField]
    [Tooltip("The chat.")]
    private Chat fChat;

    // -- props --
    private bool mIsChatting = true;
    private IDisposable mDisposable;

    // -- props/sync
    [SyncVar(hook = nameof(DidReceiveText))]
    [SerializeField]
    private string mChatText;

    // -- lifecycle --
    private void Start() {
        // set hierarchy name
        name = $"Player:{netId}{(isLocalPlayer ? ":local" : "")}";

        // if local player, listen for changes to chat and broadcast them
        if (isLocalPlayer) {
            mDisposable = fChat.OnChange(DidEnterText);
        }
    }

    private void Update() {
        ToggleChat();
        PushChatInput();
    }

    private void OnDestroy() {
        mDisposable?.Dispose();
    }

    // -- commands --
    // toggles chat if return is pressed (local player)
    private void ToggleChat() {
        if (!isLocalPlayer) {
            return;
        }

        var input = Inputs.Play;
        if (input.GetKeyDown(KeyCode.Return)) {
            mIsChatting = !mIsChatting;
        }
    }

    // push this frame's keyboard input, if any (local player)
    private void PushChatInput() {
        if (!isLocalPlayer) {
            return;
        }

        // push input as long as some key was pressed this frame
        var input = Inputs.Play;
        if (mIsChatting && input.anyKeyDown) {
            fChat.PushInput(input.inputString);
        }
    }

    // -- commands/sync
    [Command]
    private void CmdSetText(string text) {
        mChatText = text;
    }

    // -- events --
    // send text from the local player over-the-wire. this shouldn't be called for
    // non-local players.
    private void DidEnterText(string text) {
        if (isLocalPlayer) {
            CmdSetText(text);
        }
    }

    // receive text from non-local players over the wire and update the
    // display. this shouldn't be called for local players?
    private void DidReceiveText(string _, string text) {
        if (!isLocalPlayer) {
            fChat.PushText(text);
        }
    }
}
