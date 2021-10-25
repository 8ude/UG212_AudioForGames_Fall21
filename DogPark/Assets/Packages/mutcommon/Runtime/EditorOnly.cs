using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class EditorOnly : MonoBehaviour
  {
    private void Awake()
    {
      Destroy(this.gameObject);
    }
  }
}
