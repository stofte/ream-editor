if "True" == "%CI%" goto :run
set nunitpath=.\packages\NUnit.Runners.2.6.4\tools\
:run
%nunitpath%nunit-console.exe ^
LinqEditor.Core.CodeAnalysis.Tests\bin\%CONFIGURATION%\LinqEditor.Core.CodeAnalysis.Tests.dll ^
LinqEditor.Core.Session.Tests\bin\%CONFIGURATION%\LinqEditor.Core.Session.Tests.dll ^
LinqEditor.Core.Templates.Tests\bin\%CONFIGURATION%\LinqEditor.Core.Templates.Tests.dll ^
LinqEditor.Core.Tests\bin\%CONFIGURATION%\LinqEditor.Core.Tests.dll ^
/noshadow
