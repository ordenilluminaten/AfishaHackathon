class PlacesPage extends Page {
    constructor() {
        super('places', 'Все события');
    }

    render(...args) {
        return new PaginationRactive({
            template: '#places-page-template',
            data:function(){
                return {
                getItems: (filter, onDone) => {
                    Request.post({
                        url: '/Home/Places',
                        data: filter
                    }).then(onDone);
                }
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