declare var electronRequire: any;
declare var __dirname: string;

declare module "node-uuid" {
    export type v4 = () => string;
    export var v4: v4;
}
