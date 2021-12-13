using static DiscriminatedUnionDemo.WithUnwrap;

namespace DiscriminatedUnionDemo;

public static partial class Simple {
    public class Option<T> {
        public T Value { get; init; }
        public bool HasValue { get; init; }
    }
}

public static partial class WithStaticCtors {
    public class Option<T> {
        public T Value { get; }
        public bool HasValue { get; }

        private Option(T value, bool hasValue) {
            Value = value;
            HasValue = hasValue;
        }

        public static Option<T> Some(T value) =>
            new Option<T>(value, true);

        public static Option<T> None() =>
            new Option<T>(default, false);
    }
}

public static class WithUnwrap {
    public record Option<T> {
        private T Value { get; }
        private bool HasValue { get; }

        private Option(T value, bool hasValue) {
            Value = value;
            HasValue = hasValue;
        }

        public static Option<T> Some(T value) =>
            new Option<T>(value, true);

        public static Option<T> None() =>
            new Option<T>(default, false);

        public void Unwrap(
                Action<T> onIsSome,
                Action onIsNone) {
            if (HasValue) {
                onIsSome(Value);
            } else {
                onIsNone();
            }
        }

        public TOut Unwrap<TOut>(
                Func<T, TOut> onIsSome,
                Func<TOut> onIsNone) =>
            HasValue
            ? onIsSome(Value)
            : onIsNone();

        public static Func<Option<TIn>, Option<TOut>> Map<TIn, TOut>(Func<TIn, TOut> f) =>
            x => x.Unwrap(
                y => Option<TOut>.Some(f(y)),
                () => Option<TOut>.None());

        public static Func<Option<TIn>, Option<TOut>> Bind<TIn, TOut>(Func<TIn, Option<TOut>> f) =>
            x => x.Unwrap(
                y => f(y),
                () => Option<TOut>.None());
    }
}

public static class OptionExtensions {
    public static Func<Option<TIn>, Option<TOut>> OptionMap<TIn, TOut>(this Func<TIn, TOut> f) =>
        Option<TOut>.Map(f);

    public static Func<Option<TIn>, Option<TOut>> OptionBind<TIn, TOut>(this Func<TIn, Option<TOut>> f) =>
        Option<TOut>.Bind(f);
}
