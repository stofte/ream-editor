Linq Editor [![Windows build status](https://ci.appveyor.com/api/projects/status/s7adk4g4bu8dmh9k?svg=true)](https://ci.appveyor.com/project/stofte/linq-editor) [![Linux build Status](https://travis-ci.org/stofte/linq-editor.svg?branch=master)](https://travis-ci.org/stofte/linq-editor)
======================

Linq Editor is a cross-platform desktop application that allows you to write C#/LINQ queries against SQL databases. Currently supports MS SQL Server and PostgreSQL.

![Main query window](http://i.imgur.com/LGmMhVO.png "Main query window")

![Property completion](http://i.imgur.com/ZmHJwrl.png "Property completion")

![Connection manager](http://i.imgur.com/hYJcXMi.png "Connection manager")


Builds
------
[Windows x64 builds](https://ci.appveyor.com/project/stofte/linq-editor/build/artifacts) courtesy of [AppVeyor](https://www.appveyor.com/) 

Developing
----------

Prerequisites to run repository code locally

- Latest `dotnet` using [.NET Core SDK Installer](https://github.com/dotnet/cli#installers-and-binaries) 
- Download the latest [OmniSharp release](https://github.com/OmniSharp/omnisharp-roslyn/releases) and unzip to `omnisharp`
- `gulp-cli` should be installed globally (`npm install -g gulp-cli`)
- `typings` should be installed globally (`npm install -g typings`) 

Restore depedencies

- `dotnet restore`
- `npm install`

Build and watch the TypeScript and CSS, and start the shell.

- `gulp` 
- `npm start`

The shell handles starting the Query and OmniSharp background processes
as required.

Tests
-----

Test integration scripts perform basic feature tests against a live database.
The connectionstrings used can be found in `scripts/connections.json`.

- `npm run generate-for-sqlserver` scripts for MS SQL Server
- `npm run generate-for-npgsql` scripts for PostgreSQL

Once the database is updated, the tests can be run using

- `npm run int-test-sqlserver`
- `npm run int-test-npgsql`

Connection strings are found in `test/int-helpers.js`

TODO
----
- Table/cell selections
- Error handling
- Culture handling
- Multiple instance handling
- EF fails if table has no primary key?
- SQLite EFCore database providers
- OmniSharp hosting/custom build?
- Websocket/rx query backend
- [dotnet run in folder with spaces fails](https://github.com/dotnet/cli/issues/1189)
