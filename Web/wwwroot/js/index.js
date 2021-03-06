﻿var header = Ractive.extend({
    isolated: false,
    template: '#header-template',
    data: function () {
        return {
            friendFilter: (item, seach) => {
                if (seach == null || seach == "")
                    return item;

                seach = seach.toLowerCase();
                if ((item.first_name != null
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
                dropdown.hide(document.getElementById('bot-dropdown-li'), 'bot-dropdown');
            });
        },
        acceptOffer: (_ctx, idEvent, idUser) => {
            Request.post({
                method: Request.method.post,
                url: '/acceptOffer',
                data: {
                    idUserEvent:idEvent,
                    idUser
                }
            }).then(() => {
                Header.refreshMyNotifications();
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
        }).then(() => {
            Header.refreshMyPlaces();
            Header.refreshFriendPlaces();
            Header.refreshMyNotifications();
        });
    });
}

function onInited() {
    getVkfriends();
}

var yaMap = null;
function initYaMap(city) {
    ymaps.ready(initUserLocation);
}

function initUserLocation() {
    //пытаемся достать город из бд
    var lat = appRactive.get('currentUser.customData.latitude');
    var lon = appRactive.get('currentUser.customData.longitude');
    if (lon != null && lat != null) {
        setupMap([lat, lon]);
    } else {
        selectCity(1, 'Москва');
    }
}

function setupMap(coords) {
    if (yaMap == null) {
        yaMap = new ymaps.Map('myMap', {
            center: coords,
            zoom: 9
        });
        resetGeoObjects(router.ractive.get('items'));
    }else{
        yaMap.setCenter(coords, 9);
    }
}

function selectCity(id, name) {
    ymaps.geocode(name).then(res => {
        let coords = res.geoObjects.get(0).geometry.getCoordinates();
        setupMap(coords);
        debugger;
        appRactive.set('currentUser.customData.idCity', id);
        if(router.current.id != "place"){
            router.ractive.set('filter.idCity',id);
            router.ractive.updateItems();
        }
        Request.post({
            url: '/Home/SelectCity',                                                                           
            data: {
                idCity: id,
                lat: coords[0],
                lon: coords[1]
            }
        })
    });
}

function resetGeoObjects(items){
    if(yaMap == null || yaMap.geoObjects==null)
        return;
    yaMap.geoObjects.removeAll()

    var objectManager = new ymaps.ObjectManager({ clusterize: true })
    debugger;
    for(let item of items){
        if(item.coordinate == null)
            continue;
        // Добавление единичного объекта.
        objectManager.add({
            type: 'Feature',
            id: item.id,
            geometry: {
                type: 'Point',
                coordinates: [item.coordinate.lat, item.coordinate.lon]
            },
            properties: {
                hintContent: item.name,
                balloonContent: `<img src='${item.photos[0]}' width="40px" height="40px" style="float:left; margin-right: 5px;object-fit: cover; " />
                                 <div style="float:left">
                                    <h3>${item.name}</h3>
                                    <span>${item.address}</span>
                                 </div>`
            }
        });
    } 
    yaMap.geoObjects.add(objectManager);
}

function declOfNum(_number, _stringArray) {
    if (_number < 0)
        _number *= -1;
    const numArray = [2, 0, 1, 1, 1, 2];
    return _stringArray[_number % 100 <= 4 || _number % 100 >= 20 ? numArray[_number % 10 < 5 ? _number % 10 : 5] : 2];
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




