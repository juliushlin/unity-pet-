param([switch]$Once)
$ErrorActionPreference='Stop'
$sourceRoot=Split-Path $PSScriptRoot -Parent
$petExe=Join-Path $sourceRoot 'Builds\Xiaoyue\Xiaoyue.exe'
$bridge=Join-Path $PSScriptRoot 'codex-bridge.js'
$unityCli=Join-Path $env:LOCALAPPDATA 'Unity\bin\unity.exe'
$state=Join-Path $env:LOCALAPPDATA 'Xiaoyue'
New-Item -ItemType Directory -Force $state | Out-Null
@{executable=$petExe;bridge=$bridge}|ConvertTo-Json|Set-Content (Join-Path $state 'launcher.json') -Encoding UTF8
$mutex=New-Object System.Threading.Mutex($false,'Local\Xiaoyue.UnityWatcher')
try {$locked=$mutex.WaitOne(0)}catch [System.Threading.AbandonedMutexException]{$locked=$true}
if(!$locked){exit}
try {
 do {
  try {
   $result=& $unityCli editors running --format json 2>$null | Out-String | ConvertFrom-Json
   foreach($editor in $result.data.instances){
    $project=$editor.projectPath
    if(!(Test-Path (Join-Path $project 'Assets')) -or !(Test-Path $petExe)){continue}
    # The CLI also discovers Editors created with -createProject instead of -projectPath.
    $menu=Join-Path $project 'Assets\Editor\XiaoyueProjectLauncher.cs'
    if($project -ne $sourceRoot -and !(Test-Path $menu)){
     New-Item -ItemType Directory -Force (Split-Path $menu) | Out-Null
     Copy-Item (Join-Path $PSScriptRoot 'XiaoyueProjectLauncher.cs.template') $menu
    }
    $existing=Get-CimInstance Win32_Process -Filter "Name='Xiaoyue.exe'" | Where-Object {$_.CommandLine -match ("--unity-owner-pid\s+"+$editor.pid+"(?:\s|$)")}
    if($existing){continue}
    $start=New-Object System.Diagnostics.ProcessStartInfo
    $start.FileName=$petExe
    $start.Arguments='--unity-owner-pid '+$editor.pid+' --unity-project "'+$project+'"'
    $start.WorkingDirectory=Split-Path $petExe
    $start.UseShellExecute=$false
    $start.CreateNoWindow=$true
    $start.EnvironmentVariables['XIAOYUE_BRIDGE_PATH']=$bridge
    $pet=[System.Diagnostics.Process]::Start($start)
    Set-Content (Join-Path $project 'Library\Xiaoyue-desktop.pid') $pet.Id
   }
  }catch {Add-Content (Join-Path $state 'watcher.log') ((Get-Date -Format o)+' '+$_.Exception.Message)}
  if(!$Once){Start-Sleep -Seconds 5}
 }while(!$Once)
}finally{$mutex.ReleaseMutex();$mutex.Dispose()}
