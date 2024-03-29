$ErrorActionPreference = "Stop"
$7z = "D:\[Knight]\7-Zip\7z.exe"


#curl 7.66
Set-Location -Path "F:\XTransmit\Windows\Source-XTransmit\XTransmit\Resources\Binary\curl"
$files = 
    "curl.exe",
    "curl-ca-bundle.crt",
    "libcurl-x64.dll",
    "cygpcre-1.dll"

foreach($file in $files) {
    if (Test-Path -Path ".\$file") {
        & $7z a -tgzip "$file.gz" $file
        Remove-Item $file
    }
}


#shadowsocks 3.3.1
Set-Location -Path "F:\XTransmit\Windows\Source-XTransmit\XTransmit\Resources\Binary\shadowsocks"
$files = 
    "cygev-4.dll",
    "cyggcc_s-seh-1.dll",
    "cygmbedcrypto-3.dll",
    "cygpcre-1.dll",
    "cygsodium-23.dll",
    "cygwin1.dll",
    "ss-local.exe"

foreach($file in $files) {
    if (Test-Path -Path ".\$file") {
        & $7z a -tgzip "$file.gz" $file
        Remove-Item $file
    }
}


