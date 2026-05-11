using System.Globalization;

namespace ClassicHitster.Shared;

public sealed class SongCard
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public required string Composer { get; init; }
    public required int Year { get; init; }
    public bool IsApproximateYear { get; init; }
    public string? Era { get; init; }
    public string? Performer { get; init; }
    public required string AudioFile { get; init; }
    public string? Notes { get; init; }
    public string? Work { get; init; }
    public string? Piece { get; init; }
    public string? ComposerShortName { get; init; }
    public string? PremierDate { get; init; }

    public string YearDisplay => IsApproximateYear
        ? $"ca. {Year.ToString(CultureInfo.InvariantCulture)}"
        : Year.ToString(CultureInfo.InvariantCulture);
}
