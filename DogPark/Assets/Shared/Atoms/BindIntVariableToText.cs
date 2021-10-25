using MutCommon.UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.UI;

public class BindIntVariableToText: MonoBehaviour {
    // -- fields --
    [SerializeField]
    [Tooltip("The variable to bind to the text field.")]
    private IntReference fVariable;

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
    private void SetText(int value) {
        mText.text = value.ToString();
    }
}
