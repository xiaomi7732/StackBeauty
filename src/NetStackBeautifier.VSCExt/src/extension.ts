// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import { createBeautifiedDocument, SHOW_BEAUTIFIED_DOCUMENT } from './Actions';
import { StackDocumentProvider } from './StackDocumentProvider';

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {
    const stackScheme = 'callstack';
    const stackProvider = new StackDocumentProvider();
    const registerStackDocumentProvider = vscode.workspace.registerTextDocumentContentProvider(stackScheme, stackProvider);

    const showBeautified = vscode.commands.registerCommand(SHOW_BEAUTIFIED_DOCUMENT, () => createBeautifiedDocument(context));

    context.subscriptions.push(registerStackDocumentProvider);
    context.subscriptions.push(showBeautified);
}

// this method is called when your extension is deactivated
export function deactivate() { }
