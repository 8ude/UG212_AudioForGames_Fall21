using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class ScreenSpaceRaycaster : Raycaster
  {
    protected override Ray Ray()
    {
      var pos3d = transform.position;
      var pos2d = camera.WorldToScreenPoint(pos3d);
      var ray = camera.ScreenPointToRay(pos2d);
      return ray;
    }
  }
}