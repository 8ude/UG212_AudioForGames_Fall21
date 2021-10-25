namespace MutCommon.UnityAtoms
{
  public interface IInterpolator<T>
  {
    T Interpolate(T a, T b, float t);
  }
}