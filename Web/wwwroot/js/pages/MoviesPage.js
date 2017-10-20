class MoviesPage extends Page {
    constructor() {
        super('movies', 'Кино');
    }

    render(...args) {
        debugger;
        return PaginationRactive({
            template: '#movies-page-template',
            data: {
                getItems: (filter, onDone) => {
                    Request.post({
                        url: '/Home/Movies',
                        data: filter
                    }).then(onDone);
                },
                Filter: {
                    ViewType: 0
                }
            },
            on: {
                init: function () {
                    this.updateItems();
                },
                complete: function () {
                    afishaDatetimePicker("#datepicker",
                        {
                            enableTime: false,
                            onClose: (selectedDates, dateStr, instance) => {
                                this.set('Filter.Date', selectedDates[0].toLocaleString());
                                this.updateItems();
                            }
                        });
                }
            }
        });
    }
}