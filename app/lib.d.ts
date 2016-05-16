declare const electronRequire: any;
declare const __dirname: string;
declare const MODE: string;
declare const process: any;

declare module 'node-uuid' {
    export type v4 = () => string;
    export var v4: v4;
}

declare module CodeMirror {
    const keyMap: any;
    const hint: any;
    const registerHelper: any;
}
