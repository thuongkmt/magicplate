Write-Host "Install software tools" -ForegroundColor Green

# install chocolatey
Set-ExecutionPolicy Bypass -Scope Process -Force; iex ((New-Object System.Net.WebClient).DownloadString('https://chocolatey.org/install.ps1'))

# install mysql
choco install mysql
choco install mysql.workbench

# teamviewer 12 setup
choco install teamviewer --version 12.0.72365

# git
choco install git

#rabbitmq
choco install rabbitmq





