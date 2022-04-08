In order to start/ stop  windows service from website which is hosting by IIS 
It requires to install subinacl tool to grant permission for IIS AppPool group

after installing Subinacl tool run below command with CMD under administrator perms. 

the tool can find  in this folder "C:\Program Files (x86)\Windows Resource Kits\Tools"  after installation.

subinacl.exe  /service Konbi.RFIDTable /grant="IIS AppPool"=F