using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class IntInterpolator : IInterpolator<int>
  {
    public int Interpolate(int a, int b, float t)
      => (int)Mathf.Floor(Mathf.Lerp(a, b + 1, t));
  }
}