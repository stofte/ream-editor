type eventType = 'new';
type templateType = 'sqlserver' | 'npgsql' | 'sqlite' | 'code';

export class TemplateMessage {
    constructor(
        public type: eventType,
        public template: string,
        public templateType: templateType
    ) {}
}
