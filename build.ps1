$ErrorActionPreference = "Stop"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$compiler = "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\csc.exe"
$source = Join-Path $projectRoot "src\RoachParadise.cs"
$releaseDirectory = Join-Path $projectRoot "release"
$output = Join-Path $releaseDirectory "RoachParadise.exe"

if (-not (Test-Path -LiteralPath $compiler)) {
    throw "未找到 .NET Framework C# 编译器：$compiler"
}

New-Item -ItemType Directory -Force -Path $releaseDirectory | Out-Null

& $compiler /nologo /target:winexe /platform:anycpu "/out:$output" `
    "/resource:$projectRoot\assets\roach-stage1.png,roach-stage1.png" `
    "/resource:$projectRoot\assets\roach-stage2.png,roach-stage2.png" `
    "/resource:$projectRoot\assets\roach-adult.png,roach-adult.png" `
    "/resource:$projectRoot\assets\roach-egg.png,roach-egg.png" `
    "/resource:$projectRoot\assets\pesticide-spray.png,pesticide-spray.png" `
    "/reference:$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationCore.dll" `
    "/reference:$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\WPF\PresentationFramework.dll" `
    "/reference:$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\WPF\WindowsBase.dll" `
    "/reference:$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\System.Xaml.dll" `
    "/reference:$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\System.Windows.Forms.dll" `
    "/reference:$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\System.Drawing.dll" `
    $source

if ($LASTEXITCODE -ne 0) { throw "构建失败，退出码：$LASTEXITCODE" }
Write-Host "构建完成：$output"
