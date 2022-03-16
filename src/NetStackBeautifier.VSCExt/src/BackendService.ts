import * as vscode from 'vscode';
import axios from "axios";

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