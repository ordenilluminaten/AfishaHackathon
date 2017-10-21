class PlacePage extends Page {
    constructor(event, parentPage) {
        //TODO select title based on event type
        super('event', event.Name, parentPage);
        this.event = event;
    }

    render(...args) {   
        let page = Ractive({
            template: '#place-page-template',
            data: this.event,
            on: {
                init: () => {
                    // Request.post({
                    //     url: '/GetEventData',
                    //     data: {
                    //         idEvent: this.event.ID
                    //     }
                    // }).then((userEventData) => {
                    //     router.ractive.set('userEventData', userEventData);
                    // });
                },
                complete: () => {
                    Tabs.init("#page");
                    VK.Widgets.Comments('reviews', {}, this.event.id);
                },
                inviteCompanion: (ctx, idPlace) => {
                    var modal = new Modal({
                        id: 'invite-companion-modal',
                        title: 'Пригласить спутников',
                        content: {
                            request: {
                                method: 'GET',
                                url: '/CreateInviteCompanion',
                                data: {
                                    idPlace: idPlace
                                }
                            }
                        },
                        onLoaded: () => {
                            var date = new Date();
                            let ractiveDate = inviteRactive.get('Date');
                            if (ractiveDate != null)
                                date = new Date(ractiveDate);
                            afishaDatetimePicker(".companion-datepicker", {
                                defaultDate: date,
                                dateFormat: 'd M Y H:i',
                                onClose: (selectedDates, dateStr, instance) => {
                                    inviteRactive.set('Date', selectedDates[0].toLocaleString());
                                    this.updateItems();
                                }
                            });
                        },
                        type: ModalType.custom,
                        types: {
                            custom: {
                                buttons: {
                                    accept: {
                                        class: 'btn-confirm',
                                        text: 'Сохранить',
                                        attrs: {},
                                        callback: () => {
                                            var inviteData = inviteRactive.get();
                                            Request.post({
                                                url: '/CreateInviteCompanion',
                                                data: inviteData,
                                            }).then((data) => {
                                                router.ractive.set('userEventData.idUserEvent', data.idUserEvent);
                                                Modal.close('invite-companion-modal');
                                            });
                                        }
                                    },
                                    cancel: {
                                        class: 'btn-cancel',
                                        text: 'Закрыть',
                                        attrs: {},
                                        callback: () => {
                                            Modal.close('invite-companion-modal');
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        });
        page.attachChild(this._renderUserEvents(), { target: 'user-events' });
        return page;
    }

    _renderUserEvents() {
        this.userEventsRactive = PaginationRactive({
            template: '#user-events-template',
            data: {
                getItems: (filter, onDone) => {
                    filter.IdPlace = this.event.id;
                    Request.post({
                        url: '/Home/UserEvents',
                        data: filter
                    }).then(onDone);
                },
                Filter: {
                    viewType: 0
                },
                getAgeString: (age) => {
                    if (age <= 0) return 'любой';
                    return `${age}+`;
                },
                getDateString: (date) => {
                    return new Date(date).toLocaleString();
                },
                getGenderString: (gender) => {
                    return gender === 0 ? 'мужской' : gender === 1 ? 'женский' : 'любой';
                },
                getUserOffer: (offers, idUser) => {
                    let i = offers.findIndex(x => x.idUser === idUser);
                    return i < 0 ? null : offers[i];
                },
                getPendingCount: (offers) => {
                    let count = 0;
                    for (let offer of offers) {
                        if (offer.state === 0)
                            count++;
                    }
                    return count;
                },
                getAcceptedOffers: (offers) => {
                    return offers.filter(x => x.state === 1);
                }
            },
            on: {
                init: function () {
                    this.updateItems();
                },
                complete: function () {
                    afishaDatetimePicker("#datetimepicker",
                        {
                            minDate: new Date(),
                            dateFormat: 'd M Y H:i',
                            onClose: (selectedDates, dateStr, instance) => {
                                this.set('filter.date', selectedDates[0].toLocaleString());
                                this.updateItems();
                            }
                        });
                },
                createOffer: function (ctx, idUserEvent) {
                    Request.post({
                        url: '/Home/CreateOffer',
                        data: { idUserEvent: idUserEvent }
                    }).then(res => {
                        toastr.success('Заявка успешно отправлена', 3000);
                        this.updateItems();
                    });
                },
                removeOffer: function (ctx, idOffer) {
                    var modal = new Modal({
                        title: 'Вы уверены, что хотите удалить заявку?',
                        type: ModalType.confirm,
                        types: {
                            confirm: {
                                buttons: {
                                    accept: {
                                        callback: () => {
                                            Request.post({
                                                url: '/Home/RemoveOffer',
                                                data: { idOffer: idOffer }
                                            }).then(res => {
                                                toastr.success('Заявка успешно удалена', 3000);
                                                this.updateItems();
                                            });
                                            modal.close();
                                        }
                                    }
                                }
                            }
                        }
                    });
                },
                removeUserEvent: function (ctx, userEvent) {
                    let msg = 'Вы уверены, что хотите отменить мероприятие? ';
                    //есть хоть один в команде
                    let acceptedOffer = userEvent.offers.find(x => x.state == 1)
                    if (acceptedOffer != null) {
                        msg += `Вы будете удалены из мероприятия, а ответственность будет передана ${acceptedOffer.user.firstName} ${acceptedOffer.user.lastName}`;
                    }
                    var modal = new Modal({
                        title: msg,
                        type: ModalType.confirm,
                        types: {
                            confirm: {
                                buttons: {
                                    accept: {
                                        callback: () => {
                                            Request.post({
                                                url: '/Home/RemoveUserEvent',
                                                data: { idUserEvent: userEvent.id }
                                            }).then(res => {
                                                if (acceptedOffer != null) {
                                                    toastr.success('Вы удалены из мероприятия', 3000);
                                                } else {
                                                    toastr.success('Мероприятие успешно удалено', 3000);
                                                }
                                                router.ractive.set('userEventData.idUserEvent', null);
                                                this.updateItems();
                                            });
                                            modal.close();
                                        }
                                    }
                                }
                            }
                        }
                    });
                },
                inviteFriend: function(){
                    var modal = new Modal({
                        id: 'invite-friends-modal',
                        title: 'Пригласить друзей',
                        type: ModalType.custom,
                        content:{
                            html:'<div id="invite-friends-wrapper"></div>',
                            ractiveOptions:{
                                enable: true,
                                wrapper: 'invite-friends-wrapper',
                                template: 'invite-friends-template',
                                data:{
                                    filter:{
                                        search:null
                                    },
                                    filterFriends: (friends, filter)=>{
                                        if(filter.search == null || filter.search.length == 0)
                                            return friends;
                                        let search = filter.search.toLowerCase();
                                        let res = [];
                                        for (var key in friends) {
                                            let friend = friends[key];
                                            if(friend.first_name.toLowerCase().includes(search)
                                            || friend.last_name.toLowerCase().includes(search)
                                            || friend.id.toString().includes(search))
                                            res.push(friend);
                                        }
                                        return res;
                                    }
                                },
                                events: {
                                    inviteFriend: (ctx, idFriend) => {
                                        Request
                                    }
                                }
                            }   
                        },
                        types: {
                            custom: {
                                buttons: {
                                    cancel: {
                                        class: 'btn-default',
                                        text: 'Закрыть',
                                        attrs: {},
                                        callback: () => {
                                            Modal.close('invite-friends-modal');
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
            }
        });
        return this.userEventsRactive;
    }
}