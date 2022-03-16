import * as vscode from 'vscode';
import { getStackJson } from './BackendService';
import { generateColorizer, parseJsonStack } from './callStackLanguageService';

export const SHOW_BEAUTIFIED_DOCUMENT = 'stackbeauty.showBeautified';

export async function createBeautifiedDocument(context: vscode.ExtensionContext) {
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
        generateColorizer(stackJson);
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