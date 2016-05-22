Linq Editor [![Build status](https://ci.appveyor.com/api/projects/status/s7adk4g4bu8dmh9k?svg=true)](https://ci.appveyor.com/project/stofte/linq-editor)
===========
Linq Editor is a desktop application that allows you to write C#/LINQ queries against SQL databases.

Builds
------
[Windows x64 builds](https://ci.appveyor.com/project/stofte/linq-editor/build/artifacts) courtesy of [AppVeyor](https://www.appveyor.com/) 

Developing
----------
- Install the latest `dotnet` using [.NET Core SDK Installer](https://github.com/dotnet/cli#installers-and-binaries)
- Download the latest [OmniSharp release](https://github.com/OmniSharp/omnisharp-roslyn/releases) and unzip to `omnisharp`
- `npm install`
- `npm start`

TODO
----
- Default query
- Error handling
- Culture handling
- Multiple instance handling
- EF fails if table has no primary key?
- SQLite and Postgres EF Core database providers

VS Code
-------------
- omnisharp/vscode for dotnet cli https://github.com/gregg-miskelly/omnisharp-vscode/blob/daily-build-docs/debugger.md
- windows cannot debug pdbs https://github.com/OmniSharp/omnisharp-vscode/wiki/Portable-PDBs#downloading-a-net-cli-which-supports-debugtype-option
