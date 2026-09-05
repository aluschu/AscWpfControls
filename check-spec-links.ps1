# Prueft die Markdown-Verweise zwischen den Specs.
#
# Die Regel dahinter: Ein Link verspricht ein Dokument, das es gibt. Ein Name in
# Code-Auszeichnung (`trade-irgendwas.md`) ist ein Merkposten fuer eine Spec, die noch
# nicht geschrieben ist. Wer beides vermischt, sucht frueher oder spaeter nach einer
# Datei, die nie existiert hat.
#
# Anlass ist das Verschieben erledigter Specs nach Data/Specs/Done/ - dabei verschieben
# sich alle relativen Pfade mit, und zwar in beide Richtungen. Dieses Skript sagt in zwei
# Sekunden, ob dabei etwas liegengeblieben ist.
#
# Aufruf:  .\check-spec-links.ps1
# Exit 0 = alles loest auf, Exit 1 = mindestens ein toter Verweis.

$specDir = Join-Path $PSScriptRoot 'Data\Specs'
$tot = 0
$geprueft = 0

Get-ChildItem -Path $specDir -Filter *.md -Recurse | ForEach-Object {
    $datei = $_
    $inhalt = Get-Content -Path $datei.FullName -Raw -Encoding UTF8
    if (-not $inhalt) { return }

    foreach ($treffer in [regex]::Matches($inhalt, '\[([^\]]+)\]\(([^)]+\.md)\)')) {
        $ziel = $treffer.Groups[2].Value
        if ($ziel -match '^https?:') { continue }

        $geprueft++
        $voll = Join-Path $datei.DirectoryName $ziel
        if (-not (Test-Path -LiteralPath $voll)) {
            $tot++
            $von = Resolve-Path -Relative $datei.FullName
            Write-Host "TOT: $von -> $ziel" -ForegroundColor Red
        }
    }
}

if ($tot -eq 0) {
    Write-Host "$geprueft Verweise geprueft, alle loesen auf." -ForegroundColor Green
    exit 0
}

Write-Host ""
Write-Host "$tot von $geprueft Verweisen zeigen ins Leere." -ForegroundColor Red
Write-Host "Entweder die Spec anlegen - oder den Namen in Code-Auszeichnung setzen," -ForegroundColor Yellow
Write-Host "solange sie nur geplant ist: ``trade-irgendwas.md`` statt [..](..)." -ForegroundColor Yellow
exit 1
