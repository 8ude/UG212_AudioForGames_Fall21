using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class Vector2Interpolator : IInterpolator<Vector2>
  {
    public Vector2 Interpolate(Vector2 a, Vector2 b, float t)
      => Vector2.Lerp(a, b, t);
  }
}