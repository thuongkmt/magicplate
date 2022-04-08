$bin = ((Get-Location).Path) + "\Konbi.WindowServices.QRPayment.exe"

$serviceName = "Konbi.WindowServices.QRPayment"

$s = (Get-WmiObject win32_service -Filter "name='Konbi.WindowServices.QRPayment'")
if($s)
{
    Stop-Service -Name $serviceName
	$s.delete()
}


Write-Host "Creating new service | BIN: " + $bin
New-Service -Name $serviceName -BinaryPathName $bin -StartupType Auto -Description "KONBINI"
Start-Service -Name $serviceName
Get-Service $serviceName