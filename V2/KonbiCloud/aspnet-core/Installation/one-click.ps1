#Author: Ha Doan
#Date: 28/06/2019
#One-click installation for Magicbox

#step1 install tools
& .\InstallTools.ps1

#step2 run settings powershell script from Long


#step3 get deployment dlls from git
& .\CloneSoftware.ps1

#step4 setup iis and install app
& .\SetupIIS.ps1
& .\NewIISPoolAndWeb.ps1


#step5 Config hardware/software
#config rabbitmq
Start-Process ".\rabbitmqconfig.bat"
#config mysql
Start-Process ".\ConfigMysql.bat"
#migrate database


#step6 all set!
