class Header {
    constructor() {

    }

    static openFriends() {
        let userIds = [1];
        userIds = appRactive.get('currrentUser.friends.userIds');
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