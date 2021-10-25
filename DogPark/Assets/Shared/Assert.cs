using U = UnityEngine.Assertions;

namespace X {
    public struct Assert {
        public static T Unwrap<T>(T value) where T: class {
            U.Assert.IsNotNull(value, $"{typeof(T).Name} value was null!");
            return value;
        }

        public static T Unwrap<T>(T? value) where T: struct {
            if (value == null) {
                U.Assert.IsTrue(false, $"{typeof(T).Name} value was null!");
            }

            return (T)value;
        }
    }
}
