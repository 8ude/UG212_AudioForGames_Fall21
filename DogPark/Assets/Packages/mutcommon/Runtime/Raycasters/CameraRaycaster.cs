using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MutCommon
{
  public class CameraRaycaster : Raycaster
  {
    protected override Ray Ray() => new Ray(camera.transform.position, camera.transform.forward);
  }
}