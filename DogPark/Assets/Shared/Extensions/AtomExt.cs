using System;
using UnityAtoms;

public static class AtomExt {
    // register a change handler for this reference, if possible. immediately invokes
    // the action with the current value. returns a disposable to unregister the action.
    public static IDisposable Subscribe<T, P, C, V, E1, E2, F, VI>(
        this AtomReference<T, P, C, V, E1, E2, F, VI> reference,
        Action<T> action
    )
    where P : struct, IPair<T>
    where C : AtomBaseVariable<T>
    where V : AtomVariable<T, P, E1, E2, F>
    where E1: AtomEvent<T>
    where E2: AtomEvent<P>
    where F : AtomFunction<T, T>
    where VI: AtomVariableInstancer<V, P, T, E1, E2, F> {
        // set initial state
        action(reference.Value);

        // if there is nothing to listen to, abort
        if (reference.Usage != AtomReferenceUsage.VARIABLE && reference.Usage != AtomReferenceUsage.VARIABLE_INSTANCER) {
            return Disposable.None;
        }

        // otherwise, register the listener
        var evt = reference.GetEvent<E1>();
        evt.Register(action);

        // and unregister on dispose
        return Disposable.From(() => {
            evt.Unregister(action);
        });
    }
}
