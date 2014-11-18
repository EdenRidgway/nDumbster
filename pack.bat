:: packs Release binaries into NuGet packages into nuget folder
set dir=nuget\lib\net40
md %dir%
xcopy bin %dir% /r /y
call .nuget\nuget.exe pack nuget\nDumbster.nuspec -OutputDirectory nuget