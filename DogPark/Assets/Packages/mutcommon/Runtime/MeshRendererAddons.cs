using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace MutCommon
{
  [RequireComponent(typeof(MeshRenderer))]
  public class MeshRendererAddons : MonoBehaviour
  {
    private MeshRenderer _meshRenderer;
    private MeshRenderer meshRenderer => _meshRenderer ?? GetComponent<MeshRenderer>();

    private void Awake()
    {
      _meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetEmissionColor(Color color)
    {
      meshRenderer.material.SetColor("_EmissionColor", color);
    }

    public void SetEmissionIntensity(float intensity)
    {
      var color = meshRenderer.material.GetColor("_EmissionColor");
      Debug.LogError("NAO FUNCIONA AINDA");
      meshRenderer.material.SetColor("_EmissionColor", color * Mathf.LinearToGammaSpace(intensity));
    }
  }
}