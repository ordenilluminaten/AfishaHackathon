var header = Ractive.extend({
    isolated: false,
    template: '#header-template'
});

var sidebar = Ractive.extend({
    isolated: false,
    template: '#sidebar-template'
});

var appRactive = Ractive({
    target: '#app-wrapper',
    template: '#app-template',
    components: {
        Header: header,
        Sidebar: sidebar
    },
    data: {
        currentPage: null
    },
    on: {
        init: () => {
            try {
                VK.init(onInited, function () {

                }, '5.68');
            }
            catch (error) {
                console.log(error);
            }
        },
        complete: () => {
            router.navigate('all');
        }
    }
});

function friendsWillGo() {
    VK.api('friends.get', {
        order: 'hints',
        count: 100,
        fields: 'nickname,photo_100'
    }, (data) => {
        let friends = {};
        let ids = data.response.items.map((item) => {
            friends[item.id] = item;
            return item.id;
        });
        Request.post({
            url: '/GetUsersEventsByIds',
            data: {
                ids: ids
            }
        }).then((usersEvents) => {
            if (usersEvents != null)
                for (let idUser in usersEvents)
                    friends[idUser].events = usersEvents[idUser];

            appRactive.set("currentUser.friends", friends);
            Tabs.init("#friends-dropdown .tabs");
        });
    });
}

function onInited() {
    friendsWillGo();
}



