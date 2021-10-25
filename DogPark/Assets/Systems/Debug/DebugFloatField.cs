using System;
using MutCommon.UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UI = UnityEngine.UI;

[ExecuteInEditMode]
public sealed class DebugFloatField: DebugField {
    // -- fields --
    [Header("Data")]
    [SerializeField]
    [Tooltip("The backing float variable.")]
    private FloatReference fVariable;

    [SerializeField]
    [Tooltip("The variable's name.")]
    private string fName;

    [SerializeField]
    [Tooltip("The variable's min value.")]
    private float fMin = 0.0f;

    [SerializeField]
    [Tooltip("The variable's max value.")]
    private float fMax = 1.0f;

    // -- fields/ui
    [Header("UI")]
    [SerializeField]
    [Tooltip("The variable's name.")]
    private UI.Text fNameLabel;

    [SerializeField]
    [Tooltip("The text field for the current value.")]
    private UI.InputField fField;

    [SerializeField]
    [Tooltip("The slider for the current value.")]
    private UI.Slider fSlider;

    [SerializeField]
    [Tooltip("The string format for the float; inferred if unset.")]
    private string fStringFormat;

    // -- props --
    private IDisposable mDisposable;

    // -- lifecycle --
    private void Awake() {
        // set name in hierarchy
        name = fName.Replace(" ", "") + "Field";

        // configure name label
        fNameLabel.text = fName;

        // configure slider
        fSlider.minValue = fMin;
        fSlider.maxValue = fMax;

        // infer string format based on scale
        if (string.IsNullOrEmpty(fStringFormat)) {
            fStringFormat = InferStringFormat();
        }

        // set initial state of ui
        DidChangeValue(fVariable.Value);

        // bind events
        mDisposable = Disposable.From(
            fVariable.Subscribe(DidChangeValue),
            fField.onValueChanged.Subscribe(DidEditText),
            fSlider.onValueChanged.Subscribe(DidMoveSlider)
        );
    }

    private void OnDestroy() {
        mDisposable?.Dispose();
    }

    // -- commands --
    private void SetValue(float value) {
        fVariable.Value = value;
    }

    // -- queries --
    private string InferStringFormat() {
        var scale = fMax - fMin;
        if (scale <= 1.0f) {
            return "0.000";
        } else if (scale <= 10.0f) {
            return "0.00";
        } else {
            return "0.0";
        }
    }

    // -- events --
    private void DidChangeValue(float value) {
        fSlider.value = value;

        // only update the field when it's not editing
        if (!fField.isFocused) {
            fField.text = value.ToString(fStringFormat);
        }
    }

    private void DidMoveSlider(float value) {
        SetValue(value);
    }

    private void DidEditText(string text) {
        if (!float.TryParse(text, out var value)) {
            return;
        }

        // only update the value if its within the permitted range
        if (Mathf.Approximately(value, Mathf.Clamp(value, fMin, fMax))) {
            SetValue(value);
        }
    }
}
