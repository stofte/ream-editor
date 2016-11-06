Ream [![Windows build status](https://ci.appveyor.com/api/projects/status/x8h5dx8lhwv40h8b?svg=true)](https://ci.appveyor.com/project/stofte/ream-editor) [![Linux build Status](https://travis-ci.org/stofte/ream-editor.svg?branch=master)](https://travis-ci.org/stofte/ream-editor)
======================

Ream is a cross-platform desktop application that allows you to write C#/LINQ queries against SQL databases. Currently supports MS SQL Server and PostgreSQL.

![Main query window](http://i.imgur.com/LGmMhVO.png "Main query window")

![Property completion](http://i.imgur.com/ZmHJwrl.png "Property completion")

![Connection manager](http://i.imgur.com/hYJcXMi.png "Connection manager")


Builds
------
[Windows x64 builds](https://ci.appveyor.com/project/stofte/ream-editor/build/artifacts) courtesy of [AppVeyor](https://www.appveyor.com/) 

Developing
----------

Prerequisites to run repository code locally

- Latest `dotnet` using [.NET Core SDK Installer](https://github.com/dotnet/cli#installers-and-binaries) 
- Download the latest [OmniSharp release](https://github.com/OmniSharp/omnisharp-roslyn/releases) and unzip to `omnisharp`
- `gulp-cli` should be installed globally (`npm install -g gulp-cli`)
- `typings` should be installed globally (`npm install -g typings`) 

Restore depedencies

- Follow setup instructions for [ream-query](https://github.com/stofte/ream-query/blob/master/README.md#development)
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
- CodeMirror drag/drop inside editor is broken
- Align directory references in general, esp in typescript code
- Setup omnisharp before starting streams spec test
- Table/cell selections
- Error handling
- Culture handling
- Multiple instance handling
- EF fails if table has no primary key?
- OmniSharp hosting/custom build?
- [dotnet run in folder with spaces fails](https://github.com/dotnet/cli/issues/1189)

