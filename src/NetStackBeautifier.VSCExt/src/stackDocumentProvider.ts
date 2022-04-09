import {
    CancellationToken,
    EventEmitter,
    TextDocumentContentProvider,
    Uri
} from 'vscode';

interface IStackTreeParams {
    parameterName: string;
    parameterType: string;
    rawParameterType: string;
    fullParameterName: string;
}
export interface IStackTreeMethod {
    name: string;
    parameters: IStackTreeParams[];
    genericParameterTypes: string[];
    rawGenericParameterTypes: string[];
}
export interface IStackTreeFullClass {
    nameSections: string[];
    genericParameterTypes: string[];
    rawGenericParameterTypes: string[];
    fullClassNameOrDefault: string;
    shortClassNameOrDefault: string;
}
export interface IStackTreeTypeValue {
    value?: string;
    assemblySignature: string;
    fileInfo: string;
    fullClass?: IStackTreeFullClass;
    id: string;
    method: IStackTreeMethod;
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
