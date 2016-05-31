import { Injectable } from '@angular/core';
import { ReplaySubject, Subject, Observable } from 'rxjs/Rx';
import { Connection } from '../models/connection';

@Injectable()
export class ConnectionService {
    private key = 'connections';
    private stream: Observable<Connection[]>;
    private ops = new ReplaySubject<IStreamOperation>();
    
    constructor() {
        // initial values
        this.ops.next(x => x);
        this.stream = this.ops
            .scan((acc: Connection[], op: IStreamOperation) => {
                return op([...acc]);
            }, this.load());
        this.stream.subscribe(cs => {
            this.save(cs);
        });
    }
    
    public get all(): Observable<Connection[]> {
        return this.stream;
    }
    
    public add(conn: Connection) {
        this.ops.next((conns) => {
            conn.id = this.getNextValidId(0, conns);
            return [conn, ...conns];   
        });
    }
    
    public remove(conn: Connection) {
        this.ops.next((conns) => {
            return [...conns.filter(x => x.id !== conn.id)];
        });
    }

    public update(conn: Connection) {
        this.ops.next((conns) => {
            let idx = -1;
            conns.forEach((x, i) => {
                x.id === conn.id ? idx = i : null;
            });
            assert(idx >= 0, 'update id not found');
            return [...conns.slice(0, idx),
            <Connection> {
                id: conn.id,
                connectionString: conn.connectionString,
            }, ...conns.slice(idx + 1)];
        });
    }
    
    private load(): Connection[] {
        try {
            let raw = localStorage.getItem(this.key);
            return raw ? <Connection[]> JSON.parse(raw) : [];
        }
        catch (e) {
            console.error(`connection-service load exception: ${e}`);
            return [];
        }
    }
    
    private save(conns: Connection[]) {
        localStorage.setItem(this.key, JSON.stringify(conns));
    }

    private getNextValidId(guess = 0, conns: Connection[]): number {
        if (conns.find(c => c.id === guess)) {
            return this.getNextValidId(guess + 1, conns);
        }
        return guess;
    }
}
