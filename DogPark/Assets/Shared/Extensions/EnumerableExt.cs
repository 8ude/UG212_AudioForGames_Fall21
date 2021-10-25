using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class EnumerableExt {
    /// Select from each item and filter nulls.
    public static IEnumerable<O> SelectNonNull<I, O>(
        this IEnumerable<I> source,
        Func<I, O> selector
    ) where O: class {
        return source.Select(selector).Where((t) => t != null);
    }

    /// Get the object with highest selected value.
    public static T MaxBy<T, U>(
        this IEnumerable<T> source,
        Func<T, U> selector
    ) where T: class where U: IComparable<U> {
        Debug.Assert(source != null, "source must not be null!");

        var match = null as T;
        var max = default(U);

        foreach (var element in source) {
            var value = selector(element);

            if (value.CompareTo(max) > 0 || match == null) {
                max = value;
                match = element;
            }
        }

        return match;
    }
}
