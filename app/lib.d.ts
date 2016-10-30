// global consts
type Configuration = 'PRODUCTION' | 'DEVELOPMENT';
declare const MODE: Configuration;
declare const IS_LINUX: boolean;
declare const IS_WINDOWS: boolean;

declare const electronRequire: any;
declare const __nodeHttp: any;

declare module 'node-uuid' {
    export type v4 = () => string;
    export var v4: v4;
}

declare module NodeJS  {
    interface Process {
        resourcesPath: any;
    }
}

declare module CodeMirror {
    const keyMap: any;
    const hint: any;
    const lint: any;
}

interface IStreamOperation extends Function {
    (conns: any): any;
}

declare function Assert(cond: any): void;
declare function Assert(cond: any, msg: string): void;

type ConnectionType = 'sqlserver' | 'npgsql' | 'sqlite';

declare function Handsontable(elm: HTMLElement, options: any): void;
