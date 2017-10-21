var header = Ractive.extend({
    isolated: false,
    template: '#header-template'
});

var appRactive = Ractive({
    target: '#app-wrapper',
    template: '#app-template',
    components: {
        Header: header          
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
            router.navigate('places');
            Tabs.init('#header');
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




