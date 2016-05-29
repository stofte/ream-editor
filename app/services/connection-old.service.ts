import { Injectable } from '@angular/core';
import { StorageService } from './storage.service';
import { Connection } from '../models/connection';

@Injectable()
export class ConnectionService {
    private storageKey = 'connectionstrings';
    public connections: Connection[] = null;
    private id: number = 0;
    
    constructor(private storageService: StorageService) {
        this.connections = this.storageService.Load(this.storageKey, []);
        this.id = this.connections.reduce((prev, curr) => Math.max(prev, curr.id), 0);
    }
    
    public get defaultConnection(): Connection {
        if (this.connections.length > 0) {
            return this.connections[0]; // todo
        }
        return null;
    }
    
    public get(id: number): Connection {
        return <Connection> {
            connectionString: 'dummy'
        };
        // return this.connections.find(x => x.id === id);
    }
    
    public get Connections(): Connection[] {
        return this.connections;
    }
    
    public addNew(conn: Connection) {
        conn.id = this.getNextValidId();
        this.connections.push(conn);
        this.storageService.Save(this.storageKey, this.connections);
    }
    
    public remove(conn: Connection) {
        this.connections = this.connections.filter(x => x.id !== conn.id);
        this.storageService.Save(this.storageKey, this.connections);
    }
    
    public update(conn: Connection) {
        this.connections.find(c => {
           if (c.id === conn.id) {
               c.connectionString = conn.connectionString;
               this.storageService.Save(this.storageKey, this.connections);
               return true;
           } 
           return false;
        });
    }
    
    private getNextValidId(guess = 0): number {
        if (this.connections.find(c => c.id === guess)) {
            return this.getNextValidId(guess + 1);
        }
        return guess;
    }
}
