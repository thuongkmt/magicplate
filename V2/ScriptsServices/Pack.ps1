

$workingDirectory = get-location

# read settings

$jsonConfigFile = "$workingDirectory\settings.json"

$jsonConfig = Get-Content $jsonConfigFile | ConvertFrom-Json

$localAdminAngular = "$workingDirectory"+"\"+ $jsonConfig.LocalAdminConfig.Angular
$localAdminNetCoreFolder = "$workingDirectory"+"/"+ $jsonConfig.LocalAdminConfig.Api +"/src/KonbiCloud.Web.Host"
$localAdminNetCoreProjectFile = $localAdminNetCoreFolder + "/KonbiCloud.Web.Host.csproj"

$localCustomerUI = "$workingDirectory"+"\"+ $jsonConfig.LocalAdminConfig.CustomerUI 
$localPOS = "$workingDirectory"+"\"+ $jsonConfig.LocalAdminConfig.POSFolder 
$devicesFolder =  "$workingDirectory"+"\"+ $jsonConfig.LocalAdminConfig.DevicesFolder 

$cloudAdminAngular = "$workingDirectory"+"\"+ $jsonConfig.CloudAdminConfig.Angular
$cloudAdminNetCoreFolder = "$workingDirectory"+"/"+ $jsonConfig.CloudAdminConfig.Api +"/src/KonbiCloud.Web.Host"
$cloudAdminNetCoreProjectFile = $cloudAdminNetCoreFolder + "/KonbiCloud.Web.Host.csproj"




#Build device controllers 

# this path is for visual studio enterprise 2017. for other vs version maybe the path will be different. need to define later
$msbuildpath = "$Env:programfiles (x86)\Microsoft Visual Studio\2017\Enterprise\MSBuild\15.0\Bin\msbuild.exe"

# Visual studio 2017 Community version.
If(!(test-path $msbuildpath )){	
	$msbuildpath = "$Env:programfiles (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MSBuild.exe"
}
If((test-path $msbuildpath )){
	# Build AlarmLight.
	powershell "& '$msbuildpath'" "$devicesFolder\KonbiBrain.WindowServices.AlarmLight\KonbiBrain.WindowServices.AlarmLight.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
	# Build BarCode Scanner.
	powershell "& '$msbuildpath'" "$devicesFolder\BarcodeReaderListener\BarcodeReaderListener.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
	# Build Camera.
	powershell "& '$msbuildpath'" "$devicesFolder\Konbi.Camera\Konbi.Camera.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
	# Build IUC.COTF.
	powershell "& '$msbuildpath'" "$devicesFolder\KonbiBrain.WindowServices.IUC.COTF\KonbiBrain.WindowServices.IUC.COTF.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
	# Build RFIDTableCOTF.
	powershell "& '$msbuildpath'" "$devicesFolder\KonbiBrain.WindowServices.CotfPad\KonbiBrain.WindowServices.CotfPad.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
	# Build RFIDTableCustomPrice.
	powershell "& '$msbuildpath'" "$devicesFolder\KonbiBrain.RfidTable.TakeAwayPrice\KonbiBrain.RfidTable.TakeAwayPrice.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
	# Build TagMapping.
	powershell "& '$msbuildpath'" "$devicesFolder\Konbini.RfidTable.ProductTagMapping\Konbini.RfidTable.ProductTagMapping.csproj" /p:Configuration=Debug /clp:ErrorsOnly /clp:Summary
}

#Set-Location -Path = "$devicesFolder" + "\KonbiBrain.WindowServices.IUC"

# build POS
Set-Location -Path $localPOS
npm install
ng build  --prod

# build local admin angular
Set-Location -Path $localAdminAngular
npm install
ng build  --prod


#build admin net core api
dotnet publish $localAdminNetCoreProjectFile --configuration Release --runtime win7-x64

# build cloud admin angular
Set-Location -Path $cloudAdminAngular
npm install
ng build  --prod
#ng serve

#build cloud admin net core api
dotnet publish $cloudAdminNetCoreProjectFile --configuration Release --runtime win7-x64



# copy over publishing files 
$adminAngularPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.adminAngular
$adminApiPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.adminApi
$customerUiPubishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.customerUI
$servicesPublishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.services
$posPublishFolder = $jsonConfig.publishFolder.root + $jsonConfig.publishFolder.POS

# for cloud
$cloudAdminAngularPubishFolder = $jsonConfig.publishFolder.root +"\\cloud\\angular"
$cloudAdminApiPubishFolder = $jsonConfig.publishFolder.root + "\\cloud\\api"

