var header = Ractive.extend({
    isolated: false,
    template: '#header-template',
    data: function () {
        return {
            friendFilter: (item, seach) => {
                if(seach == null || seach == "")
                    return item;
                
                seach = seach.toLowerCase();
                if((item.first_name != null 
                        && item.first_name.toLowerCase().includes(seach)) 
                || (item.last_name != null 
                        && item.last_name.toLowerCase().includes(seach)))
                    return item;
                return null;
            }
        }
    },
    on: {
        setFamiliarWithBot: (_ctx) => {
            Request.post({
                method: Request.method.post,
                url: '/home/setFamiliarWithBot'
            }).then(() => {
                appRactive.set('currentUser.customData.isFamiliarWithBot', true);
                dropdown.hide(document.getElementById('bot-dropdown'), 'bot-dropdown');

            });
        },
        openGroupDialog: (_ctx, _url) => {
            window.open(_url, '_blank');
        },
        openPlace: (_ctx, _id) => {
            Request.post({
                url: '/GetUserPlace',
                data: {
                    id: _id
                }
            }).then((place) => {
                router.navigate('place', place);
            });
        }
    }
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
            debugger;
            initYaMap();
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

function initYaMap(city){
    ymaps.ready(()=>{
        var map = new ymaps.Map('myMap', {
            center: [59.94, 30.32],
            zoom: 9
        });

        debugger;
        var myGeocoder = ymaps.geocode("Москва");
        myGeocoder.then(
            function (res) {
                map.setCenter(res.geoObjects.get(0).geometry.getCoordinates(), 9);
            },
            function (err) {}
        );
        debugger;
    })
}

function resetGeoObjects(){
   
}


// var myObjectManager = new ymaps.ObjectManager({ clusterize: true }),
// currentId = 0;

// // Добавление единичного объекта.
// myObjectManager.add({
//     type: 'Feature',
//     id: currentId++,
//     geometry: {
//         type: 'Point',
//         coordinates: [56.23, 34.79]
//     },
//     properties: {
//         hintContent: 'Текст всплывающей подсказки',
//         balloonContent: 'Содержимое балуна'
//     }
// });
// map.geoObjects.add(myObjectManager);




