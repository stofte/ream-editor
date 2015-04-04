set COVER_FOLDER=TestCoverage
if not exist %COVER_FOLDER% ( mkdir %COVER_FOLDER% )
if "True" == "%CI%" goto :run
set CONFIGURATION=Debug
:run
.\packages\OpenCover.4.5.3723\OpenCover.Console.exe -target:runtests.bat -register:user -filter:"+[LinqEditor.Core*]* -[*Tests]*" -output:.\%COVER_FOLDER%\TestCoverage.xml
if "True" == "%CI%" goto :skipdisplay
.\packages\ReportGenerator.2.1.4.0\reportgenerator.exe -reports:.\%COVER_FOLDER%\TestCoverage.xml -targetdir:%COVER_FOLDER%
start .\%COVER_FOLDER%\index.htm
:skipdisplay
if "_" == "%CI%_" goto :skipupload
echo Uploading coverage to coveralls.io
.\packages\coveralls.io.1.3.4\tools\coveralls.net.exe %APPVEYOR_BUILD_FOLDER%\%COVER_FOLDER%\TestCoverage.xml
:skipupload
