dotnet publish --self-contained -r win-x64 -p:PublishSingleFile=true
dotnet publish --self-contained -r linux-x64 -p:PublishSingleFile=true

@echo off
copy .\bass\bass.dll .\bin\Release\net10.0\win-x64\publish\
copy .\bass\libbass.so .\bin\Release\net10.0\linux-x64\publish\
copy .\LICENSE .\bin\Release\net10.0\win-x64\publish\
copy .\LICENSE .\bin\Release\net10.0\linux-x64\publish\
xcopy .\Copyrights .\bin\Release\net10.0\win-x64\publish\ /E /I /Y
xcopy .\Copyrights .\bin\Release\net10.0\linux-x64\publish\ /E /I /Y
tar -cvzf .\bin\Release\net10.0\win-x64\publish\win-x64-build.tar.gz -C .\bin\Release\net10.0\win-x64\publish\ .
tar -cvzf .\bin\Release\net10.0\linux-x64\publish\linux-x64-build.tar.xz -C .\bin\Release\net10.0\linux-x64\publish\ .