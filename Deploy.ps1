$logfile = "logs/deploy.log"
$exludedFiles = 
    "es", 
    "fr", 
    "cs", 
    "de", 
    "it", 
    "ja", 
    "ko", 
    "pl", 
    "ru", 
    "tr",
    "pt-BR",
    "zh-Hans",
    "zh-Hant",
    "publish",
    "runtimes",
    "appsettings.Development.json",
    "logbook.exe"

$keyfile = "C:\Users\milan\ssh_keys\deploy_private_key"

dotnet publish >> $logfile
New-Item -Path . -Name "deploy" -ItemType "Directory" >> $logfile
Copy-Item "bin/Release/net9.0/*" -Destination deploy

foreach ($file in $exludedFiles)
{
    Remove-Item -Path "deploy/${file}"
}

Compress-Archive -Path "deploy/*" -DestinationPath "deploy/logbook.zip"

scp -i $keyfile "deploy/logbook.zip" deploy@xilobone.com:/home/deploy
ssh -i $keyfile deploy@xilobone.com "sudo unzip -o /home/deploy/logbook.zip -d /opt/logbook/bin"
ssh -i $keyfile deploy@xilobone.com "sudo chmod 666 /opt/logbook/bin/*"
ssh -i $keyfile deploy@xilobone.com "sudo systemctl restart logbook"
ssh -i $keyfile deploy@xilobone.com "rm -f /home/deploy/logbook.zip"

Remove-Item -Path "deploy" -Recurse