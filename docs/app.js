const backend = "https://stackbeauty-dev.whiteplant-313c6159.eastus2.azurecontainerapps.io"
// const backend = "https://localhost:7079"

const parserPath = "/Beautified";
const divRenderPath = "/HtmlContent";

const btnBeautify = document.getElementById('btnBeautify');
const btnClearInput = document.getElementById('btnClearInput');
const resultDiv = document.getElementById('result');
const simpleCheckBox = document.getElementById('cbxRenderMode');
const cbxAutoSubmitExample = document.getElementById('cbxAutoSubmitExample');
const darkThemeButton = document.getElementById('themeDark');
const lightThemeButton = document.getElementById('themeLight');
const styleDiv = document.getElementById('themeStyleSheet');
const callstackInputTextArea = document.getElementById('callstackinput');
const btnExample1 = document.getElementById('btnExampleInput1');
const btnExample2 = document.getElementById('btnExampleInput2');
const btnExample3 = document.getElementById('btnExampleInput3');
const btnExample4 = document.getElementById('btnExampleInput4');
const btnExamples = [btnExample1, btnExample2, btnExample3, btnExample4];
var timeoutHandler;

const onThemeChanged = (ev) => {
    const themeFileName = ev.target.value + ".css";
    styleDiv.href = themeFileName;
}

btnBeautify.addEventListener("click", beautifyButtonClicked);
btnClearInput.addEventListener('click', () => {
    callstackInputTextArea.value = "";
    btnClearInput.disabled = true;
    btnBeautify.disabled = true;
});
darkThemeButton.addEventListener("click", onThemeChanged);
lightThemeButton.addEventListener("click", onThemeChanged);

callstackInputTextArea.addEventListener('input', async (ev) => {
    btnClearInput.disabled = (ev.target.value === "");
    btnBeautify.disabled = (ev.target.value === "");

    if (cbxAutoSubmitExample.checked) {
        await beautifyButtonClicked();
    }
});

const exampleButtonCount = btnExamples.length;
for (let i = 0; i < exampleButtonCount; i++) {
    btnExamples[i].addEventListener('click', () => inputExample(`example${i + 1}.txt`));
}

async function inputExample(fileName) {
    const response = await fetch(fileName);
    callstackInputTextArea.value = await response.text();
    btnClearInput.disabled = false;
    btnBeautify.disabled = false;
    if (cbxAutoSubmitExample.checked) {
        await beautifyButtonClicked();
    }
}

async function beautifyButtonClicked() {
    const callstackInput = callstackInputTextArea.value;
    try {
        timeoutHandler = setTimeout(() => setInprogressResult(), 500);
        const jsonResult = await getBeautifiedAsync(callstackInput);
        if (!jsonResult || jsonResult.length === 0) {
            if (!!timeoutHandler) {
                clearTimeout(timeoutHandler);
            };

            throw {
                title: "No content returned",
            };
        }
        const divContent = await getDivContent(jsonResult);
        if (!!timeoutHandler) {
            clearTimeout(timeoutHandler);
        };
        resultDiv.innerHTML = divContent;
    } catch (ex) {
        console.error(ex);
        if (!!timeoutHandler) {
            clearTimeout(timeoutHandler);
        };
        resultDiv.innerHTML = `An error happened. Details: ${ex.title}`;
    }
    finally {
        if (!!timeoutHandler) {
            clearTimeout(timeoutHandler);
        };
    }
}

async function getBeautifiedAsync(data) {
    const response = await fetch(backend + parserPath, {
        method: 'POST',
        headers: {
            'Content-Type': 'text/plain'
        },
        body: data,
    });

    if (response.ok) {
        return response.json();
    }
    throw await response.json();
}

async function getDivContent(data) {
    const response = await fetch(backend + divRenderPath + '?' + new URLSearchParams({
        "RenderMode": simpleCheckBox.checked ? "Simple" : "Full",
    }), {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    });
    return response.text();
}

function setInprogressResult() {
    resultDiv.innerHTML = "It is cold. Give me a few seconds to warm up....";
}
