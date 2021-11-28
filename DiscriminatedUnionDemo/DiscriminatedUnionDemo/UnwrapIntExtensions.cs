using static DiscriminatedUnionDemo.WithUnwrap;

namespace DiscriminatedUnionDemo.UnwrapExts {
    public static class UnwrapIntExtensions {
        public static Option<int> DivideBy(this int x, int y) =>
                y == 0
                ? Option<int>.None()
                : Option<int>.Some(x / y);

        public static int AddBy(this int x, int y) =>
            x + y;

        public static int SubtractBy(this int x, int y) =>
            x - y;

        public static int MultiplyBy(this int x, int y) =>
            x * y;

        public static Option<int> OptionDivideBy(this Option<int> x, int y) =>
            Option<int>.Bind<int, int>(z => z.DivideBy(y))(x);

        public static Option<int> OptionAddBy(this Option<int> x, int y) =>
            Option<int>.Map<int, int>(z => z.AddBy(y))(x);

        public static Option<int> OptionSubtractBy(this Option<int> x, int y) =>
            Option<int>.Map<int, int>(z => z.SubtractBy(y))(x);

        public static Option<int> OptionMultiplyBy(this Option<int> x, int y) =>
            Option<int>.Map<int, int>(z => z.MultiplyBy(y))(x);
    }
}
