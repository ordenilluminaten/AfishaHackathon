var header = Ractive.extend({
    isolated: false,
    template: '#header-template'
});

var sidebar = Ractive.extend({
    isolated: false,
    template: '#sidebar-template'
});

var filter = Ractive.extend({
    isolated: false,
    template: '#filter-template'
});

var appRactive = Ractive({
    target: '#app-wrapper',
    template: '#app-template',
    components: {
        Header: header,
        Sidebar: sidebar,
        Filter: filter
    },
    data: {
        currentPage: null,
        filter: {
            Category: 0,
            Search: null,
            ViewType: 0,
            Sort: 'id'
        }
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
            Tabs.init(".tabs");
        }
    }
});

function getVkfriends() {
    VK.api('friends.get', {
        order: 'hints',
        count: 100,
        fields: 'nickname,photo_100'
    }, (data) => {
        let friends = {};
        let userIds = [];
        let count = 0;
        userIds = data.response.items.map((item) => {
            friends[item.id] = item;
            count++;
            return item.id;
        });

        appRactive.set("currentUser.friends", {
            count: count,
            userIds: userIds,
            items: friends
        });       
    });
}

function onInited() {
    getVkfriends();
}




