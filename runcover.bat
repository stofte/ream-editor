set folder=TestCoverage
set configuration=Debug
.\packages\OpenCover.4.5.3723\OpenCover.Console.exe -target:runtests.bat -register:user -filter:"+[LinqEditor.Core*]* -[*Tests]*" -output:.\%folder%\TestCoverage.xml
.\packages\ReportGenerator.2.1.4.0\reportgenerator.exe -reports:.\%folder%\TestCoverage.xml -targetdir:%folder%  
start .\%folder%\index.htm