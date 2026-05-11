using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassicHitster.Shared;

public static class SongJsonLoader
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        WriteIndented = true
    };

    public static async Task<IReadOnlyList<SongCard>> LoadAsync(Stream jsonStream, CancellationToken cancellationToken = default)
    {
        var json = await JsonSerializer.DeserializeAsync<List<SongJsonDto>>(jsonStream, Options, cancellationToken)
            .ConfigureAwait(false);

        if (json is null)
        {
            return Array.Empty<SongCard>();
        }

        var songs = json.Select(ConvertFromDto).ToList();
        Validate(songs);
        return songs;
    }

    public static async Task<IReadOnlyList<SongCard>> LoadFromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await LoadAsync(stream, cancellationToken).ConfigureAwait(false);
    }

    private static SongCard ConvertFromDto(SongJsonDto dto)
    {
        return new SongCard
        {
            Id = dto.Id?.ToString() ?? throw new InvalidOperationException("ID is required"),
            Title = dto.Stueck ?? throw new InvalidOperationException("Stueck (Title) is required"),
            Composer = dto.Komponist ?? throw new InvalidOperationException("Komponist (Composer) is required"),
            Year = dto.DatumKurz ?? throw new InvalidOperationException("DatumKurz (Year) is required"),
            IsApproximateYear = false,
            Era = null,
            Performer = null,
            AudioFile = $"{dto.Id}.mp3",
            Notes = dto.Info,
            Work = dto.Werk,
            Piece = dto.Stueck,
            ComposerShortName = dto.KompNN,
            PremierDate = dto.Urauffuehrung
        };
    }

    private static void Validate(IReadOnlyList<SongCard> songs)
    {
        var duplicateIds = songs
            .GroupBy(song => song.Id, StringComparer.OrdinalIgnoreCase)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToArray();

        if (duplicateIds.Length > 0)
        {
            throw new InvalidOperationException("Duplicate song IDs: " + string.Join(", ", duplicateIds));
        }

        foreach (var song in songs)
        {
            if (string.IsNullOrWhiteSpace(song.Id))
            {
                throw new InvalidOperationException("A song entry has an empty ID.");
            }

            if (string.IsNullOrWhiteSpace(song.Title))
            {
                throw new InvalidOperationException($"Song '{song.Id}' has an empty title.");
            }

            if (string.IsNullOrWhiteSpace(song.Composer))
            {
                throw new InvalidOperationException($"Song '{song.Id}' has an empty composer.");
            }

            if (string.IsNullOrWhiteSpace(song.AudioFile))
            {
                throw new InvalidOperationException($"Song '{song.Id}' has an empty audio file.");
            }
        }
    }

    private sealed class SongJsonDto
    {
        public int? Id { get; set; }
        public string? Stueck { get; set; }
        public string? Werk { get; set; }
        public string? Komponist { get; set; }
        public int? DatumKurz { get; set; }
        public string? Info { get; set; }
        public string? KompNN { get; set; }
        public string? Urauffuehrung { get; set; }
    }
}
