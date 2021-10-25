using System;
using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

[RequireComponent(typeof(Outline))]
public class GrabOutliner : MonoBehaviour, GrabListener
{
    [Serializable]
    private class OutlineSettings {
        public BoolReference Enabled;
        public FloatReference OutlineWidth;
        public ColorReference OutlineColor;
    }

    [SerializeField] private OutlineSettings outlineNeutral;
    [SerializeField] private OutlineSettings outlineTargetted;
    [SerializeField] private OutlineSettings outlineGrabbed;

    private Outline outline;

    // Start is called before the first frame update
    private void Awake() 
    {
        outline = GetComponent<Outline>();
        SetOutlineSettings(outlineNeutral);
    }

    private void SetOutlineSettings(OutlineSettings settings) {
        outline.enabled = settings.Enabled;
        outline.OutlineWidth = settings.OutlineWidth;
        outline.OutlineColor = settings.OutlineColor;
    }

    public void OnTargetted()
    {
        SetOutlineSettings(outlineTargetted);
    }

    public void OnUntargetted()
    {
        SetOutlineSettings(outlineNeutral);
    }

    public void OnGrabbed()
    {
        SetOutlineSettings(outlineGrabbed);
    }

    public void OnReleased()
    {
        SetOutlineSettings(outlineNeutral);
    }
}
