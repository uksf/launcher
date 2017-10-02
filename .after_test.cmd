nuget install NUnit.Runners -Version 3.7.0 -OutputDirectory tools
nuget install OpenCover -Version 4.6.519 -OutputDirectory tools
nuget install coveralls.net -Version 0.412.0 -OutputDirectory tools
 
.toolsOpenCover.4.6.519toolsOpenCover.Console.exe -target:.toolsNUnit.Runners.3.7.0toolsnunit-console.exe -targetargs:"/nologo /noshadow UKSF-Launcher.Tests.dll" -filter:"+[*]* -[*.Tests]*" -register:user

.toolscoveralls.net.0.412toolscsmacnz.Coveralls.exe --opencover -i .results.xml 