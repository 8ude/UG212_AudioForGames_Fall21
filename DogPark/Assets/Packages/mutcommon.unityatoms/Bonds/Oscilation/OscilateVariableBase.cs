using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public abstract class OscilateVariableBase<I, R, V, C, T, P, E1, E2, F, VI> : MonoBehaviour
    where I : IInterpolator<T>, new()
    where R : AtomReference<T, P, C, V, E1, E2, F, VI>
    where V : AtomVariable<T, P, E1, E2, F>
    where P : struct, IPair<T>
    where C : AtomBaseVariable<T>
    where E1 : AtomEvent<T>
    where E2 : AtomEvent<P>
    where F : AtomFunction<T, T>
    where VI : AtomVariableInstancer<V, P, T, E1, E2, F>
  {
    [SerializeField]
    private V value;

    [SerializeField]
    private FloatReference frequency;

    [SerializeField]
    private FloatReference phase;

    [SerializeField]
    private R maxValue;

    [SerializeField]
    private R minValue;

    private readonly I interpolator = new I();

    void OnEnable()
    {
      f0 = frequency.Value;
      t0 = Time.time;
      tt = t0;
      p = 0;
    }

    float f0;
    float t0;
    float tt;
    float p;

    void Update()
    {
      var t = Time.time - t0;
      var f = frequency.Value;
      p += tt * (f - f0);
      var x = Mathf.Sin(Mathf.PI * 2 * (f * t - p) + phase * Mathf.Deg2Rad);
      f0 = f;
      tt = t;

      value.Value = interpolator.Interpolate(minValue.Value, maxValue.Value, 0.5f * x + 0.5f);
    }
  }
}
