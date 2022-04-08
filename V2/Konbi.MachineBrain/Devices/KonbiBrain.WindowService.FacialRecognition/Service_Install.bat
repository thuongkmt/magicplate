C:\Windows\Microsoft.NET\Framework\v4.0.30319\InstallUtil.exe /servicename="Konbi.FacialRecognition" %~dp0\bin\Debug\KonbiBrain.WindowService.FacialRecognition.exe
net start "Konbi.FacialRecognition"
pause

or 

sc create Konbi.FacialRecognition binpath="D:\Project\MagicPlate\Magicplate-CSharp\V2\Konbi.MachineBrain\Devices\KonbiBrain.WindowService.FacialRecognition\bin\Debug\KonbiBrain.WindowService.FacialRecognition.exe"