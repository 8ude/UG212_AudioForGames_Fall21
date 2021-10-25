using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityAtoms.BaseAtoms;
using Mirror;
using SplineMesh;
using Random = UnityEngine.Random;

public class RandomColor : NetworkBehaviour {
    [Range(0, 1)] public float hue = -1.0f;
    [SerializeField] [Range(0, 1)] private float saturation;
    [SerializeField] [Range(0, 1)] private float brightness;

    [Tooltip("The min value in degrees of the color range to skip. Greens start at ~60.")]
    [SerializeField] private int skipMin = 60;

    [Tooltip("The max value in degrees of the color range to skip. Greens end at ~160.")]
    [SerializeField] private int skipMax = 160;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // if there is no hue set, pick one that's not green
        if (hue <= 0) {
            hue = PickRandomHue();
        }

        color = Color.HSVToRGB(hue, saturation, brightness);
    }

    public List<string> namesToIgnore;

    // Color32 packs to 4 bytes
    [SyncVar(hook = nameof(SetColor))]
    public Color32 color = Color.black;

    public ColorVariable localPlayerColor;

    public List<string> ExtraShaderFields;

    // Unity clones the material when GetComponent<Renderer>().material is called
    // Cache it here and destroy it in OnDestroy to prevent a memory leak
    List<Material> cachedMaterials;

    void SetColor(Color32 oldColor, Color32 newColor)
    {
        if (cachedMaterials == null) cachedMaterials = GetComponentsInChildren<Renderer>()
            .Where(r => !namesToIgnore.Contains(r.gameObject.name))
            .Select(r => r.material)
            .Union(GetComponentsInChildren<SplineMeshTiling>()?
            .Select(smt => smt.material))
        .ToList();
        cachedMaterials.ForEach(m => {
            m.color = newColor;
            ExtraShaderFields?.ForEach(esf => m.SetColor(esf, newColor));
        });

        if(isLocalPlayer && localPlayerColor != null) {
            localPlayerColor.Value = newColor;
        }

        //cachedMaterial.color = newColor;
    }

    void OnDestroy()
    {
        cachedMaterials.ForEach(m => Destroy(m));
    }

    // -- queries --
    private float PickRandomHue() {
        // given the remaining color span
        var length = 360;
        var span = length - (skipMax - skipMin);

        // sample a hue and normalize it
        return Mathf.Repeat(skipMax + Random.value * span, length) / length;
    }
}
