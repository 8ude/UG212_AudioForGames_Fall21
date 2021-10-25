using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class CursorRaycaster : Raycaster
  {
    protected override Ray Ray() => camera.ScreenPointToRay(Input.mousePosition);
  }
}