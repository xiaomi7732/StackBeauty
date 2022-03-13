import { CancellationToken, EventEmitter, TextDocumentContentProvider, Uri } from 'vscode';
import { getStackJson } from './extension';

export class StackDocumentProvider implements TextDocumentContentProvider {
    public onDidChangeEmitter = new EventEmitter<Uri>();
    public onDidChange = this.onDidChangeEmitter.event;

    public provideTextDocumentContent(uri: Uri, _token: CancellationToken): string {
        return uri.path.trim();
    }
}