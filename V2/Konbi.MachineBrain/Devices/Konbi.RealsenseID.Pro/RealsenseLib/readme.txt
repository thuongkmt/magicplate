

1. Group dll numer one
rsid.dll -> from c plus pluss dll
rsid_c.dll -> from c plus pluss dll

These two above dlls we will copy directly to the bin/Debug or bin/Relase foler to run in the runtime

2. Group dll number two
rsid_dotnet.dll from c# -> we will reference it to the


3. Convert PlateForm target to x64 in the Project Properties/Build tab

4. When deploy on the new windows, need to install this is required
+ download Microsoft Visual C++runtime( https://support.microsoft.com/en-us/topic/the-latest-supported-visual-c-downloads-2647da03-1eea-4433-9aff-95f26a218cc0)
it'name is VC_redist.x64.exe
+ install Dotnet framework 4.7.2