type eventType = 'new';
type databaseType = 'sqlserver' | 'npgsql' | 'sqlite';

export class SocketMessage {
    constructor(
        public type: eventType,
        public connection: string = null,
        public database: databaseType
    ) {}
}
