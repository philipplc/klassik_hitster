using System.Net;
using System.Text;
using ClassicHitster.Shared;
using QRCoder;

var options = GeneratorOptions.Parse(args);
var songsPath = options.SongsPath ?? FindFileUpwards("Data", "songs.json") ?? Path.Combine(AppContext.BaseDirectory, "Data", "songs.json");
var outputRoot = options.OutputPath ?? ResolveDefaultOutputDirectory(songsPath);

if (!File.Exists(songsPath))
{
    Console.Error.WriteLine($"songs.json not found: {songsPath}");
    Console.Error.WriteLine("Usage: ClassicHitster.CardGenerator [--songs path-to-songs.json] [--out output-folder]");
    return 2;
}

Directory.CreateDirectory(outputRoot);
var qrDirectory = Path.Combine(outputRoot, "Qr");
Directory.CreateDirectory(qrDirectory);

var songs = await SongJsonLoader.LoadFromFileAsync(songsPath);
if (songs.Count == 0)
{
    Console.Error.WriteLine("songs.json is empty.");
    return 3;
}

using var qrGenerator = new QRCodeGenerator();
foreach (var song in songs)
{
    var payload = CardPayload.Create(song.Id);
    using var qrData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
    var pngQrCode = new PngByteQRCode(qrData);
    var pngBytes = pngQrCode.GetGraphic(16);
    File.WriteAllBytes(Path.Combine(qrDirectory, song.Id + ".png"), pngBytes);
}

WriteCsv(Path.Combine(outputRoot, "cards.csv"), songs);
WriteHtml(Path.Combine(outputRoot, "cards.html"), songs);

Console.WriteLine($"Generated {songs.Count} QR cards.");
Console.WriteLine($"Output: {outputRoot}");
return 0;

static string? FindFileUpwards(params string[] relativeParts)
{
    var current = new DirectoryInfo(Directory.GetCurrentDirectory());
    while (current is not null)
    {
        var candidate = Path.Combine(new[] { current.FullName }.Concat(relativeParts).ToArray());
        if (File.Exists(candidate))
        {
            return candidate;
        }

        current = current.Parent;
    }

    return null;
}

static string ResolveDefaultOutputDirectory(string songsPath)
{
    var dataDirectory = Path.GetDirectoryName(Path.GetFullPath(songsPath));
    var root = dataDirectory is null ? Directory.GetCurrentDirectory() : Directory.GetParent(dataDirectory)?.FullName;
    return Path.Combine(root ?? Directory.GetCurrentDirectory(), "CardsOutput");
}

static void WriteCsv(string path, IReadOnlyList<SongCard> songs)
{
    var builder = new StringBuilder();
    builder.AppendLine("Id;QrPayload;Composer;Title;Year;Era;Performer;AudioFile;Notes");

    foreach (var song in songs)
    {
        builder.AppendLine(string.Join(';',
            Csv(song.Id),
            Csv(CardPayload.Create(song.Id)),
            Csv(song.Composer),
            Csv(song.Title),
            Csv(song.YearDisplay),
            Csv(song.Era),
            Csv(song.Performer),
            Csv(song.AudioFile),
            Csv(song.Notes)));
    }

    File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
}

static string Csv(string? value)
{
    value ??= string.Empty;
    return '"' + value.Replace("\"", "\"\"") + '"';
}

static void WriteHtml(string path, IReadOnlyList<SongCard> songs)
{
    var builder = new StringBuilder();
    builder.AppendLine("<!doctype html>");
    builder.AppendLine("<html lang=\"de\">");
    builder.AppendLine("<head>");
    builder.AppendLine("<meta charset=\"utf-8\">");
    builder.AppendLine("<title>Classic Hitster Karten</title>");
    builder.AppendLine("<style>");
    builder.AppendLine("body{font-family:Arial,sans-serif;margin:24px;color:#1d1723;} .grid{display:grid;grid-template-columns:repeat(auto-fill,minmax(220px,1fr));gap:16px;} .card{border:1px solid #bbb;border-radius:14px;padding:14px;break-inside:avoid;page-break-inside:avoid;} img{width:150px;height:150px;display:block;margin:auto;} .id{text-align:center;font-size:12px;color:#666;margin-top:8px;} .solution{border-top:1px solid #ddd;margin-top:10px;padding-top:10px;font-size:13px;} @media print{body{margin:10mm}.card{border-color:#333}} ");
    builder.AppendLine("</style>");
    builder.AppendLine("</head>");
    builder.AppendLine("<body>");
    builder.AppendLine("<h1>Classic Hitster Karten</h1>");
    builder.AppendLine("<p>QR-Code scannen. Die Lösung steht hier nur als Druck-/Kontrollhilfe.</p>");
    builder.AppendLine("<div class=\"grid\">");

    foreach (var song in songs)
    {
        builder.AppendLine("<section class=\"card\">");
        builder.AppendLine($"<img src=\"Qr/{Html(song.Id)}.png\" alt=\"QR {Html(song.Id)}\">");
        builder.AppendLine($"<div class=\"id\">{Html(song.Id)}</div>");
        builder.AppendLine("<div class=\"solution\">");
        builder.AppendLine($"<strong>{Html(song.YearDisplay)}</strong><br>");
        builder.AppendLine($"{Html(song.Composer)}<br>");
        builder.AppendLine($"<em>{Html(song.Title)}</em>");
        if (!string.IsNullOrWhiteSpace(song.Era))
        {
            builder.AppendLine($"<br>{Html(song.Era)}");
        }
        builder.AppendLine("</div>");
        builder.AppendLine("</section>");
    }

    builder.AppendLine("</div>");
    builder.AppendLine("</body>");
    builder.AppendLine("</html>");

    File.WriteAllText(path, builder.ToString(), Encoding.UTF8);
}

static string Html(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

internal sealed record GeneratorOptions(string? SongsPath, string? OutputPath)
{
    public static GeneratorOptions Parse(string[] args)
    {
        string? songsPath = null;
        string? outputPath = null;

        for (var i = 0; i < args.Length; i++)
        {
            if (string.Equals(args[i], "--songs", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                songsPath = args[++i];
            }
            else if (string.Equals(args[i], "--out", StringComparison.OrdinalIgnoreCase) && i + 1 < args.Length)
            {
                outputPath = args[++i];
            }
        }

        return new GeneratorOptions(songsPath, outputPath);
    }
}
