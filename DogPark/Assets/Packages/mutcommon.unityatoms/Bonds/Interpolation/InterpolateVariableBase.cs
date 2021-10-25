using UnityAtoms;
using UnityAtoms.BaseAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public abstract class InterpolateVariableBase<I, R, V, C, T, P, E1, E2, F, VI> : MonoBehaviour
    where I : IInterpolator<T>, new()
    where R : AtomReference<T, P, C, V, E1, E2, F, VI>
    where V : AtomVariable<T, P, E1, E2, F>
    where C : AtomBaseVariable<T>
    where P : struct, IPair<T>
    where E1 : AtomEvent<T>
    where E2 : AtomEvent<P>
    where F : AtomFunction<T, T>
    where VI : AtomVariableInstancer<V, P, T, E1, E2, F>
  {
    [SerializeField]
    private FloatReference interpolation;

    [SerializeField]
    private R value0;

    [SerializeField]
    private R value1;

    [SerializeField]
    private V output;

    [SerializeField]
    private bool useCurve;

    // TODO: make curve only show when useCurve is set
    [SerializeField]
    private AnimationCurve curve;

    private readonly I interpolator = new I();

    void Update() => output.Value = interpolator.Interpolate(value0.Value, value1.Value, useCurve ? curve.Evaluate(interpolation.Value) : interpolation.Value);
  }
}