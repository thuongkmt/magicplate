$service = Get-WmiObject -Class Win32_Service -Filter "Name='Konbi.WindowServices.QRPayment'"
if($service)
{
	Write-Host 'Deleting Konbi.WindowServices.QRPayment'
	$service.delete()
}
else {
	Write-Host 'Service not found'
}