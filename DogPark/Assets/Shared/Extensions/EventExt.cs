using System;
using UnityEngine.Events;

public static class EventExt {
    public static IDisposable Subscribe(this UnityEvent evt, UnityAction action) {
        evt.AddListener(action);

        return Disposable.From(() => {
            evt.RemoveListener(action);
        });
    }

    public static IDisposable Subscribe<T>(this UnityEvent<T> evt, UnityAction<T> action) {
        evt.AddListener(action);

        return Disposable.From(() => {
            evt.RemoveListener(action);
        });
    }
}
