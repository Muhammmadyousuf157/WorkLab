const s = x_spreadsheet('#spreadsheet');

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
    const spreadSheetContent = JSON.stringify(data);
    await hubConnection.invoke('SendSpreadSheetContent', spreadSheetContent, sessionKey);
});