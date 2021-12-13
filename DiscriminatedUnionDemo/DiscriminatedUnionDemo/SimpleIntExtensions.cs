using static DiscriminatedUnionDemo.Simple;

namespace DiscriminatedUnionDemo.SimpleExts;

public static class SimpleIntExtensions {
    public static Option<int> DivideBy(this int x, int y) =>
        y == 0
        ? new Option<int>() { HasValue = false }
        : new Option<int>() { HasValue = true, Value = x / y };
}
