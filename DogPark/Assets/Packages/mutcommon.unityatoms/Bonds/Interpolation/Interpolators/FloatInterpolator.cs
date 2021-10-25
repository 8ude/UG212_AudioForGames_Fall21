using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class FloatInterpolator : IInterpolator<float>
  {
    public float Interpolate(float a, float b, float t)
      => Mathf.Lerp(a, b, t);
  }
}