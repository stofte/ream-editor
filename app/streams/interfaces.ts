// json interfaces for query backend api
interface BaseRequest { id: string; }
export interface BaseResponse { code: string; message: string; }

enum StatusCode {
    Ok = 0,
    CompilationError,
    ServerUnreachable,
    ConnectionStringSyntax,
    NamespaceIdentifier,
    UnknownError   
}

type databaseProviderType = 'SqlServer' | 'Npgsql' | 'Sqlite';

export interface CodeRequest extends BaseRequest {
    text: string;
}


export interface QueryTemplateRequest extends BaseRequest {
    serverType: databaseProviderType;
    connectionString: string;
    text: string;
}

export interface CodeTemplateRequest extends BaseRequest {
    text: string;
}

export interface CodeTemplateResponse extends BaseResponse {
        namespace: string;
        template: string;
        header: string;
        footer: string;
        columnOffset: number;
        lineOffset: number;
        defaultQuery: string;
}