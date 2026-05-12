param(
    [string]$SourceRawAlt = "C:\Code\JKH\ClassicHitster.App\Resources\RawAlt",
    [string]$TargetRaw = "C:\Code\JKH\ClassicHitster.App\Resources\Raw",
    [string]$NewSongsJson = "C:\Code\JKH\Data\songs.json",
    [ValidateSet("Numeric", "Classic")]
    [string]$TargetNameStyle = "Numeric",
    [switch]$Apply,
    [switch]$ClearTarget
)

$ErrorActionPreference = "Stop"

function Get-TargetFileName {
    param([int]$Id)

    if ($TargetNameStyle -eq "Classic") {
        return "classic_{0:000}.mp3" -f $Id
    }

    return "$Id.mp3"
}

function Read-JsonUtf8 {
    param([string]$Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return @()
    }

    return @(Get-Content -LiteralPath $Path -Raw -Encoding UTF8 | ConvertFrom-Json)
}

if (-not (Test-Path -LiteralPath $SourceRawAlt)) {
    throw "RawAlt-Ordner nicht gefunden: $SourceRawAlt"
}

if (-not (Test-Path -LiteralPath $TargetRaw)) {
    New-Item -ItemType Directory -Path $TargetRaw | Out-Null
}

if ($ClearTarget -and $Apply) {
    Get-ChildItem -LiteralPath $TargetRaw -Filter "*.mp3" -File -ErrorAction SilentlyContinue |
        Remove-Item -Force
}

