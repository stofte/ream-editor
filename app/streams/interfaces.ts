// json interfaces for query backend api
interface BaseRequest { id: string; }

enum StatusCode {
    Ok = 0,
    CompilationError,
    ServerUnreachable,
    ConnectionStringSyntax,
    NamespaceIdentifier,
    UnknownError   
}

export interface CodeRequest extends BaseRequest {
    text: string;
}
