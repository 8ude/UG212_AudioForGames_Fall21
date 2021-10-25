using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using System.Linq;
using FMODUnity;

public class SetFmodParam : MonoBehaviour
{
  public bool onStart;
  public bool onUpdate;

  public string paramName;
  public float paramValue; 
  public StudioEventEmitter[] fmodEmitters;

  void Start() {
    if (onStart) {
      SetParams();
    }
  }

  void Update() {
    if (onUpdate) {
      SetParams();
    }
  }

  void SetParams() { 
    foreach (StudioEventEmitter e in fmodEmitters) {
      e.SetParameter(paramName, paramValue);
    }
  }
}