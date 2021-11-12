$(document).ready(() => {
    $('#video-panel').sidenav({ edge: 'right' });

    const users = [];

    $('.participant').each(function () {
        const name = $(this).find('a').text();
        users.push(name);
    });

    $('#participant-search').keyup(function () {
        const query = $(this).val().trim().toLowerCase();

        if (!query) {
            showUsers(users);
            return;
        }

        const filteredUsers = users.filter(name => name.toLowerCase().includes(query));

        showUsers(filteredUsers);
    });

    function showUsers(users) {
        const $list = $('.participant-list ul');

        if (users.length === 0) {
            $list.html('<li class="participant center"><a class="blue-grey-text">No Participant</a></li>');
            return;
        }

        let markup = '';

        for (const user of users) {
            markup += `<li class="participant"><a class="blue-text">${user}</a></li>`;
        }

        $list.html(markup);
    }
});