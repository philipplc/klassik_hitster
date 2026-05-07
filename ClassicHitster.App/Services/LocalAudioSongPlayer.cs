using ClassicHitster.Shared;
using Plugin.Maui.Audio;

namespace ClassicHitster.App.Services;

public sealed class LocalAudioSongPlayer : IDisposable
{
    private IAudioPlayer? audioPlayer;
    private Stream? audioStream;

    public async Task PlayAsync(SongCard song)
    {
        Stop();

        audioStream = await FileSystem.OpenAppPackageFileAsync(song.AudioFile);
        audioPlayer = AudioManager.Current.CreatePlayer(audioStream);
        audioPlayer.Play();
    }

    public void Pause()
    {
        audioPlayer?.Pause();
    }

    public void Resume()
    {
        audioPlayer?.Play();
    }

    public void Stop()
    {
        audioPlayer?.Stop();
        audioPlayer?.Dispose();
        audioPlayer = null;

        audioStream?.Dispose();
        audioStream = null;
    }

    public void Dispose()
    {
        Stop();
    }
}
