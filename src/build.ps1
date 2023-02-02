$app = "csum"
$ras = "System.Configuration", "System.Xml", "System.Windows.Forms", "System.Drawing"
Add-Type -OutputType ConsoleApplication -ReferencedAssemblies $ras -Path "./*.cs" `
  -OutputAssembly (Join-Path (Resolve-Path "..") "${app}.exe")