If(!(test-path $cloudAdminAngularPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $cloudAdminAngularPubishFolder
}
If(!(test-path $cloudAdminApiPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $cloudAdminApiPubishFolder
}

If(!(test-path $adminAngularPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $adminAngularPubishFolder
}
If(!(test-path $adminApiPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $adminApiPubishFolder
}
If(!(test-path $customerUiPubishFolder))
{
      New-Item -ItemType Directory -Force -Path $customerUiPubishFolder
}
If(!(test-path $posPublishFolder))
{
      New-Item -ItemType Directory -Force -Path $posPublishFolder
}

If(!(test-path "$servicesPublishFolder\AlarmLight"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\AlarmLight"
}
If(!(test-path "$servicesPublishFolder\BarCode"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\BarCode"
}
If(!(test-path "$servicesPublishFolder\Camera"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\Camera"
}
If(!(test-path "$servicesPublishFolder\IUC"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\IUC"
}
If(!(test-path "$servicesPublishFolder\RFIDTableCOTF"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\RFIDTableCOTF"
}
If(!(test-path "$servicesPublishFolder\RFIDTableCustomPrice"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\RFIDTableCustomPrice"
}
If(!(test-path "$servicesPublishFolder\TagMapping"))
{
      New-Item -ItemType Directory -Force -Path "$servicesPublishFolder\TagMapping"
}



#delete old files
Get-ChildItem -Path "$cloudAdminAngularPubishFolder" -include *  -Recurse | remove-item -recurse
  Get-ChildItem -Path "$cloudAdminApiPubishFolder" -include *  -Recurse | remove-item -recurse
  
 Get-ChildItem -Path "$adminAngularPubishFolder" -include *  -Recurse | remove-item -recurse
  Get-ChildItem -Path "$adminApiPubishFolder" -include *  -Recurse | remove-item -recurse
   Get-ChildItem -Path "$customerUiPubishFolder" -include *  -Recurse | remove-item -recurse
   Get-ChildItem -Path "$posPublishFolder" -include *  -Recurse | remove-item -recurse
   
   
# copy cloud admin angular
Set-Location -Path "$cloudAdminAngular\dist"

xcopy *.* "$cloudAdminAngularPubishFolder" /c /h /k /r /y /e /s


# copy cloud admin api
Set-Location -Path "$cloudAdminNetCoreFolder\bin\Release\netcoreapp2.1\win7-x64\publish"
xcopy *.* $cloudAdminApiPubishFolder /c /h /k /r /y /e /s


# copy admin angular
Set-Location -Path "$localAdminAngular\dist"

xcopy *.* "$adminAngularPubishFolder" /c /h /k /r /y /e /s


# copy admin api
Set-Location -Path "$localAdminNetCoreFolder\bin\Release\netcoreapp2.1\win7-x64\publish"
xcopy *.* $adminApiPubishFolder /c /h /k /r /y /e /s

# copy customer ui
Set-Location -Path $localCustomerUI
xcopy *.* $customerUiPubishFolder /c /h /k /r /y /e /s

# copy POS
Set-Location -Path "$localPOS\dist\konbi-pos"

xcopy *.* "$posPublishFolder" /c /h /k /r /y /e /s

# copy deployment script to output
xcopy "$workingDirectory\DeployWebApps.ps1" $jsonConfig.publishFolder.root /c /h /k /r /y /e /s

Set-location $workingDirectory

#copy services
Set-Location -Path "$devicesFolder\KonbiBrain.WindowServices.AlarmLight\Bin\Debug"
xcopy *.* "$servicesPublishFolder\AlarmLight" /c /h /k /r /y /e /s

Set-Location -Path "$devicesFolder\BarcodeReaderListener\Bin\Debug"
xcopy *.* "$servicesPublishFolder\BarCode" /c /h /k /r /y /e /s

Set-Location -Path "$devicesFolder\Konbi.Camera\Bin\Debug"
xcopy *.* "$servicesPublishFolder\Camera" /c /h /k /r /y /e /s

Set-Location -Path "$devicesFolder\KonbiBrain.WindowServices.IUC.COTF\Bin\Debug"
xcopy *.* "$servicesPublishFolder\IUC" /c /h /k /r /y /e /s

Set-Location -Path "$devicesFolder\KonbiBrain.WindowServices.CotfPad\Bin\Release"
xcopy *.* "$servicesPublishFolder\RFIDTableCOTF" /c /h /k /r /y /e /s

Set-Location -Path "$devicesFolder\KonbiBrain.RfidTable.TakeAwayPrice\Bin\unicode_debug"
xcopy *.* "$servicesPublishFolder\RFIDTableCustomPrice" /c /h /k /r /y /e /s

Set-Location -Path "$devicesFolder\Konbini.RfidTable.ProductTagMapping\Bin\x64\Debug"
xcopy *.* "$servicesPublishFolder\TagMapping" /c /h /k /r /y /e /s

Set-location $workingDirectory