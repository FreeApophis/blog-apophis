namespace ApophisBlog.Importer.Functional;

using System.Collections.Immutable;

/// <summary>Missing in Funcky 3.7: turning a sequence of Eithers inside out (see funcky-feedback.md).</summary>
public static class EnumerableEitherExtensions
{
    /// <summary>All Right → Right of all values in order; otherwise the first Left.</summary>
    public static Either<TLeft, IReadOnlyList<TRight>> Sequence<TLeft, TRight>(this IEnumerable<Either<TLeft, TRight>> eithers)
        where TLeft : notnull
        where TRight : notnull
        => eithers
            .Aggregate(
                Either<TLeft, ImmutableList<TRight>>.Right([]),
                (accumulated, either) => accumulated.SelectMany(values => either.Select(values.Add)))
            .Select(values => (IReadOnlyList<TRight>)values);
}
