// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import axios from 'axios';
import { createSyntaxHighlighting, StackDocumentProvider } from './stackDocumentProvider';

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

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
    const stackScheme = 'callstack';
    const stackProvider = new StackDocumentProvider();
    const registerStackDocumentProvider = vscode.workspace.registerTextDocumentContentProvider(stackScheme, stackProvider);
    
    const showDateTimeCommand = vscode.commands.registerCommand('stackbeauty.showDateTime', () => {
        vscode.window.showWarningMessage('Current datetime: ' + (new Date()).toISOString() + 'in theme: ' + vscode.window.activeColorTheme.kind);
        vscode.debug.activeDebugConsole.appendLine("Hello debug info");
    });
    // The command has been defined in the package.json file
    // Now provide the implementation of the command with registerCommand
    // The commandId parameter must match the command field in package.json
    const disposable = vscode.commands.registerCommand('stackbeauty.helloWorld', () => {
        // The code you place here will be executed every time your command is executed
        // Display a message box to the user
        vscode.window.showInformationMessage('Hello from StackBeauty!');
    });

    const showBeautified = vscode.commands.registerCommand('stackbeauty.showBeautified', () => {
        createBeautifiedDocument(context);
    });

    const showBeautifiedWebView = vscode.commands.registerCommand('stackbeauty.showBeautifiedWebView', () => {
        createPanel(context);
    });

    context.subscriptions.push(registerStackDocumentProvider);
    context.subscriptions.push(disposable);
    context.subscriptions.push(showDateTimeCommand);
    context.subscriptions.push(showBeautified);
    context.subscriptions.push(showBeautifiedWebView);
}

async function createBeautifiedDocument(context: vscode.ExtensionContext) {
    if (vscode.window.activeTextEditor === undefined) {
        vscode.window.showErrorMessage("No content to beautify. No active text editor is detected.");
        return;
    }

    let selectedText = vscode.window.activeTextEditor.document.getText(vscode.window.activeTextEditor.selection);
    if (selectedText === '') {
        console.log('No text selected');
        selectedText = vscode.window.activeTextEditor.document.getText();
    }
    console.log(`selected text: ${selectedText}`); 

    if (selectedText === undefined || selectedText === '') {
        vscode.window.showErrorMessage("No text is available for beautifying.");
        return;
    }

    const stackJson = await getStackJson(selectedText);
    if (stackJson && stackJson.length) {
        createSyntaxHighlighting(stackJson);
        const parsedStack = parseJsonStack(stackJson);
        const uri = vscode.Uri.parse(`callstack: ${parsedStack}`);
        const doc = await vscode.workspace.openTextDocument(uri);
        vscode.languages.setTextDocumentLanguage(doc, 'callstack');
        await vscode.window.showTextDocument(doc, {
            preview: false,
            viewColumn: vscode.ViewColumn.Two
        });
    }
}


export const CLASS_DELIMITER = '.';
export const STRING_SPACE = ' ';
export const METHOD_BODY = '{ ... }';
const parseJsonStack = (jsonStack: IStackTree[]) => jsonStack.map((line) => {
        const { method, fullClass } = line.typeValue;
        const endLineStringStack = [];
        if (line.typeDiscriminator === 1) {
            return line.typeValue.value;
        }

        if (fullClass) {
            const { genericParameterTypes } = fullClass;
            endLineStringStack.push(fullClass.shortClassNameOrDefault);
            if (genericParameterTypes && genericParameterTypes.length) {
                endLineStringStack.push(`<${genericParameterTypes.join(', ')}>${CLASS_DELIMITER}`);
            } else {
                endLineStringStack.push(CLASS_DELIMITER);
            }
        }
        if (method) {
            const { parameters, genericParameterTypes } = method;
            endLineStringStack.push(method.name);

            // Push type params if they exist
            if (genericParameterTypes && genericParameterTypes.length) {
                endLineStringStack.push(`<${genericParameterTypes.join(', ')}>`);
            }
            // Push method params
            const paramString = parameters.length ? parameters.map((param) => [
                param.parameterType && param.parameterType,
                param.parameterType && param.parameterName && STRING_SPACE,
                param.parameterName && param.parameterName
            ].join('')).join(', ') : '';
            endLineStringStack.push(`(${paramString}) ${METHOD_BODY}`);
        }
        return endLineStringStack.join('');
    }).join('\n');

