
const endpoint = "https://localhost:7079/";
const parserPath = "Beautified";
const divRenderPath = "HtmlContent";

const btnBeautify = document.getElementById('btnBeautify');
const resultDiv = document.getElementById('result');
const simpleCheckBox = document.getElementById('cbxRenderMode');

btnBeautify.addEventListener("click", BeautifyButtonClicked);

async function BeautifyButtonClicked() {
    const callstackInput = document.getElementById("callstackinput").value;
    const jsonResult = await GetBeautifiedAsync(callstackInput);
    const divContent = await GetDivContent(jsonResult);
    resultDiv.innerHTML = divContent;
}

async function GetBeautifiedAsync(data) {
    const response = await fetch(endpoint + parserPath, {
        method: 'POST',
        headers: {
            'Content-Type': 'text/plain'
        },
        body: data,
    });

    return response.json();
}

async function GetDivContent(data) {
    const response = await fetch(endpoint + divRenderPath + '?' + new URLSearchParams({
        "RenderMode": simpleCheckBox.checked ? "Simple" : "Full"
    }), {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    });
    return response.text();
}