export enum QueryStreamEvent {
    RunCodeResponse
}

export class Message {
    constructor(public type: QueryStreamEvent) {

    }
}
