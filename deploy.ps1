remove-item .\pub\ -recurse -erroraction silentlycontinue
dotnet publish .\src\cli\cli.csproj -c Release -o pub
copy-item -path .\pub\cli.exe -destination $env:APPDATA\utils\gmailer.exe -verbose
