import { Observable } from 'rxjs/Rx';
import { TemplateResponse } from './interfaces';
import * as uuid from 'node-uuid';

export class TemplateInfo {
    public connectionId: number;
    public header: string;
    public footer: string;
    public template: string;
    public lineOffset: number;
    public columnOffset: number;
    public namespaceToken: string;
}

export class TemplateCache {
    private templates: TemplateInfo[] = [];

    public query(connectionId: number, initialText: string): TemplateInfo {
        const tmpl = this.templates.find(x => x.connectionId === connectionId);
        if (tmpl) {
            const newNamespace = 'a' + uuid.v4().replace(/-/g, '');
            const newTemplate = (tmpl.header + initialText + tmpl.footer)
                .replace(new RegExp(tmpl.namespaceToken, 'g'), newNamespace);
            return <TemplateInfo> {
                template: newTemplate,
                lineOffset: tmpl.lineOffset,
                columnOffset: tmpl.columnOffset,
                namespaceToken: newNamespace
            };
        }
        return null;
    }

    public add(connectionId: number, template: string, initialText: string, line: number, column: number, namespaceToken: string) {
        if (this.templates.find(x => x.connectionId === connectionId)) {
            return;
        }
        const lines = template.split(/\n/);
        let header = lines.slice(0, line).join('\n');
        let footer = lines.slice(line + 1).join('\n');
        header = header + '\n' + lines[line].substring(0, column);
        footer = lines[line].substring(column) + '\n' + footer;
        this.templates.push(<TemplateInfo> {
            connectionId,
            header,
            footer,
            lineOffset: line,
            columnOffset: column,
            namespaceToken
        });
    }
}