using System.Text.Json;

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
        var songs = await JsonSerializer.DeserializeAsync<List<SongCard>>(jsonStream, Options, cancellationToken)
            .ConfigureAwait(false);

        if (songs is null)
        {
            return Array.Empty<SongCard>();
        }

        Validate(songs);
        return songs;
    }

    public static async Task<IReadOnlyList<SongCard>> LoadFromFileAsync(string path, CancellationToken cancellationToken = default)
    {
        await using var stream = File.OpenRead(path);
        return await LoadAsync(stream, cancellationToken).ConfigureAwait(false);
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
}
