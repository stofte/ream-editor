import { Injectable } from '@angular/core';
import { UserStream } from '../streams/user.stream';

@Injectable()
export class UserService {
    constructor(stream: UserStream) {

    }
}
