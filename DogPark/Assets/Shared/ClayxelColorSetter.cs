using System.Collections;
using System.Collections.Generic;
using Clayxels;
using MutCommon.UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

[RequireComponent(typeof(ClayObject))]
public class ClayxelColorSetter : MonoBehaviour
{
  [SerializeField] private ColorReference color;

  private ClayObject clayObject;

  private void Start()
  {
    clayObject = GetComponent<ClayObject>();
    color?.GetChanged()?.Register(c => clayObject.color = c);
    if (color?.Value != null)
    {
      clayObject.color = color.Value;
    }
  }
}
