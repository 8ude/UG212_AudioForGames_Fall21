using System;

public struct Disposable: IDisposable {
    // -- props --
    private Action mAction;

    // -- lifetime --
    private Disposable(Action action) {
        mAction = action;
    }

    // -- IDisposable --
    public void Dispose() {
        mAction?.Invoke();
        mAction = null;
    }

    // -- factories --
    public static Disposable None = new Disposable(null);

    public static Disposable From(Action action) {
        return new Disposable(action);
    }

    public static Disposable From(params IDisposable[] disposables) {
        return From(() => {
            foreach (var disposable in disposables) {
                disposable.Dispose();
            }
        });
    }
}
