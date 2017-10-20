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
            Items: [],
            getSortType: function (_name) {
                if (_name === this.get('Filter.SortName'))
                    return this.get('Filter.SortType');
                return 'unsorted';
            }
        };
    },
    isolated: false,
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
                    getItemsFunc(this.get('Filter'), this.get('SetData'));
                    return true;
                },
                calcPagination: function (_ctx) {
                    const filter = self.get('Filter');
                    const pages = [];
                    if (filter.PageTotal < 6) {
                        for (var i = 1; i <= filter.PageTotal; i++) {
                            pages.push({ Num: i, Active: filter.Page == i });
                        }
                    } else {
                        if (filter.Page < 4) {
                            for (var i = 1; i <= 4; i++) {
                                pages.push({ Num: i, Active: filter.Page == i });
                            }
                            pages.push({ Num: filter.PageTotal });
                        } else if (filter.Page >= 4 && filter.Page < filter.PageTotal - 1) {
                            for (var i = filter.Page - 2; i <= filter.Page + 1; i++) {
                                pages.push({ Num: i, Active: filter.Page == i });
                            }
                            pages.push({ Num: filter.PageTotal });
                        } else {
                            pages.push({ Num: 1 });
                            for (var i = filter.PageTotal - 3; i <= filter.PageTotal; i++) {
                                pages.push({ Num: i, Active: filter.Page == i });
                            }
                        }
                    }
                    self.set('Pages', pages);
                },
                changeSort: function (_ctx, _name) {
                    const sortType = self.get('Filter.SortType') === 'asc' ? 'desc' : 'asc';
                    self.set('Filter.SortType', sortType);
                    self.set('Filter.SortName', _name);
                    self.set('Filter.Sort', _name + ':' + sortType);
                    self.set('Filter.Page', 1);
                    self.fire('updateItems');
                },
                changeSelectValue: function (_ctx) {
                    const sort = self.get('Filter.Sort');
                    if (sort.indexOf(':') != -1) {
                        const splittedVal = sort.split(':');

                        self.set('Filter.SortName', splittedVal[0]);
                        self.set('Filter.SortType', splittedVal[1]);

                        self.set('Filter.Page', 1);
                        self.fire('updateItems');
                    }
                },
                toggleAll: function (_ctx) {
                    if (this.get('Filter.SelectAll')) {
                        this.set('Filter.SelectedItems', []);
                    } else {
                        const items = this.get('Items');
                        const itemsIds = [];
                        for (let i = 0; i < items.length; i++) {
                            itemsIds.push(items[i].Id);
                        }
                        this.set('Filter.SelectedItems', itemsIds);
                    }
                },
                toggleRow: (_ctx, _id) => {
                    const items = self.get('Filter.SelectedItems');
                    const index = items.indexOf(_id);
                    if (index !== -1) {
                        self.set('Filter.SelectAll', false);
                    }
                },
                changeValue: (_ctx, _name, _value, _needUpdate) => {
                    self.set('Filter.' + _name, _value);
                    if (_needUpdate !== false) {
                        self.set('Filter.Page', 1);
                        self.fire('updateItems');
                    }
                },
                "Paginator.selectPage": (_ctx, _page, _isInput) => {
                    if (_isInput) {
                        if (_ctx.original.keyCode === 13) {
                            if (_page <= 0 || _page > self.get('Filter.PageTotal') || _page === self.get('Filter.Page')) {
                                self.set('Filter.PageTemp', self.get('Filter.Page'));
                                return;
                            }
                            self.set('Filter.Page', _page);
                            self.fire('updateItems');
                        }
                    } else {
                        if (_page <= 0 || _page > self.get('Filter.PageTotal') || _page === self.get('Filter.Page')) {
                            self.set('Filter.PageTemp', self.get('Filter.Page'));
                            return;
                        }
                        self.set('Filter.Page', _page);
                        self.fire('updateItems');
                    }
                }
            });
        }

        // минимум для запроса
        var dataItems = self.get('Items');
        if (dataItems != undefined && dataItems.length !== 0) {
            self.set('Items', dataItems);
            self.set('Filter.PageTemp', self.get('Filter.Page'));
            self.fire('calcPagination');
        } else {
            self.set('Items', []);
            self.set('Filter.PageTemp', 1);
        }
        this.set('SetData', (_data, _callBack) => {

            if (typeof(_data.Items) == 'undefined') {
                var res = JSON.parse(_data.Data);
                self.set('Items', res[Object.keys(res)[0]].Items);
            } else {
                self.set('Items', _data.Items);
            }
            _data.Filter.PageTemp = _data.Filter.Page;

            if (self.get('Filter.SortName') === null) {
                if (_data.Filter.Sort !== null) {
                    var split = _data.Filter.Sort.split('_');
                    _data.Filter.SortName = split[0];
                    _data.Filter.SortType = split[1];
                }
            } else {
                _data.Filter.SortName = self.get('Filter.SortName');
                _data.Filter.SortType = self.get('Filter.SortType');
            }
            self.set('Filter', _data.Filter);
            self.fire('calcPagination');
            if (typeof (_callBack) == 'function') {
                _callBack();
            }
        });

        this.observe('Filter.Page', (_oldVal, _newVal, _key) => {
            if (_newVal !== _oldVal) {
                self.set('Filter.SelectAll', false);
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