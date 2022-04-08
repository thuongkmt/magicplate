Import-Module WebAdministration 
New-WebAppPool -name "konbilocaladmin"  -force

$appPool = Get-Item -name "konbilocaladmin" 
$appPool.processModel.identityType = "NetworkService"
$appPool.enable32BitAppOnWin64 = 1
$appPool | Set-Item


md "c:\konbini\konbilocaladmin"

# All on one line
$site = $site = new-WebSite -name "konbilocaladmin" 
                            -PhysicalPath "c:\Web Sites\konbilocaladmin"
                            -ApplicationPool "konbilocaladmin" 
                            -force