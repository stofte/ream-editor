type eventType = 'new';
type databaseType = 'sqlserver' | 'npgsql' | 'sqlite';

export class SessionMessage {
    constructor(
        public type: eventType,
        public connection: string = null,
        public database: databaseType
    ) {}
}
