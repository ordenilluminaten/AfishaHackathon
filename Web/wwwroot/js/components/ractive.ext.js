var ractiveHelpers = Ractive.defaults.data;
Ractive.defaults.isolated = false;
/**
 * Разблокирует нод
 * @param {{}} _node - node  полученный из ractive
 */
Ractive.defaults.enableElement = (_ctx) => {
    if (_ctx.original == null)
        return;
    _ctx.node.classList.remove('disabled');
    _ctx.node.removeAttribute('disabled');
};
/**
 * Блокирует нод
 * @param {{}} _node - node полученный из ractive
 * true - Есть двойные клики, false - нет
 */
Ractive.defaults.disableElement = (_ctx) => {
    if (_ctx.original == null)
        return false;
    if (_ctx.original.detail > 1)
        return true;

    _ctx.node.classList.add('disabled');
    _ctx.node.setAttribute('disabled', 'disabled');

    return false;
};
ractiveHelpers.DeclOfNum = (_number, _stringArray) => {
    return declOfNum(_number, _stringArray);
}
ractiveHelpers.toFixed = function (_number, _count) {
    if (_number == 0 || _number == null || _count == null)
        return 0;
    return parseFloat(_number).toFixed(_count);
}

var Paginator = Ractive.extend({
    isolated: false,
    template: '#paginator-template'
});

//шаблонный метод getItems => function getItems(filter, ondone){тут действия}
//должен быть Filter
var PaginationRactive = Ractive.extend({
    data: function () {
        return {
            items: [],
            getSortType: function (_name) {
                if (_name === this.get('filter.sortName'))
                    return this.get('filter.sortType');
                return 'unsorted';
            }
        };
    },
    isolated: true,
    components: {
        Paginator: Paginator
    },
    oninit: function () {
        var self = this;
        //подписки
        {
            self.updateItems = function () {
                return new Promise((resolve, reject) => {
                    if (self.fire('updateItems')) {
                        resolve();
                        return;
                    }
                    reject();
                });
            }

            self.on({
                updateItems: function (_ctx) {
                    const getItemsFunc = this.get('getItems');
                    if (getItemsFunc === null) {
                        console.log('Функция getItems не назначена');
                        return false;
                    }
                    getItemsFunc(this.get('filter'), this.get('setData'));
                    return true;
                },
                calcPagination: function (_ctx) {
                    const filter = self.get('filter');
                    const pages = [];
                    if (filter.pageTotal < 6) {
                        for (var i = 1; i <= filter.pageTotal; i++) {
                            pages.push({ num: i, active: filter.page == i });
                        }
                    } else {
                        if (filter.page < 4) {
                            for (var i = 1; i <= 4; i++) {
                                pages.push({ num: i, active: filter.page == i });
                            }
                            pages.push({ num: filter.pageTotal });
                        } else if (filter.page >= 4 && filter.page < filter.pageTotal - 1) {
                            for (var i = filter.page - 2; i <= filter.page + 1; i++) {
                                pages.push({ num: i, active: filter.page == i });
                            }
                            pages.push({ num: filter.pageTotal });
                        } else {
                            pages.push({ num: 1 });
                            for (var i = filter.pageTotal - 3; i <= filter.pageTotal; i++) {
                                pages.push({ num: i, active: filter.page == i });
                            }
                        }
                    }
                    self.set('pages', pages);
                },
                changeSort: function (_ctx, _name) {
                    const sortType = self.get('filter.sortType') === 'asc' ? 'desc' : 'asc';
                    self.set('filter.sortType', sortType);
                    self.set('filter.sortName', _name);
                    self.set('filter.sort', _name + ':' + sortType);
                    self.set('filter.page', 1);
                    self.fire('updateItems');
                },
                changeSelectValue: function (_ctx) {
                    const sort = self.get('filter.sort');
                    if (sort.indexOf(':') != -1) {
                        const splittedVal = sort.split(':');

                        self.set('filter.sortName', splittedVal[0]);
                        self.set('filter.sortType', splittedVal[1]);

                        self.set('filter.page', 1);
                        self.fire('updateItems');
                    }
                },
                toggleAll: function (_ctx) {
                    if (this.get('filter.selectAll')) {
                        this.set('filter.selectedItems', []);
                    } else {
                        const items = this.get('items');
                        const itemsIds = [];
                        for (let i = 0; i < items.length; i++) {
                            itemsIds.push(items[i].Id);
                        }
                        this.set('filter.selectedItems', itemsIds);
                    }
                },
                toggleRow: (_ctx, _id) => {
                    const items = self.get('filter.selectedItems');
                    const index = items.indexOf(_id);
                    if (index !== -1) {
                        self.set('filter.selectAll', false);
                    }
                },
                changeValue: (_ctx, _name, _value, _needUpdate) => {
                    screenLeft
                    self.set('filter.' + _name, _value);
                    if (_needUpdate !== false) {
                        self.set('filter.page', 1);
                        self.fire('updateItems');
                    }
                },
                selectPage: (_ctx, _page, _isInput) => {
                    if (_isInput) {
                        if (_ctx.original.keyCode === 13) {
                            if (_page <= 0 || _page > self.get('filter.pageTotal') || _page === self.get('filter.page')) {
                                self.set('filter.pageTemp', self.get('filter.page'));
                                return;
                            }
                            self.set('filter.page', _page);
                            self.fire('updateItems');
                        }
                    } else {
                        if (_page <= 0 || _page > self.get('filter.pageTotal') || _page === self.get('filter.page')) {
                            self.set('filter.pageTemp', self.get('filter.page'));
                            return;
                        }
                        self.set('filter.page', _page);
                        self.fire('updateItems');
                    }
                }
            });
        }

        // минимум для запроса
        var dataItems = self.get('items');
        if (dataItems != undefined && dataItems.length !== 0) {
            self.set('items', dataItems);
            self.set('filter.pageTemp', self.get('filter.page'));
            self.fire('calcPagination');
        } else {
            self.set('items', []);
            self.set('filter.pageTemp', 1);
        }
        this.set('setData', (_data, _callBack) => {
            _data.filter.pageTemp = _data.filter.page;
            self.set('filter', _data.filter);
            self.set('items', _data.items);

            // if (self.get('filter.sortName') === null) {
            //     if (_data.Filter.sort !== null) {
            //         var split = _data.Filter.sort.split('_');
            //         _data.Filter.sortName = split[0];
            //         _data.Filter.sortType = split[1];
            //     }
            // } else {
            //     _data.Filter.sortName = self.get('filter.sortName');
            //     _data.Filter.sortType = self.get('filter.sortType');
            // }            
            self.fire('calcPagination');
            if (typeof (_callBack) == 'function')
                _callBack();
        });

        this.observe('filter.page', (_oldVal, _newVal, _key) => {
            if (_newVal !== _oldVal) {
                self.set('filter.selectAll', false);
            }
        });


    }
});

Ractive.events.enter = function (node, fire) {
    const keyup = function (e) {
        if (e.which === 13) {
            fire({
                node: node,
                original: e
            });
        }
    };

    node.addEventListener('keyup', keyup);

    return {
        teardown: function () {
            node.removeEventListener('keyup', keyup);
        }
    };
};