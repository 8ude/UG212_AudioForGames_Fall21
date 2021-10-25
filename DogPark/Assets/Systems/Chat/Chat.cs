using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using System.Linq;

public class Chat: MonoBehaviour {
    // -- constants --
    private const int kBufferLength = 5;

    // -- fields --
    [Header("Data")]
    [SerializeField]
    [Tooltip("The current chat text.")]
    private StringReference fText;

    [Header("UI")]
    [SerializeField]
    [Tooltip("The chat text display.")]
    private TextMeshPro fTextView;

    [SerializeField]
    [Tooltip("The camera to face towards.")]
    private GameObjectReference fCamera;

    // -- props --
    private IDisposable mDisposable;

    // -- lifecycle --
    private void Awake() {
        // bind to atom events
        mDisposable = fText.Subscribe(RenderText);
    }

    private void Update() {
        FaceCamera();
    }

    private void OnDestroy() {
        // clean up any atom listeners
        mDisposable?.Dispose();
    }

    // -- commands --
    public void PushInput(string input) {
        // make sure we have at least one valid character to add
        if (!string.IsNullOrEmpty(input) && input.Any(IsValid)) {
            fText.Value = GetUpdatedText(input);
        }
    }

    public void PushText(string text) {
        fText.Value = text;
    }

    private void RenderText(string text) {
        fTextView.text = text;
    }

    private void FaceCamera() {
        var forward = Vector3.ProjectOnPlane(fCamera.Value.transform.forward, Vector3.up).normalized;
        transform.forward = forward;
    }

    // -- queries --
    private string GetCurrentText() {
        return fText.Value;
    }

    private string GetUpdatedText(string input) {
        var text = GetCurrentText() ?? "";

        // process each character in input
        foreach (var c in input) {
            // if backspace, delete
            if (c == '\b') {
                if (!string.IsNullOrEmpty(text)) {
                    text = text.Substring(0, text.Length - 1);
                }
            }
            // otherwise, if this character is supported, concat
            else if (IsValid(c)) {
                text += c;
            }
        }

        // convert all ws to spaces
        text = Regex.Replace(text, "\\s", " ");

        // trim to end of string if too long
        var start = text.Length - kBufferLength;
        if (start >= 0) {
            text = text.Substring(start, kBufferLength);
        }

        return text;
    }

    private bool IsValid(char c) {
        return c >= 32 || c == '\b' || c == '\t';
    }

    // -- events --
    public IDisposable OnChange(Action<string> action) {
        return fText.Subscribe(action);
    }
}