function createPanel(context: vscode.ExtensionContext) {
    console.log('Show Beautified is called.');
    if (vscode.window.activeTextEditor === undefined) {
        vscode.window.showErrorMessage("No content to beautify. No active text editor is detected.");
        return;
    }

    let selectedText = vscode.window.activeTextEditor.document.getText(vscode.window.activeTextEditor.selection);
    if (selectedText === '') {
        console.log('No text selected');
        selectedText = vscode.window.activeTextEditor.document.getText();
    }
    console.log(`selected text: ${selectedText}`);

    if (selectedText === undefined || selectedText === '') {
        vscode.window.showErrorMessage("No text is available for beautifying.");
        return;
    }

    const panel = vscode.window.createWebviewPanel(
        'stackbeauty',
        'Beautified Stack',
        vscode.ViewColumn.Two,
        {
            // Enable javascript in the webview
            enableScripts: true,
        }
    );

    getHtmlForWebview(context, selectedText).then((result: string) => {
        panel.webview.html = result;
    }).catch(err => {
        vscode.window.showErrorMessage("Unexpected error " + err);
        return;
    });
}

const isBackendHealthy = async (backend: string) => await axios({
    'method': 'get',
    'baseURL': backend,
    'url': "/healthz",
});

export async function getStackJson(inputText: string) {
    const backend: string = "https://stackbeauty-dev.whiteplant-313c6159.eastus2.azurecontainerapps.io";
    const isHealthy = await isBackendHealthy(backend);
    const beautifiedEndpoint: string = "/Beautified";

    if (isHealthy) {
        const res = await axios({
            'method': 'post',
            'url': beautifiedEndpoint,
            'baseURL': backend,
            'headers': { 'Content-Type': 'text/plain' },
            'data': inputText,
        });

        return res.data;
    } else {
        vscode.window.showErrorMessage("The backend for the Beautifier can't be reached...");
        return "";
    }
}

async function getHtmlForWebview(context: vscode.ExtensionContext, inputText: string): Promise<string> {
    const backend: string = "https://stackbeauty-dev.whiteplant-313c6159.eastus2.azurecontainerapps.io";
    const beautifiedEndpoint: string = "/Beautified";
    const htmlEndpoint: string = "/Html";

    const healthCheckResponse = await isBackendHealthy(backend);

    if (!healthCheckResponse || healthCheckResponse.status !== 200) {
        vscode.window.showErrorMessage("The backend for the Beautifier can't be reached...");
        return "";
    }

    const stackJson = await axios({
        'method': 'post',
        'url': beautifiedEndpoint,
        'baseURL': backend,
        'headers': { 'Content-Type': 'text/plain' },
        'data': inputText,
    });
    console.log('Got json back:' + JSON.stringify(stackJson.data));

    if (!stackJson || stackJson.status !== 200 || !stackJson.data || stackJson.data.length === 0) {
        console.log('No beautify result');
        return `
        <!DOCTYPE html>
        <html lang="en">
        <head>
        <meta charset="UTF-8">
        <meta name="viewport" content="width=device-width, initial-scale=1.0">
        <title>Cat Coding</title>
        </head>
        <body>
            There's no beautify result. Is the selection a valid callstack?
        </body>
        </html>`;
    }

    const htmlResponse = await axios({
        'method': 'post',
        'baseURL': backend,
        'url': htmlEndpoint,
        'headers': { 'Content-Type': 'application/json' },
        'data': stackJson.data,
    });

    if (!htmlResponse || htmlResponse.status !== 200) {
        console.log('Not able to get html for beautified content.');
    }
    return htmlResponse.data;
}

// this method is called when your extension is deactivated
export function deactivate() { }
