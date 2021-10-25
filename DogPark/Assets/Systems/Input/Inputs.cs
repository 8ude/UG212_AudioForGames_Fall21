using UnityEngine;

/// Manages the control mode and provides input wrappers that only work
/// if the corresponding mode is active.
public class Inputs: MonoBehaviour {
    // -- statics --
    private static Inputs sInputs;

    // -- props --
    private InputMode mMode = InputMode.Play;

    // -- lifecycle --
    private void Awake() {
        // store reference to the controls, there should only be on instance
        Debug.Assert(sInputs == null, "Multiple instances of Controls in scene!");
        sInputs = this;
    }

    private void Update() {
        if (IsToggleModeDown()) {
            ToggleMode();
        }
    }

    // -- commands --
    // flip to the other controls mode
    private void ToggleMode() {
        mMode = (InputMode)(((int)mMode + 1) % 2);
    }

    // -- queries --
    /// The current input mode.
    public static InputMode Mode => sInputs.mMode;

    /// An input bridge for play mode.
    public static InputBridge Play => sInputs.GetBridge(InputMode.Play);

    /// A input bridge for free mode.
    public static InputBridge Free => sInputs.GetBridge(InputMode.Free);

    // construct a bridge that is enabled if it matches the current mode
    private InputBridge GetBridge(InputMode mode) {
        return new InputBridge(mMode == mode);
    }

    // check if the toggle mode command is pressed
    private bool IsToggleModeDown() {
        return (
            Input.GetKey(KeyCode.LeftControl) &&
            Input.GetKey(KeyCode.LeftShift) &&
            Input.GetKeyDown(KeyCode.M)
        );
    }
}
