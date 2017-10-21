class Header {
    constructor() {

    }

    static openFriends() {
        let userIds = [];
        userIds = appRactive.get('currentUser.friends.userIds');
        if(userIds != null && userIds.length > 0) {
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
}