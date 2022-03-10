// The module 'vscode' contains the VS Code extensibility API
// Import the module and reference it with the alias vscode in your code below
import * as vscode from 'vscode';
import axios from 'axios';

// this method is called when your extension is activated
// your extension is activated the very first time the command is executed
export function activate(context: vscode.ExtensionContext) {

    // Use the console to output diagnostic information (console.log) and errors (console.error)
    // This line of code will only be executed once when your extension is activated
    console.log('Congratulations, your extension "stackbeauty" is now active!');

    let showDateTimeCommand = vscode.commands.registerCommand('stackbeauty.showDateTime', () => {

        axios({
            'method': 'get',
            'url': 'https://stackbeauty-dev.whiteplant-313c6159.eastus2.azurecontainerapps.io/healthz',
        }).then(response => {
            console.log(response.data);
        });
        vscode.window.showWarningMessage('Current datetime: ' + (new Date()).toISOString() + 'in theme: ' + vscode.window.activeColorTheme.kind);
        vscode.debug.activeDebugConsole.appendLine("Hello debug info");
    });
    // The command has been defined in the package.json file
    // Now provide the implementation of the command with registerCommand
    // The commandId parameter must match the command field in package.json
    let disposable = vscode.commands.registerCommand('stackbeauty.helloWorld', () => {
        // The code you place here will be executed every time your command is executed
        // Display a message box to the user
        vscode.window.showInformationMessage('Hello from StackBeauty!');
    });

    context.subscriptions.push(disposable);
    context.subscriptions.push(showDateTimeCommand);
}

// this method is called when your extension is deactivated
export function deactivate() { }
