set folder=TestCoverage
if "True" == "%CI%" goto :ci
set configuration=Debug
goto :run
:ci
set configuration=Release
mkdir %folder%
:run
.\packages\OpenCover.4.5.3723\OpenCover.Console.exe -target:runtests.bat -register:user -filter:"+[LinqEditor.Core*]* -[*Tests]*" -output:.\%folder%\TestCoverage.xml
.\packages\ReportGenerator.2.1.4.0\reportgenerator.exe -reports:.\%folder%\TestCoverage.xml -targetdir:%folder%
if "True" == "%CI%" goto :skipdisplay
start .\%folder%\index.htm
:skipdisplay
if "_" == "%CI%_" goto :skipupload
.\packages\coveralls.io.1.3.4\tools\coveralls.net.exe -opencover %folder%\TestCoverage.xml
:skipupload