

export class Connection {
    public id: number; 
    public editing = false;
    public connectionString: string = null;
    public temporary: string;
    public type: string;
    constructor(connectionString: string, connectionType: string) {
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
