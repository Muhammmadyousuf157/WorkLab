const s = x_spreadsheet('#spreadsheet');

try {
    s.loadData(JSON.parse($('#editor-content').text()));
} catch (e) {
}

$('.x-spreadsheet-bottombar').remove();
$('.x-spreadsheet-toolbar-btns').children().eq(2).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(25).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(26).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(0).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(1).hide();

$('.x-spreadsheet-toolbar-btns').children().eq(18).find('.x-spreadsheet-dropdown-content').css({"width": "260px"});

$('.x-spreadsheet-toolbar-btns').children().eq(6).first().children().eq(0).children().eq(1).children().eq(8).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(6).first().children().eq(0).children().eq(1).children().eq(9).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(6).first().children().eq(0).children().eq(1).children().eq(10).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(6).first().children().eq(0).children().eq(1).children().eq(11).hide();
$('.x-spreadsheet-toolbar-btns').children().eq(6).first().children().eq(0).children().eq(1).children().eq(12).hide();

let fileSaveTimeout = undefined;

let typing = false;
let timeout = undefined;

async function timeoutFunction() {
    typing = false;
    await hubConnection.invoke('StoppedTyping', sessionKey);
}

async function configTypingIndication() {
    if (typing == false) {
        typing = true
        await hubConnection.invoke('StartedTyping', sessionKey);
        timeout = setTimeout(timeoutFunction, 1000);
    } else {    
        clearTimeout(timeout);
        timeout = setTimeout(timeoutFunction, 1000);
    }
}


function configSessionFileUpdate(content) {
    if (!sessionCurrentFile) return;

    if (fileSaveTimeout) clearTimeout(fileSaveTimeout);

    fileSaveTimeout = setTimeout(async () => {
        const url = `/WorkSpace/Session/UpdateFileContent?filePath=${sessionCurrentFile.filePath}&sessionKey=${sessionKey}`;
        const response = await fetch(url, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(content)
        });

        if (response.status !== 200)
            showAlert('Error', 'something went wrong while saving the file', true, 'OK');
    }, 3000);
}

hubConnection.on('ReceiveSpreadSheetContent', spreadSheetContent => {
    const data = JSON.parse(spreadSheetContent);
    s.loadData(data);

    configSessionFileUpdate(spreadSheetContent);
});

s.change(async data => {
    configTypingIndication();
    const spreadSheetContent = JSON.stringify(data);
    await hubConnection.invoke('SendSpreadSheetContent', spreadSheetContent, sessionKey);
});

$('#btn_Download').click(async () => {
    debugger;
    const response = await fetch(`/WorkSpace/Session/GetSessionFileTitle?sessionKey=${sessionKey}`);
    const title = await response.text();

    const new_wb = xtos(s.getData());

    /* generate download */
    XLSX.writeFile(new_wb, `${title}.xlsx`);
});