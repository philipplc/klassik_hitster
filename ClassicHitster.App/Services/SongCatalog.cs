using ClassicHitster.Shared;

namespace ClassicHitster.App.Services;

public static class SongCatalog
{
    private static readonly SemaphoreSlim LoadLock = new(1, 1);
    private static IReadOnlyList<SongCard>? songs;

    public static async Task<IReadOnlyList<SongCard>> GetSongsAsync()
    {
        if (songs is not null)
        {
            return songs;
        }

        await LoadLock.WaitAsync();
        try
        {
            if (songs is not null)
            {
                return songs;
            }

            await using var stream = await FileSystem.OpenAppPackageFileAsync("songs.json");
            songs = await SongJsonLoader.LoadAsync(stream);
            return songs;
        }
        finally
        {
            LoadLock.Release();
        }
    }

    public static async Task<SongCard?> GetSongByScannedPayloadAsync(string? scannedPayload)
    {
        var cardId = CardPayload.TryExtractCardId(scannedPayload);
        if (cardId is null)
        {
            return null;
        }

        var allSongs = await GetSongsAsync();
        return allSongs.FirstOrDefault(song => string.Equals(song.Id, cardId, StringComparison.OrdinalIgnoreCase));
    }

    public static async Task<SongCard?> GetSongByIdAsync(string? cardId)
    {
        if (string.IsNullOrWhiteSpace(cardId))
        {
            return null;
        }

        var allSongs = await GetSongsAsync();
        return allSongs.FirstOrDefault(song => string.Equals(song.Id, cardId.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
