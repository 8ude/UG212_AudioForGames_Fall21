using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityAtoms;

namespace MutCommon.UnityAtoms
{
  public class RaiseEventWithValue<R, V, C, T, P, E1, E2, F, VI> : MonoBehaviour
    where R : AtomReference<T, P, C, V, E1, E2, F, VI>
    where V : AtomVariable<T, P, E1, E2, F>
    where P : struct, IPair<T>
    where C : AtomBaseVariable<T>
    where E1 : AtomEvent<T>
    where E2 : AtomEvent<P>
    where F : AtomFunction<T, T>
    where VI : AtomVariableInstancer<V, P, T, E1, E2, F>
  {
    [SerializeField] private E1 eventToRaise;
    [SerializeField] private R value;

    public void Raise()
      => eventToRaise.Raise(value.Value);

    public void Raise(V variable)
      => eventToRaise.Raise(variable.Value);

    public void Raise(C constant)
      => eventToRaise.Raise(constant.Value);
  }
}