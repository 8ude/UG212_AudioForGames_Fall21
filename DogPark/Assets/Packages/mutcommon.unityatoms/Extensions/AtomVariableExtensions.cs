using UnityAtoms.BaseAtoms;
using UnityAtoms;
using UnityEngine;

namespace MutCommon.UnityAtoms
{
  public static class AtomVariableExtensions
  {
    public static E1 SetChangedIfNull<T, P, E1, E2, F>(this AtomVariable<T, P, E1, E2, F> variable)
      where P : struct, IPair<T>
      where E1 : AtomEvent<T>
      where E2 : AtomEvent<P>
      where F : AtomFunction<T, T>
    {
      if (variable.Changed == null)
      {
        variable.Changed = ScriptableObject.CreateInstance<E1>();
      }

      return variable.Changed;
    }
  }
}