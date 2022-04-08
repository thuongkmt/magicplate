$serviceName = "Konbi.WindowServices.QRPayment"
Stop-Service -Name $serviceName
Start-Service -Name $serviceName