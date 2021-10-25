using System;
using System.Collections.Generic;
using System.Linq;

// -- type --
/// A category of desire
public enum Desire {
    Ball = 0,
    Stick,
    Pet,
}

// -- repo --
public static class Desires {
    // -- statics --
    private static readonly Desire[] kAll =
        (Desire[]) Enum.GetValues(typeof(Desire));

    // -- queries --
    /// The number of available desires.
    public static int Length => kAll.Length;

    /// Get a shuffled list of desires.
    public static IEnumerable<Desire> Shuffled() {
        var list = kAll.ToList();
        list.Shuffle();
        return list;
    }
}
