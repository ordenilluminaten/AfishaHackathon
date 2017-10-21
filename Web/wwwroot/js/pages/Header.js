class Header {
    constructor() {

    }

    static openFriends() {
        debugger;
        let userIds = [1];
        // userIds = appRactive.get('currrentUser.friends.userIds');
        // if(userIds != null && userIds.length > 0) {
            Request.post({
                url: '/GetUsersEventsByIds',
                data: {
                    ids: userIds
                }
            }).then((friendsEvents) => {
                appRactive.set('currrentUser.friendsEvents', friendsEvents);                                                 
            });
        // }     
    }
}