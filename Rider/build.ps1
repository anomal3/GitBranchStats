<#
.SYNOPSIS
    Builds the Git Branch Stats Rider plugin and copies the result to the Build folder.

.PARAMETER Configuration
    "Release" (default) or "Debug".

.PARAMETER OutDir
    Output folder relative to the project root. Default: Build.

.PARAMETER Task
    Gradle task to run. Default: buildPlugin.
    Other values: runIde, verifyPlugin, test.

.EXAMPLE
    .\build.ps1
    .\build.ps1 -Configuration Debug
    .\build.ps1 -Task runIde
#>
[CmdletBinding()]
param(
    [ValidateSet("Release","Debug")]
    [string]$Configuration = "Release",
    [string]$OutDir = "Build",
    [string]$Task = "buildPlugin"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot

# --- Check gradlew.bat --------------------------------------------------------
$gradlew = Join-Path $root "gradlew.bat"
if (-not (Test-Path $gradlew)) {
    throw "gradlew.bat not found at: $gradlew"
}

# --- Download gradle-wrapper.jar if missing -----------------------------------
$wrapperJar = Join-Path $root "gradle\wrapper\gradle-wrapper.jar"
if (-not (Test-Path $wrapperJar)) {
    Write-Host "gradle-wrapper.jar not found -- downloading..." -ForegroundColor Yellow
    $jarUrl = "https://github.com/gradle/gradle/raw/v9.0.0/gradle/wrapper/gradle-wrapper.jar"
    Invoke-WebRequest -Uri $jarUrl -OutFile $wrapperJar -UseBasicParsing
    Write-Host "Downloaded gradle-wrapper.jar" -ForegroundColor Green
}

# --- Locate JDK 17 or 21 (Gradle does not support Java 22+) ------------------
function Get-JavaMajorVersion([string]$javaExe) {
    # java -version writes to stderr; capture via temp file to avoid ErrorRecord wrapping
    $tmp = [System.IO.Path]::GetTempFileName()
    try {
        Start-Process -FilePath $javaExe -ArgumentList "-version" `
            -RedirectStandardError $tmp -NoNewWindow -Wait
        $line = Get-Content $tmp -ErrorAction SilentlyContinue | Select-Object -First 1
        return [regex]::Match($line, '"(\d+)').Groups[1].Value
    } finally {
        Remove-Item $tmp -ErrorAction SilentlyContinue
    }
}

function Find-SupportedJava {
    # Check JAVA_HOME first
    if ($env:JAVA_HOME -and (Test-Path "$env:JAVA_HOME\bin\java.exe")) {
        $major = Get-JavaMajorVersion "$env:JAVA_HOME\bin\java.exe"
        if ($major -and [int]$major -le 21) { return $env:JAVA_HOME }
    }

    # Search JBR bundled with JetBrains IDEs
    $candidates = @(
        "C:\Program Files\JetBrains\JetBrains Rider 2025.3*\jbr",
        "C:\Program Files\JetBrains\JetBrains Rider 2025.2*\jbr",
        "C:\Program Files\JetBrains\JetBrains Rider 2025.1*\jbr",
        "C:\Program Files\JetBrains\IntelliJ IDEA*\jbr"
    )
    foreach ($pattern in $candidates) {
        $match = Get-Item $pattern -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($match -and (Test-Path "$($match.FullName)\bin\java.exe")) {
            return $match.FullName
        }
    }

    # Fall back to java on PATH if version is acceptable
    $javaCmd = Get-Command java -ErrorAction SilentlyContinue
    if ($javaCmd) {
        $major = Get-JavaMajorVersion $javaCmd.Source
        if ($major -and [int]$major -le 21) {
            return Split-Path (Split-Path $javaCmd.Source)
        }
    }

    return $null
}

$javaHome = Find-SupportedJava
if (-not $javaHome) {
    Write-Host ""
    Write-Host "ERROR: No suitable JDK found (need 17 or 21)." -ForegroundColor Red
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  1. Set JAVA_HOME to a JDK 17 or 21 installation."
    Write-Host "  2. Install JetBrains Rider or IntelliJ IDEA (JBR is used automatically)."
    Write-Host "  3. Download JDK 21 from https://adoptium.net and add it to PATH."
    exit 1
}

$env:JAVA_HOME = $javaHome

Write-Host ""
Write-Host "JAVA_HOME : $javaHome" -ForegroundColor DarkGray
Write-Host "Task      : $Task"        -ForegroundColor DarkGray
Write-Host ""

# --- Run Gradle ---------------------------------------------------------------
$stopwatch = [System.Diagnostics.Stopwatch]::StartNew()

Write-Host "Running: gradlew $Task ..." -ForegroundColor Cyan
& $gradlew $Task "--no-daemon"
$exitCode = $LASTEXITCODE

$stopwatch.Stop()
$elapsed = $stopwatch.Elapsed.ToString("mm\:ss")

if ($exitCode -ne 0) {
    Write-Host ""
    Write-Host "BUILD FAILED (exit $exitCode) after $elapsed." -ForegroundColor Red
    exit $exitCode
}

# --- Copy output to Build/ (only for buildPlugin) -----------------------------
if ($Task -eq "buildPlugin") {
    $distDir  = Join-Path $root "build\distributions"
    $artifact = Get-ChildItem $distDir -Filter "*.zip" -ErrorAction SilentlyContinue |
                Sort-Object LastWriteTime -Descending |
                Select-Object -First 1

    if (-not $artifact) {
        throw "Build succeeded but no .zip found in $distDir"
    }

    $outPath = Join-Path $root $OutDir
    New-Item -ItemType Directory -Force -Path $outPath | Out-Null
    $dest = Join-Path $outPath $artifact.Name
    Copy-Item $artifact.FullName -Destination $dest -Force

    $sizeMb = [math]::Round($artifact.Length / 1MB, 2)
    Write-Host ""
    Write-Host "Done in $elapsed. Plugin -> $dest  ($sizeMb MB)" -ForegroundColor Green
    Write-Host ""
    Write-Host "Install: Rider -> Settings -> Plugins -> gear icon -> Install Plugin from Disk -> select the .zip" -ForegroundColor DarkGray
} else {
    Write-Host ""
    Write-Host "Done in $elapsed." -ForegroundColor Green
}
