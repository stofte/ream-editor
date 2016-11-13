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

Development requires a recent [NodeJS/npm](https://nodejs.org/en/) and 
[.NET Core CLI](https://www.microsoft.com/net/core) installed in the path.

To setup a local repository follow these steps:

1. Clone repository with `--recursive` to fetch the `query` (ReamQuery) submodule 
2. [Setup](https://github.com/stofte/ream-query/blob/master/README.md) ReamQuery and run tests to verify submodule functionality
3. Download the latest [OmniSharp release](https://github.com/OmniSharp/omnisharp-roslyn/releases) and unzip to `omnisharp`
4. `gulp-cli` should be installed globally (`npm install -g gulp-cli`)
5. `typings` should be installed globally (`npm install -g typings`)
6. `npm install` to restore dependencies
7. `npm run bower-install` to install bower dependencies

Build and watch the TypeScript and Sass files, and start the shell:

1. `gulp`
2. `npm start`

Tests
-----

Tests are primarily integration tests and focus on communication with
OmniSharp and ReamQuery backends. Currently runs against SQLite
database found in `query/sql/world.sqlite`.

- `npm test` stress testing of streams and plain regular unit tests
- `npm run int-test` tests final build (run `build.cmd` or `build.sh` first)

Building
--------

The editor can be built using the `scripts\build.cmd` and `scripts\build.sh` scripts.
Script must be run from the repository root folder.

TODO
----
- CodeMirror drag/drop inside editor is broken
- Align directory references in general, esp in typescript code
- Table/cell selections
- Error handling
- Culture handling
- Multiple instance handling
- EF fails if table has no primary key?
- OmniSharp hosting/custom build?
- [dotnet run in folder with spaces fails](https://github.com/dotnet/cli/issues/1189)
