// tsdm doesn't include this file
/// <reference path="../node_modules/retyped-codemirror-tsd-ambient/codemirror-showhint.d.ts" />

declare module CodeMirror {
    var keyMap: any;
    var hint: any;
}

declare var electronRequire: any;
declare var __dirname: string;

declare module "node-uuid" {
    export type v4 = () => string;
    export var v4: v4;
}
