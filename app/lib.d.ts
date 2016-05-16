declare var electronRequire: any;
declare var __dirname: string;
declare var MODE: string;
declare var process: any;

declare module "node-uuid" {
    export type v4 = () => string;
    export var v4: v4;
}

declare module CodeMirror {
    var keyMap: any;
    var hint: any;
    var registerHelper: any;
}
