param(
    [string]$GamePath = 'C:\Program Files (x86)\Steam\steamapps\common\Gorilla Tag'
)

$ErrorActionPreference = 'Stop'
dotnet build (Join-Path $PSScriptRoot 'GorillaCosmetics.csproj') -c Release "-p:GamePath=$GamePath"
if ($LASTEXITCODE -ne 0) { throw 'build failed' }

Add-Type -AssemblyName System.IO.Compression, System.IO.Compression.FileSystem
$release = Join-Path $PSScriptRoot 'GorillaCosmetics-3.1.0.zip'
$archive = [IO.Compression.ZipArchive]::new([IO.File]::Open($release, [IO.FileMode]::Create), [IO.Compression.ZipArchiveMode]::Create)
try {
    [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, (Join-Path $PSScriptRoot 'bin\Release\netstandard2.1\GorillaCosmetics.dll'), 'BepInEx/plugins/GorillaCosmetics/GorillaCosmetics.dll') | Out-Null
    foreach ($folder in @('Hats', 'Materials')) {
        $archive.CreateEntry("BepInEx/plugins/GorillaCosmetics/$folder/") | Out-Null
        $path = Join-Path $PSScriptRoot $folder
        if (Test-Path -LiteralPath $path) {
            foreach ($file in Get-ChildItem -LiteralPath $path -File) {
                [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, "BepInEx/plugins/GorillaCosmetics/$folder/$($file.Name)") | Out-Null
            }
        }
    }
}
finally {
    $archive.Dispose()
}
Write-Output $release
