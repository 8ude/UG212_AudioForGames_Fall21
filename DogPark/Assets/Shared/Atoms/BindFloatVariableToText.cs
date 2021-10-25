using MutCommon.UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

public class BindFloatVariableToText: MonoBehaviour {
    // -- fields --
    [SerializeField]
    [Tooltip("The variable to bind to the text field.")]
    private FloatReference fVariable;

    // -- props --
    private Text mText;

    // -- lifecycle --
    protected void Awake() {
        // capture component refs
        mText = GetComponent<Text>();

        // set text in response to variable change
        SetText(fVariable.Value);
        fVariable.GetChanged()?.Register(SetText);
    }

    // -- commands --
    private void SetText(float value) {
        mText.text = value.ToString();
    }
}
