class PlacesPage extends Page {
    constructor() {
        super('places', 'Все события');
    }

    render(...args) {
        return PaginationRactive({
            template: '#places-page-template',
            data: {
                getItems: (filter, onDone) => {
                    Request.post({
                        url: '/Home/Places',
                        data: filter
                    }).then(onDone);
                },
                filter: {
                    category: 0,                    
                    city: 1,
                    viewType: 'Grid'
                }
            },
            on: {
                init: function () {
                    this.updateItems();
                },
                complete: function () {
                }
            }
        });
    }
}