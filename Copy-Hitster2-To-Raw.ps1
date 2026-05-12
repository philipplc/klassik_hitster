# Optional: komplette Missing-Liste anhand der neuen songs.json berechnen.
if (-not [string]::IsNullOrWhiteSpace($NewSongsJson) -and (Test-Path -LiteralPath $NewSongsJson)) {
    $parsedJson = Get-Content -LiteralPath $NewSongsJson -Raw -Encoding UTF8 | ConvertFrom-Json

    # JSON-Array robust flachziehen.
    $songs = New-Object System.Collections.Generic.List[object]

    foreach ($item in @($parsedJson)) {
        if ($item -is [System.Array]) {
            foreach ($inner in $item) {
                $songs.Add($inner)
            }
        }
        else {
            $songs.Add($item)
        }
    }

    $missingAfter = New-Object System.Collections.Generic.List[object]

    foreach ($song in $songs) {
        if ($null -eq $song.id) {
            continue
        }

        $idText = [string]$song.id
        $id = 0

        if (-not [int]::TryParse($idText, [ref]$id)) {
            Write-Warning "Ungueltige ID uebersprungen: $idText"
            continue
        }

        $targetName = Get-TargetFileName -Id $id -Style $TargetNameStyle
        $targetPath = Join-Path $TargetRaw $targetName

        if (-not (Test-Path -LiteralPath $targetPath)) {
            $missingAfter.Add([PSCustomObject]@{
                Id           = $id
                ExpectedFile = $targetName
                Komponist    = $song.komponist
                Stueck       = $song.stueck
                Werk         = $song.werk
                DatumKurz    = $song.datumKurz
            })
        }
    }

    $missingPath = Join-Path $TargetRaw "raw_missing_after_hitster2.csv"
    $missingAfter | Sort-Object Id | Export-Csv -LiteralPath $missingPath -NoTypeInformation -Encoding UTF8

    Write-Host ""
    Write-Host "Fehlende Dateien nach aktuellem Raw-Abgleich: $($missingAfter.Count)"
    Write-Host $missingPath
    Write-Host ""

    $missingAfter | Sort-Object Id | Format-Table Id, ExpectedFile, Komponist, Stueck -AutoSize
}
else {
    Write-Host ""
    Write-Host "Keine NewSongsJson angegeben oder Datei nicht gefunden. Komplette Missing-Liste wurde nicht berechnet."
}