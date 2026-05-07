# Classic Hitster

Kleines .NET-MAUI-Android-Projekt für ein privates Hitster-ähnliches Spiel mit klassischer Musik.

## Inhalt

- `ClassicHitster.App`  
  .NET MAUI Android-App: QR-Code scannen, passende lokale Audiodatei abspielen, Lösung anzeigen.

- `ClassicHitster.Shared`  
  Gemeinsames Datenmodell, JSON-Lader und QR-Payload-Format.

- `ClassicHitster.CardGenerator`  
  Console-Tool: erzeugt QR-Codes, `cards.csv` und `cards.html` aus `Data/songs.json`.

- `Data/songs.json`  
  Feste Songliste. Diese Datei ist die zentrale Datenquelle.

- `CardsOutput`  
  Bereits generierte Demo-QR-Codes aus der Beispiel-Songliste.

## Voraussetzung

- Visual Studio 2022
- Workload: `.NET Multi-platform App UI-Entwicklung`
- .NET 8 SDK / MAUI Workload
- Android Emulator oder physisches Android-Gerät mit aktivem Entwicklermodus

## Start in Visual Studio

1. ZIP entpacken.
2. `ClassicHitster.sln` in Visual Studio öffnen.
3. NuGet-Restore abwarten.
4. `ClassicHitster.App` als Startprojekt setzen.
5. Android-Gerät oder Android Emulator auswählen.
6. Starten.

Beim ersten Scan fragt Android nach Kamera-Zugriff.

## Spielablauf in der App

1. `QR-Code scannen` öffnen.
2. Karte scannen.
3. App zeigt nur `Karte erkannt`.
4. `Abspielen` drücken.
5. Spieler raten Jahr / Komponist / Titel.
6. `Auflösung anzeigen` drücken.

Es gibt keine Punkteverwaltung, keine Teams und keine Spotify-Abhängigkeit.

## Neue Songs einbauen

1. Audiodatei nach `ClassicHitster.App/Resources/Raw` kopieren.
   - Beispiel: `mahler_001.mp3`
   - Der Dateiname sollte keine Leerzeichen enthalten.

2. In `Data/songs.json` einen Eintrag ergänzen:

```json
{
  "id": "mahler_001",
  "title": "Sinfonie Nr. 5: Adagietto",
  "composer": "Gustav Mahler",
  "year": 1904,
  "isApproximateYear": false,
  "era": "Spätromantik",
  "performer": "Optional: Orchester / Dirigent / Aufnahme",
  "audioFile": "mahler_001.mp3",
  "notes": "Optionaler Hinweis"
}
```

3. Wichtig: `id` muss eindeutig sein. Der QR-Code verweist nur auf diese ID.

## QR-Codes / Karten neu erzeugen

In Visual Studio:

1. `ClassicHitster.CardGenerator` als Startprojekt setzen.
2. Starten.
3. Ausgabe landet standardmäßig in `CardsOutput`.

Oder per Terminal aus dem Projektordner:

```powershell
dotnet run --project ClassicHitster.CardGenerator
```

Optional mit expliziten Pfaden:

```powershell
dotnet run --project ClassicHitster.CardGenerator -- --songs Data\songs.json --out CardsOutput
```

Der QR-Code-Inhalt hat dieses Format:

```text
classic-hitster://card/bach_001
```

Die App akzeptiert aber auch direkt nur die rohe ID, z.B.:

```text
bach_001
```

## Druck

Für den schnellen Test kannst du `CardsOutput/cards.html` im Browser öffnen und drucken. Für schöne finale Karten kannst du die PNG-Dateien aus `CardsOutput/Qr` in dein eigenes Layout übernehmen.

## Hinweis zu den Demo-Audiodateien

Die mitgelieferten `.wav`-Dateien sind nur kurze Platzhalter-Töne, damit die App sofort etwas abspielen kann. Für das echte Spiel ersetzt du sie durch eigene kurze `.mp3`-Clips und passt `audioFile` in `Data/songs.json` an.

## APK bauen

In Visual Studio:

1. Rechtsklick auf `ClassicHitster.App`.
2. `Archive...` oder `Publish...` verwenden.
3. Release-Konfiguration auswählen.
4. APK/AAB erzeugen.

Für private Weitergabe reicht normalerweise eine signierte APK. Android muss die Installation aus unbekannten Quellen erlauben.
