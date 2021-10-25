using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UI = UnityEngine.UI;

[ExecuteInEditMode]
public sealed class DebugIntField: DebugField {
    // -- fields --
    [Header("Data")]
    [SerializeField]
    [Tooltip("The backing int variable.")]
    private IntReference fVariable;

    [SerializeField]
    [Tooltip("The variable's name.")]
    private string fName;

    [SerializeField]
    [Tooltip("The variable's min value.")]
    private int fMin = int.MinValue;

    [SerializeField]
    [Tooltip("The variable's max value.")]
    private int fMax = int.MaxValue;

    // -- fields/ui
    [Header("UI")]
    [SerializeField]
    [Tooltip("The variable's name.")]
    private UI.Text fNameLabel;

    [SerializeField]
    [Tooltip("The text field for the current value.")]
    private UI.InputField fField;

    [SerializeField]
    [Tooltip("The button to increment the current value.")]
    private UI.Button fAddButton;

    [SerializeField]
    [Tooltip("The button to decrement the current value.")]
    private UI.Button fSubtractButton;

    // -- props --
    private IDisposable mDisposable;

    // -- lifecycle --
    private void Awake() {
        // set name in hierarchy
        name = fName.Replace(" ", "") + "Field";

        // configure name label
        fNameLabel.text = fName;

        // set initial state of ui
        DidChangeValue(fVariable.Value);

        // bind events
        mDisposable = Disposable.From(
            fVariable.Subscribe(DidChangeValue),
            fField.onValueChanged.Subscribe(DidEditText),
            fAddButton.onClick.Subscribe(DidClickAddButton),
            fSubtractButton.onClick.Subscribe(DidClickSubtractButton)
        );

    }

    private void OnDestroy() {
        mDisposable?.Dispose();
    }

    // -- commands --
    private void SetValue(int value) {
        fVariable.Value = Math.Max(Math.Min(value, fMax), fMin);
    }

    // -- events --
    private void DidChangeValue(int value) {
        fField.text = value.ToString();
    }

    private void DidClickAddButton() {
        SetValue(fVariable + 1);
    }

    private void DidClickSubtractButton() {
        SetValue(fVariable - 1);
    }

    private void DidEditText(string text) {
        if (int.TryParse(text, out var value)) {
            SetValue(value);
        }
    }
}
