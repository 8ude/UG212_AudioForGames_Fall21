using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class Vector3InterpolatorSlerp : IInterpolator<Vector3>
  {
    public Vector3 Interpolate(Vector3 a, Vector3 b, float t)
      => Vector3.Slerp(a, b, t);
  }
}