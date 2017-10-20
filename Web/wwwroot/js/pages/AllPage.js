class AllPage extends Page {
    constructor() {
        super('all', 'Все события');
    }

    render(...args) {
        return Ractive({ template: '#all-page-template', data: {} });
    }
}