#builder
#
$AppName = "csum.exe"
$OutputPath = "c:/opt/bin"

$Path = "src/*.cs"
$ReferencedAssemblies = "System.Configuration", "System.Windows.Forms"

Write-Host "build start." -ForegroundColor Yellow

if (-not (Test-Path $OutputPath)) {
  New-Item -Path $OutputPath -ItemType Directory | Out-Null
  Write-Output "make directory."
}
$OutputAssembly = Join-Path (Resolve-Path -Path $OutputPath) $AppName

Write-Output "build program:  $AppName"
Write-Output "    Path:       $Path"
Write-Output "    Output:     $OutputAssembly"
Write-Output "    References: $ReferencedAssemblies"
Add-Type -Path $Path -OutputType ConsoleApplication `
  -OutputAssembly $OutputAssembly `
  -ReferencedAssemblies $ReferencedAssemblies

Write-Host "build completed." -ForegroundColor Yellow
$host.UI.RawUI.ReadKey() | Out-Null
