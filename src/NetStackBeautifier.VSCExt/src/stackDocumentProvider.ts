import {
    CancellationToken,
    EventEmitter,
    TextDocumentContentProvider,
    Uri
} from 'vscode';

interface IStackTreeParams {
    parameterName: string;
    parameterType: string;
}
interface IStackTreeTypeValue {
    value?: string;
    assemblySignature: string;
    fileInfo: string;
    fullClass?: {
        nameSections: string[];
        genericParameterTypes: string[];
        fullClassNameOrDefault: string;
        shortClassNameOrDefault: string;
    };
    id: string;
    method: {
        name: string;
        parameters: IStackTreeParams[];
        genericParameterTypes: string[];
    };
    tags: any;
}
export interface IStackTree {
    typeDiscriminator: number;
    typeValue: IStackTreeTypeValue; 
}

export class StackDocumentProvider implements TextDocumentContentProvider {
    public onDidChangeEmitter = new EventEmitter<Uri>();
    public onDidChange = this.onDidChangeEmitter.event;

    public provideTextDocumentContent(uri: Uri, _token: CancellationToken): string {
        return uri.path.trim();
    }
}
