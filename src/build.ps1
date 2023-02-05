$app = "csum"
$ras = "System.Configuration", "System.Windows.Forms"
Add-Type -OutputType ConsoleApplication `
  -ReferencedAssemblies $ras -Path "./*.cs" `
  -OutputAssembly (Join-Path (Resolve-Path "..") "${app}.exe")
