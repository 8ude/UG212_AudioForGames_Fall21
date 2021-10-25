using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  [RequireComponent(typeof(Collider))]
  public class KillBox : MonoBehaviour
  {
    public LayerMask layerMask;
    private void OnTriggerEnter(Collider other)
    {
      if ((layerMask == (layerMask | (1 << other.gameObject.layer)))) return;
      Destroy(other.gameObject);
    }
  }
}