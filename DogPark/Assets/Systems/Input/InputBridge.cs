using UnityEngine;

/// A bridge for Unity's built-in Input type that only works in the corresponding
/// mode is set.
public readonly struct InputBridge {
    // -- props --
    private readonly bool mIsEnabled;

    // -- lifetime --
    public InputBridge(bool isEnabled) {
        mIsEnabled = isEnabled;
    }

    // -- queries --
    // -- queries/keys
    public bool anyKeyDown
        => mIsEnabled && Input.anyKeyDown;

    public bool GetKey(KeyCode code) {
        return mIsEnabled && Input.GetKey(code);
    }

    public bool GetKeyDown(KeyCode code) {
        return mIsEnabled && Input.GetKeyDown(code);
    }

    public bool GetKeyUp(KeyCode code) {
        return mIsEnabled && Input.GetKeyUp(code);
    }

    // -- queries/axes
    public float GetAxis(string name) {
        return mIsEnabled ? Input.GetAxis(name) : 0.0f;
    }

    // -- queries/mouse
    public Vector3 mousePosition
        // TODO: this isn't right, we'd probably need to freeze the input state
        => mIsEnabled ? Input.mousePosition : Vector3.zero;

    public Vector2 mouseScrollDelta
        => mIsEnabled ? Input.mouseScrollDelta : Vector2.zero;

    public bool GetMouseButton(int i) {
        return mIsEnabled && Input.GetMouseButton(i);
    }

    public bool GetMouseButtonDown(int i) {
        return mIsEnabled && Input.GetMouseButtonDown(i);
    }
    public bool GetMouseButtonUp(int i) {
        return mIsEnabled && Input.GetMouseButtonUp(i);
    }

    // -- queries/text
    public string inputString
        => mIsEnabled ? Input.inputString : "";
}
