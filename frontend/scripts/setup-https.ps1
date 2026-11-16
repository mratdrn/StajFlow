$ErrorActionPreference = 'Stop'
$certificateDirectory = Join-Path $PSScriptRoot '../.certs'
New-Item -ItemType Directory -Path $certificateDirectory -Force | Out-Null
$certificatePath = Join-Path $certificateDirectory 'localhost.pem'
dotnet dev-certs https --trust
if ($LASTEXITCODE -ne 0) { throw 'ASP.NET Core geliştirme sertifikasına güvenilemedi.' }
dotnet dev-certs https --export-path $certificatePath --format Pem --no-password
if ($LASTEXITCODE -ne 0) { throw 'Yerel HTTPS sertifikası dışa aktarılamadı.' }
Write-Output 'Yerel HTTPS hazır. Sertifika ve özel anahtar dosyaları Git tarafından yok sayılır.'
