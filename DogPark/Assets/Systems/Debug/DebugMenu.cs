using System;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UI = UnityEngine.UI;

[ExecuteInEditMode]
public sealed class DebugMenu: MonoBehaviour {
    // -- fields --
    [SerializeField]
    [Tooltip("If the debug menu is visible.")]
    private bool fIsVisible;

    [Header("Layout")]
    [SerializeField]
    [Tooltip("The spacing between rows.")]
    private float fSpacing = 10.0f;

    [SerializeField]
    [Tooltip("The padding at the bottom of the field list.")]
    private float fFieldListBottomPadding = 20.0f;

    // -- fields/ui
    [Header("UI")]
    [SerializeField]
    [Tooltip("A reference to the camera.")]
    private GameObjectReference fCamera;

    [SerializeField]
    [Tooltip("The toggle button.")]
    private UI.Button fButton;

    [SerializeField]
    [Tooltip("The debug panel.")]
    private GameObject fPanel;

    [SerializeField]
    [Tooltip("The field scroll view.")]
    private UI.ScrollRect fFieldsView;

    [SerializeField]
    [Tooltip("The list of fields; defaults to all children in scroll view.")]
    private DebugField[] fFields;

    // -- lifecycle --
    private void Awake() {
        // capture field references
        fFields = fFieldsView.GetComponentsInChildren<DebugField>();

        // set initial state of ui
        SetVisible(fIsVisible);
        LayoutFields();

        // bind events
        fButton.onClick.AddListener(DidClickToggle);
    }

    private void Start() {
        // configure canvas
        GetComponent<Canvas>().worldCamera = fCamera.Value.GetComponent<Camera>();
    }

    // -- commands --
    private void SetVisible(bool isVisible) {
        fIsVisible = isVisible;
        fPanel.SetActive(isVisible);
    }

    private void LayoutFields() {
        var y = 0.0f;

        // position each field
        foreach (var field in fFields) {
            // position the field
            var t = field.GetComponent<RectTransform>();
            t.anchoredPosition = new Vector2(0.0f, -y);

            // update the position of the next field
            y += t.rect.height + fSpacing;
        }

        // calculate the height of the content rect
        var height = y - fSpacing + fFieldListBottomPadding;

        // resize the content rect
        var c = fFieldsView.content;
        var s = c.sizeDelta;
        s.y = height;
        c.sizeDelta = s;

        // scroll back to top
        fFieldsView.verticalNormalizedPosition = 1.0f;
    }

    // -- events --
    private void DidClickToggle() {
        SetVisible(!fIsVisible);
    }
}
