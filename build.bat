dotnet publish --self-contained -r win-x64 -p:PublishSingleFile=true
dotnet publish --self-contained -r linux-x64 -p:PublishSingleFile=true

@echo off
copy .\bass\bass.dll .\bin\Release\net10.0\win-x64\publish\
copy .\bass\libbass.so .\bin\Release\net10.0\linux-x64\publish\
copy .\LICENSE .\bin\Release\net10.0\win-x64\publish\
copy .\LICENSE .\bin\Release\net10.0\linux-x64\publish\
xcopy .\Copyrights .\bin\Release\net10.0\win-x64\publish\ /E /I /Y
xcopy .\Copyrights .\bin\Release\net10.0\linux-x64\publish\ /E /I /Y
DEL .\bin\Release\net10.0\win-x64\publish\Avalonix.pdb
DEL .\bin\Release\net10.0\linux-x64\publish\Avalonix.pdb
tar -cvzf win-x64-build.tar.gz .\bin\Release\net10.0\win-x64\publish\*
tar -cJf linux-x64-build.tar.xz .\bin\Release\net10.0\linux-x64\publish\*