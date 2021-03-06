$(document).ready(() => {
    $('.tooltipped').tooltip();
    $('.dropdown-trigger').dropdown({ coverTrigger:false});
    $('.sidenav').sidenav();
    $('.modal').modal({
        dismissible: false
    });
    $('#video-panel').sidenav({ edge: 'right' });
});

function showAlert(title = null, message, dark, okText) {
    new duDialog(
        title,
        message,
        {
            dark,
            okText
        }
    );
}

function showActionAlert(title = null, message, dark, cancelText, okText, callback) {
    return new duDialog(
        title,
        message,
        {
            buttons: duDialog.OK_CANCEL,
            dark,
            cancelText,
            okText,
            callbacks: {
                okClick: function () {
                    this.hide();
                    callback();
                }
            }
        }
    );
}