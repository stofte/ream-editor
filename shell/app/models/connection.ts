export class Connection {
    public id : number; 
    public editing = false;
    public connectionString : string = null;
    public temporary : string;
    constructor(connectionString: string) {
        this.connectionString = connectionString;
    }
}