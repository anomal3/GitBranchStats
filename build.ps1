<#
.SYNOPSIS
    Builds the Git Branch Stats VSIX and copies it into the Build folder.

.DESCRIPTION
    Locates MSBuild via vswhere, restores and builds GitBranchStats.csproj, then
    copies the produced .vsix into <root>\Build.

.PARAMETER Configuration
    Build configuration. Default: Release.

.PARAMETER OutDir
    Output folder (relative to the project root) for the .vsix. Default: Build.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Configuration Debug
#>
[CmdletBinding()]
param(
    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [string]$OutDir = "Build"
)

$ErrorActionPreference = "Stop"

$root    = $PSScriptRoot
$project = Join-Path $root "GitBranchStats\GitBranchStats.csproj"
$outPath = Join-Path $root $OutDir

if (-not (Test-Path $project)) {
    throw "Project not found: $project"
}

# --- Locate MSBuild via vswhere -----------------------------------------------
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
if (-not (Test-Path $vswhere)) {
    throw "vswhere.exe not found. Install Visual Studio 2019/2022/18 or the Build Tools."
}

$msbuild = & $vswhere -latest -prerelease -products * `
    -requires Microsoft.Component.MSBuild `
    -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1

if (-not $msbuild -or -not (Test-Path $msbuild)) {
    throw "MSBuild.exe not found via vswhere."
}

Write-Host "MSBuild       : $msbuild"
Write-Host "Project       : $project"
Write-Host "Configuration : $Configuration"
Write-Host ""

# --- Restore ------------------------------------------------------------------
Write-Host "Restoring packages..." -ForegroundColor Cyan
& $msbuild $project /t:Restore /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "Restore failed (exit $LASTEXITCODE)." }

# --- Build --------------------------------------------------------------------
Write-Host "Building VSIX..." -ForegroundColor Cyan
& $msbuild $project /t:Rebuild /p:Configuration=$Configuration /p:DeployExtension=false /v:minimal /nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed (exit $LASTEXITCODE)." }

# --- Collect the .vsix --------------------------------------------------------
$vsix = Join-Path $root "GitBranchStats\bin\$Configuration\GitBranchStats.vsix"
if (-not (Test-Path $vsix)) {
    throw "VSIX was not produced at: $vsix"
}

New-Item -ItemType Directory -Force -Path $outPath | Out-Null
$dest = Join-Path $outPath "GitBranchStats.vsix"
Copy-Item $vsix -Destination $dest -Force

$sizeMb = [math]::Round((Get-Item $dest).Length / 1MB, 2)
Write-Host ""
Write-Host "Done. VSIX -> $dest  ($sizeMb MB)" -ForegroundColor Green