# NewId = neue numerische ID aus der finalen JSON.
# SourceFiles = alte Datei(en) aus RawAlt. Bei entfernten Duplikaten stehen Alternativen in Prioritaetsreihenfolge.
# Beispiel: NewId 100 nimmt classic_022.mp3; falls die fehlt, classic_103.mp3.
$MappingCsv = @'
NewId,SourceFiles
1,classic_166.mp3
2,classic_164.mp3
3,classic_165.mp3
4,classic_025.mp3
6,classic_161.mp3
8,classic_178.mp3
9,classic_015.mp3
10,classic_019.mp3
11,classic_163.mp3
12,classic_044.mp3
13,classic_045.mp3
14,classic_047.mp3
15,classic_048.mp3
16,classic_190.mp3
17,classic_049.mp3
18,classic_050.mp3
19,classic_051.mp3
20,classic_052.mp3
21,classic_053.mp3
22,classic_173.mp3
23,classic_055.mp3
24,classic_056.mp3
25,classic_021.mp3
26,"classic_054.mp3;classic_183.mp3"
27,classic_180.mp3
28,classic_058.mp3
29,classic_027.mp3
30,classic_194.mp3
31,classic_158.mp3
33,classic_028.mp3
34,classic_057.mp3
36,classic_060.mp3
37,classic_059.mp3
38,classic_001.mp3
39,classic_061.mp3
40,classic_062.mp3
41,classic_063.mp3
42,classic_066.mp3
43,classic_176.mp3
44,classic_065.mp3
45,classic_064.mp3
46,classic_067.mp3
48,classic_020.mp3
49,classic_070.mp3
50,classic_009.mp3
51,classic_014.mp3
52,classic_079.mp3
53,classic_076.mp3
54,classic_071.mp3
55,classic_086.mp3
56,classic_087.mp3
57,classic_160.mp3
58,classic_072.mp3
59,classic_073.mp3
60,classic_075.mp3
61,classic_191.mp3
62,classic_077.mp3
63,classic_016.mp3
64,classic_081.mp3
65,classic_082.mp3
66,classic_083.mp3
67,classic_069.mp3
68,classic_195.mp3
69,classic_182.mp3
70,classic_171.mp3
71,classic_078.mp3
72,classic_074.mp3
73,classic_085.mp3
74,classic_029.mp3
75,classic_088.mp3
76,classic_089.mp3
77,classic_004.mp3
78,classic_080.mp3
79,classic_084.mp3
80,classic_090.mp3
81,classic_101.mp3
82,classic_026.mp3
83,classic_091.mp3
84,classic_092.mp3
85,classic_188.mp3
86,classic_181.mp3
87,classic_008.mp3
88,classic_093.mp3
89,classic_192.mp3
90,classic_187.mp3
91,classic_189.mp3
92,classic_159.mp3
93,classic_095.mp3
94,classic_097.mp3
95,classic_096.mp3
96,classic_094.mp3
97,classic_010.mp3
98,classic_099.mp3
99,classic_068.mp3
100,"classic_022.mp3;classic_103.mp3"
101,classic_104.mp3
102,classic_032.mp3
103,classic_030.mp3
104,classic_193.mp3
105,classic_102.mp3
106,classic_108.mp3
107,classic_105.mp3
108,classic_006.mp3
109,classic_098.mp3
110,classic_106.mp3
111,classic_107.mp3
112,classic_007.mp3
113,classic_023.mp3
114,classic_005.mp3
115,classic_110.mp3
116,classic_111.mp3
117,classic_112.mp3
118,classic_036.mp3
119,classic_117.mp3
120,classic_113.mp3
121,classic_114.mp3
122,classic_120.mp3
123,classic_037.mp3
124,classic_115.mp3
125,classic_040.mp3
126,classic_116.mp3
127,classic_118.mp3
128,classic_002.mp3
130,classic_123.mp3
131,classic_109.mp3
132,classic_185.mp3
133,classic_121.mp3
134,classic_122.mp3
136,classic_124.mp3
137,classic_100.mp3
138,classic_024.mp3
139,classic_169.mp3
140,classic_003.mp3
141,classic_125.mp3
142,classic_126.mp3
143,classic_041.mp3
144,classic_127.mp3
145,classic_128.mp3
146,classic_129.mp3
147,classic_130.mp3
148,classic_012.mp3
149,classic_131.mp3
150,classic_132.mp3
151,classic_167.mp3
152,classic_119.mp3
153,classic_135.mp3
154,classic_136.mp3
155,classic_134.mp3
156,classic_179.mp3
157,classic_018.mp3
158,classic_137.mp3
159,classic_138.mp3
160,classic_133.mp3
161,classic_042.mp3
162,classic_139.mp3
163,classic_046.mp3
164,classic_140.mp3
165,"classic_162.mp3;classic_196.mp3"
166,classic_170.mp3
167,classic_141.mp3
169,"classic_035.mp3;classic_142.mp3"
170,classic_143.mp3
171,classic_145.mp3
172,classic_034.mp3
173,classic_144.mp3
174,classic_146.mp3
175,classic_147.mp3
176,classic_151.mp3
177,classic_149.mp3
178,classic_152.mp3
179,classic_148.mp3
180,classic_153.mp3
181,classic_154.mp3
182,classic_150.mp3
183,classic_168.mp3
184,classic_033.mp3
185,classic_011.mp3
186,classic_186.mp3
187,classic_031.mp3
189,classic_017.mp3
190,classic_013.mp3
192,classic_039.mp3
195,classic_184.mp3
196,classic_177.mp3
198,classic_038.mp3
199,classic_155.mp3
200,classic_175.mp3
201,classic_172.mp3
202,classic_157.mp3
203,classic_156.mp3
204,classic_043.mp3
'@

$mappingRows = @($MappingCsv | ConvertFrom-Csv)
$mappingByNewId = @{}
foreach ($row in $mappingRows) {
    $mappingByNewId[[int]$row.NewId] = @($row.SourceFiles -split ';' | ForEach-Object { $_.Trim() } | Where-Object { $_ })
}

$newSongs = Read-JsonUtf8 -Path $NewSongsJson
if ($newSongs.Count -eq 0) {
    Write-Warning "Neue songs.json nicht gefunden oder leer. Es wird nur anhand der Mapping-Tabelle kopiert. Pfad: $NewSongsJson"
    $newSongs = @(
        1..204 | ForEach-Object {
            [PSCustomObject]@{
                id = $_
                stueck = $null
                werk = $null
                komponist = $null
            }
        }
    )
}

