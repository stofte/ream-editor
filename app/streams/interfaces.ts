// json interfaces for query backend api
interface BaseRequest { id: string }
enum StatusCode {
    Ok = 0,
    CompilationError,
    ServerUnreachable,
    ConnectionStringSyntax,
    NamespaceIdentifier,
    UnknownError   
}

export interface CodeRequest extends BaseRequest {
    text: string
}

enum ItemType {
    Table = 0,
    Header,
    Row,
    SingleAtomic,
    SingleTabular,
    Empty,
    Close
}
export interface WebSocketMessage {
    guid: string,
    id: any,
    parent: any,
    type: ItemType,
    values: any[]
}
