[CmdletBinding(PositionalBinding=$false)]
Param(
  [switch][Alias('h')]$help,
  [switch][Alias('t')]$test,
  [ValidateSet("Debug","Release","Checked")][string[]][Alias('c')]$configuration = @("Debug"),
  [string][Alias('f')]$framework,
  [string]$vs,
  [string][Alias('v')]$verbosity = "minimal",
  [ValidateSet("Windows_NT","Linux","OSX","Browser")][string]$os,
  [switch]$allconfigurations,
  [switch]$coverage,
  [string]$testscope,
  [switch]$testnobuild,
  [ValidateSet("x86","x64","arm","arm64","wasm")][string[]][Alias('a')]$arch = @([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString().ToLowerInvariant()),
  [Parameter(Position=0)][string][Alias('s')]$subset,
  [ValidateSet("Debug","Release","Checked")][string][Alias('rc')]$runtimeConfiguration,
  [ValidateSet("Debug","Release")][string][Alias('lc')]$librariesConfiguration,
  [Parameter(ValueFromRemainingArguments=$true)][String[]]$properties
)

function Get-Help() {
  Write-Host "Common settings:"
  Write-Host "  -subset                   Build a subset, print available subsets with -subset help (short: -s)"
  Write-Host "  -vs                       Open the solution with VS using the locally acquired SDK. Path or solution name (ie -vs Microsoft.CSharp)"
  Write-Host "  -os                       Build operating system: Windows_NT, Linux, OSX, or Browser"
  Write-Host "  -arch                     Build platform: x86, x64, arm, arm64, or wasm (short: -a). Pass a comma-separated list to build for multiple architectures."
  Write-Host "  -configuration            Build configuration: Debug, Release or Checked (short: -c). Pass a comma-separated list to build for multiple configurations"
  Write-Host "  -runtimeConfiguration     Runtime build configuration: Debug, Release or Checked (short: -rc)"
  Write-Host "  -librariesConfiguration   Libraries build configuration: Debug or Release (short: -lc)"
  Write-Host "  -verbosity                MSBuild verbosity: q[uiet], m[inimal], n[ormal], d[etailed], and diag[nostic] (short: -v)"
  Write-Host "  -binaryLog                Output binary log (short: -bl)"
  Write-Host "  -help                     Print help and exit (short: -h)"
  Write-Host ""

  Write-Host "Actions (defaults to -restore -build):"
  Write-Host "  -restore                Restore dependencies"
  Write-Host "  -build                  Build all source projects (short: -b)"
  Write-Host "  -rebuild                Rebuild all source projects"
  Write-Host "  -test                   Build and run tests (short: -t)"
  Write-Host "  -pack                   Package build outputs into NuGet packages"
  Write-Host "  -sign                   Sign build outputs"
  Write-Host "  -publish                Publish artifacts (e.g. symbols)"
  Write-Host "  -clean                  Clean the solution"
  Write-Host ""

  Write-Host "Libraries settings:"
  Write-Host "  -framework              Build framework: net5.0 or net472 (short: -f)"
  Write-Host "  -coverage               Collect code coverage when testing"
  Write-Host "  -testscope              Scope tests, allowed values: innerloop, outerloop, all"
  Write-Host "  -testnobuild            Skip building tests when invoking -test"
  Write-Host "  -allconfigurations      Build packages for all build configurations"
  Write-Host ""

  Write-Host "Command-line arguments not listed above are passed thru to msbuild."
  Write-Host "The above arguments can be shortened as much as to be unambiguous (e.g. -con for configuration, -t for test, etc.)."
}

if ($help -or (($null -ne $properties) -and ($properties.Contains('/help') -or $properties.Contains('/?')))) {
  Get-Help
  exit 0
}

if ($vs) {
  . $PSScriptRoot\common\tools.ps1

  if (-Not (Test-Path $vs)) {
    $solution = $vs
    # Search for the solution in clickonce
    $vs = Split-Path $PSScriptRoot -Parent | Join-Path -ChildPath "src\clickonce" | Join-Path -ChildPath $vs | Join-Path -ChildPath "$vs.sln"
    if (-Not (Test-Path $vs)) {
      $vs = $solution
      # Search for the solution in installer
      if (-Not ($vs.endswith(".sln"))) {
        $vs = "$vs.sln"
      }
      $vs = Split-Path $PSScriptRoot -Parent | Join-Path -ChildPath "src\installer" | Join-Path -ChildPath $vs
      if (-Not (Test-Path $vs)) {
        Write-Error "Passed in solution cannot be resolved."
        exit 1
      }
    }
  }

  # This tells .NET Core to use the bootstrapped runtime
  $env:DOTNET_ROOT=InitializeDotNetCli -install:$false

  # This tells MSBuild to load the SDK from the directory of the bootstrapped SDK
  $env:DOTNET_MSBUILD_SDK_RESOLVER_CLI_DIR=$env:DOTNET_ROOT

  # This tells .NET Core not to go looking for .NET Core in other places
  $env:DOTNET_MULTILEVEL_LOOKUP=0;

  # Put our local dotnet.exe on PATH first so Visual Studio knows which one to use
  $env:PATH=($env:DOTNET_ROOT + ";" + $env:PATH);

  if ($runtimeConfiguration)
  {
    # Respect the RuntimeConfiguration variable for building inside VS with different runtime configurations
    $env:RUNTIMECONFIGURATION=$runtimeConfiguration
  }

  # Launch Visual Studio with the locally defined environment variables
  ."$vs"

  exit 0
}

# Check if an action is passed in
$actions = "b","build","r","restore","rebuild","sign","testnobuild","publish","clean"
$actionPassedIn = @(Compare-Object -ReferenceObject @($PSBoundParameters.Keys) -DifferenceObject $actions -ExcludeDifferent -IncludeEqual).Length -ne 0
if ($null -ne $properties -and $actionPassedIn -ne $true) {
  $actionPassedIn = @(Compare-Object -ReferenceObject $properties -DifferenceObject $actions.ForEach({ "-" + $_ }) -ExcludeDifferent -IncludeEqual).Length -ne 0
}

if (!$actionPassedIn) {
  $arguments = "-restore -build"
}

foreach ($argument in $PSBoundParameters.Keys)
{
  switch($argument)
  {
    "runtimeConfiguration"   { $arguments += " /p:RuntimeConfiguration=$((Get-Culture).TextInfo.ToTitleCase($($PSBoundParameters[$argument])))" }
    "librariesConfiguration" { $arguments += " /p:LibrariesConfiguration=$((Get-Culture).TextInfo.ToTitleCase($($PSBoundParameters[$argument])))" }
    "framework"              { $arguments += " /p:BuildTargetFramework=$($PSBoundParameters[$argument].ToLowerInvariant())" }
    "os"                     { $arguments += " /p:TargetOS=$($PSBoundParameters[$argument])" }
    "allconfigurations"      { $arguments += " /p:BuildAllConfigurations=true" }
    "properties"             { $arguments += " " + $properties }
    "verbosity"              { $arguments += " -$argument " + $($PSBoundParameters[$argument]) }
    # configuration and arch can be specified multiple times, so they should be no-ops here
    "configuration"          {}
    "arch"                   {}
    default                  { $arguments += " /p:$argument=$($PSBoundParameters[$argument])" }
  }
}

$failedBuilds = @()

foreach ($config in $configuration) {
  $argumentsWithConfig = $arguments + " -configuration $((Get-Culture).TextInfo.ToTitleCase($config))";
  foreach ($singleArch in $arch) {
    $argumentsWithArch =  "/p:TargetArchitecture=$singleArch " + $argumentsWithConfig
    $env:__DistroRid="win-$singleArch"
    Invoke-Expression "& `"$PSScriptRoot/common/build.ps1`" $argumentsWithArch"
    if ($lastExitCode -ne 0) {
        $failedBuilds += "Configuration: $config, Architecture: $singleArch"
    }
  }
}

if ($failedBuilds.Count -ne 0) {
    Write-Host "Some builds failed:"
    foreach ($failedBuild in $failedBuilds) {
        Write-Host "`t$failedBuild"
    }
    exit 1
}

exit 0
