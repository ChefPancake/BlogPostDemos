using static DiscriminatedUnionDemo.WithStaticCtors;

namespace DiscriminatedUnionDemo.StaticCtorsExts;

public static class StaticCtorsIntExtensions {
    public static Option<int> DivideBy(this int x, int y) =>
        y == 0
        ? Option<int>.None()
        : Option<int>.Some(x / y);
}
