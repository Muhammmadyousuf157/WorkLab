$('.archive-session').click(function () {
    const sessionId = $(this).data('sessionid');
    const $card = $(this).parent().parent().parent();

    showActionAlert('Are you sure?', 'You won\'t be able to revert this!', false, 'Cancel', 'Delete', async () => {

        const response = await fetch(`/WorkSpace/Session/DeleteSession?sessionId=${sessionId}`, { method: 'POST' });


        if (response.status !== 200) {
            showAlert('Error', 'something went wrong while deleting', true, 'OK');
            return;
        }

        $card.remove();

        if (!($('.dashboard-container .row').html().trim()))
            $('.no-session').removeClass('hide').css('display', undefined);
    });
});

$('.download-spreadsheet').on('click', async function () {
    const response = await fetch(`/WorkSpace/Session/DownloadSpreadSheet?fileId=${$(this).attr('data-fileId')}`);

    if (response.status !== 200) {
        showAlert('Error', 'something went wrong while downloading the file', true, 'OK');
        return;
    }

    const data = await response.json();

    const content = [];
    content.push(JSON.parse(data.file.content));

    const new_wb = xtos(content);

    XLSX.writeFile(new_wb, `${data.file.title}.xlsx`);
});