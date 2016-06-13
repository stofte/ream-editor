export class Connection {
    public id: number; 
    public editing = false;
    public connectionString: string = null;
    public temporary: string;
    public type: ConnectionType;
    constructor(connectionString: string, connectionType: ConnectionType) {
        this.connectionString = connectionString;
        this.type = connectionType;
    }
    public toJSON(): Connection {
        return <Connection> {
            id: this.id,
            connectionString: this.connectionString,
            type: this.type
        };
    }
}
