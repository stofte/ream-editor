import { Injectable } from '@angular/core';

@Injectable()
export class StorageService {
    
    public Load(key: string, defaultValue: any) : any {
        try {
            var val = localStorage.getItem(key);
            if (!val) return defaultValue;
            return JSON.parse(val);
        }
        catch (exn) {
            return defaultValue;
        }
    }
    
    public Save(key: string, value: any) : void {
        localStorage.setItem(key, JSON.stringify(value));
    }
}
