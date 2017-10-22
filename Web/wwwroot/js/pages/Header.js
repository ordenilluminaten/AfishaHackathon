class Header {
    constructor() {

    }

    static refreshMyPlaces() {
        Request.post({
            url: '/GetMyPlaces'
        }).then((myPlaces) => {
            appRactive.set('currentUser.myPlaces', myPlaces);
        });
    }
    static refreshFriendPlaces() {
        debugger;
        let userIds = [];
        userIds = appRactive.get('currentUser.friends.userIds');
        if (userIds != null && userIds.length > 0) {
            Request.post({
                url: '/GetUsersEventsByIds',
                data: {
                    ids: userIds
                }
            }).then((friendsEvents) => {
                appRactive.set('currentUser.friendsEvents', friendsEvents);
            });
        }
    }
    static refreshMyNotifications() {
        Request.post({
            url: '/GetMyNotifications'
        }).then((_data) => {
            appRactive.set('currentUser.customData.notifications', _data);
        });
    }
}