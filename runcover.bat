set COVER_FOLDER=TestCoverage
if "True" == "%CI%" goto :ci
set configuration=Debug
goto :run
:ci
set configuration=Release
mkdir %COVER_FOLDER%
:run
.\packages\OpenCover.4.5.3723\OpenCover.Console.exe -target:runtests.bat -register:user -filter:"+[LinqEditor.Core*]* -[*Tests]*" -output:.\%COVER_FOLDER%\TestCoverage.xml
if "True" == "%CI%" goto :skipdisplay
.\packages\ReportGenerator.2.1.4.0\reportgenerator.exe -reports:.\%COVER_FOLDER%\TestCoverage.xml -targetdir:%COVER_FOLDER%
start .\%COVER_FOLDER%\index.htm
:skipdisplay
