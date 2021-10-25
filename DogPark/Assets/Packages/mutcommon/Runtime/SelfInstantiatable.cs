using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfInstantiatable : MonoBehaviour
{

  public void InstantiateSelf(Transform location)
  {
    if (location == null)
    {
      // TODO: add uber debug as dependency?
      //UberDebug.LogErrorChannel(this, "MutCommon", "Null location passed to instantiate self");
      return;
    }
    Instantiate(this, location.position, location.rotation);
  }
}
