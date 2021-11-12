$(document).ready(() => {
    $('#signin-failed').click(function () {
        showAlert('Sign in failed', $(this).attr('data-errorMessage'), false, 'OK');
    });

    $('#signin-failed').trigger('click');

    $('#show-password').change(function () {
        const current = $(this);
        const password = $('#Password');

        if (current.is(':checked'))
            password.attr('type', 'text');
        else
            password.attr('type', 'password');
    })
});