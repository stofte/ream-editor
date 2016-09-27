// ReamQuery API types for socket messages
export type WebSocketMessageType = 'table'|'header'|'row'|'tableClose'|
    'singleAtomic'|'singleTabular'|'empty'|'close';

export interface WebSocketMessage {
    session: string;
    id: any;
    parent: any;
    type: WebSocketMessageType;
    values: any[];
}
