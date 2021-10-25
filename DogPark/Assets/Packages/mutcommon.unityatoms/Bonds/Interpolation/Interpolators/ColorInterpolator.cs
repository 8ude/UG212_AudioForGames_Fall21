using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public class ColorInterpolator : IInterpolator<Color>
  {
    public Color Interpolate(Color a, Color b, float t)
      => Color.Lerp(a, b, t);
  }
}