$allSourceFiles = @(Get-ChildItem -LiteralPath $SourceRawAlt -Filter "*.mp3" -File)
$allSourceNames = @{}
foreach ($f in $allSourceFiles) {
    $allSourceNames[$f.Name] = $true
}

$usedSourceNames = @{}
$results = New-Object System.Collections.Generic.List[object]

foreach ($song in $newSongs | Sort-Object {[int]$_.id}) {
    $newId = [int]$song.id
    $targetFile = Get-TargetFileName -Id $newId
    $targetPath = Join-Path $TargetRaw $targetFile

    $status = ""
    $chosenSource = ""
    $sourceCandidates = ""
    $note = ""

    if (-not $mappingByNewId.ContainsKey($newId)) {
        $status = "MissingNoOldMapping"
        $note = "Dieses Stueck ist neu in der finalen JSON oder war in der alten Liste bewusst nicht enthalten."
    }
    else {
        $candidates = @($mappingByNewId[$newId])
        $sourceCandidates = ($candidates -join ';')

        foreach ($candidate in $candidates) {
            $candidatePath = Join-Path $SourceRawAlt $candidate
            if (Test-Path -LiteralPath $candidatePath) {
                $chosenSource = $candidate
                break
            }
        }

        if ([string]::IsNullOrWhiteSpace($chosenSource)) {
            $status = "MissingSourceFile"
            $note = "Keine der erwarteten RawAlt-Dateien existiert."
        }
        else {
            $sourcePath = Join-Path $SourceRawAlt $chosenSource
            $usedSourceNames[$chosenSource] = $true

            if ($Apply) {
                Copy-Item -LiteralPath $sourcePath -Destination $targetPath -Force
                $status = "Copied"
            }
            else {
                $status = "WouldCopy"
            }
        }
    }

    $results.Add([PSCustomObject]@{
        Status           = $status
        NewId            = $newId
        TargetFile       = $targetFile
        ChosenSourceFile = $chosenSource
        SourceCandidates = $sourceCandidates
        Stueck           = $song.stueck
        Werk             = $song.werk
        Komponist        = $song.komponist
        Note             = $note
    })
}

$reportPath = Join-Path $TargetRaw "raw_copy_report.csv"
$missingPath = Join-Path $TargetRaw "raw_missing.csv"
$unusedPath = Join-Path $TargetRaw "raw_unused_from_rawalt.csv"

$results | Export-Csv -LiteralPath $reportPath -NoTypeInformation -Encoding UTF8
$results | Where-Object { $_.Status -like "Missing*" } | Export-Csv -LiteralPath $missingPath -NoTypeInformation -Encoding UTF8

$unused = $allSourceFiles |
    Where-Object { -not $usedSourceNames.ContainsKey($_.Name) } |
    Sort-Object Name |
    ForEach-Object {
        [PSCustomObject]@{
            SourceFile = $_.Name
            FullName   = $_.FullName
            Note       = "Nicht kopiert: entweder Duplikat-Alternative, in neuer Liste entfernt, oder ohne Mapping."
        }
    }

$unused | Export-Csv -LiteralPath $unusedPath -NoTypeInformation -Encoding UTF8

$copyCount = @($results | Where-Object { $_.Status -eq "Copied" -or $_.Status -eq "WouldCopy" }).Count
$missingCount = @($results | Where-Object { $_.Status -like "Missing*" }).Count
$unusedCount = @($unused).Count

Write-Host ""
Write-Host "Fertig."
Write-Host "Neue Songs:       $($newSongs.Count)"
Write-Host "RawAlt MP3s:      $($allSourceFiles.Count)"
Write-Host "Zuordnungen:      $copyCount"
Write-Host "Fehlend:          $missingCount"
Write-Host "Unused RawAlt:    $unusedCount"
Write-Host ""
Write-Host "Report:           $reportPath"
Write-Host "Fehlende:         $missingPath"
Write-Host "Unused RawAlt:    $unusedPath"

if (-not $Apply) {
    Write-Host ""
    Write-Host "Preview-Modus: Es wurde noch nichts kopiert."
    Write-Host "Wenn raw_copy_report.csv gut aussieht, denselben Befehl mit -Apply erneut starten."
}
