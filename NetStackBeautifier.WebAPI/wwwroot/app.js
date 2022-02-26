const parserPath = "/Beautified";
const divRenderPath = "/HtmlContent";

const btnBeautify = document.getElementById('btnBeautify');
const resultDiv = document.getElementById('result');
const simpleCheckBox = document.getElementById('cbxRenderMode');
const darkThemeButton = document.getElementById('themeDark');
const lightThemeButton = document.getElementById('themeLight');
const styleDiv = document.getElementById('themeStyleSheet');
const onThemeChanged = (ev) => {
    const themeFileName = ev.target.value + ".css";
    styleDiv.href = themeFileName;
}

btnBeautify.addEventListener("click", beautifyButtonClicked);
darkThemeButton.addEventListener("click", onThemeChanged);
lightThemeButton.addEventListener("click", onThemeChanged);

async function beautifyButtonClicked() {
    const callstackInput = document.getElementById("callstackinput").value;
    const jsonResult = await getBeautifiedAsync(callstackInput);
    const divContent = await getDivContent(jsonResult);
    resultDiv.innerHTML = divContent;
}

async function getBeautifiedAsync(data) {
    const response = await fetch(parserPath, {
        method: 'POST',
        headers: {
            'Content-Type': 'text/plain'
        },
        body: data,
    });

    return response.json();
}

async function getDivContent(data) {
    const response = await fetch(divRenderPath + '?' + new URLSearchParams({
        "RenderMode": simpleCheckBox.checked ? "Simple" : "Full",
        "t": darkThemeButton.checked ? darkThemeButton.value : lightThemeButton.value,
    }), {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(data),
    });
    return response.text();
}
