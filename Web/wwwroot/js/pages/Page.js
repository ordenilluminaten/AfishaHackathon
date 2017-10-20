class Page {
    /**
     * Страница
     * @param {string} id
     * @param {string} title
     * @param {Page} parentPage
     */
    constructor(id, title, parentPage = null) {
        this.id = id;
        this.title = title;
        this.parent = parentPage;
    }

    /**
     * returns Ractive instance
     * @param {any} args
     */
    render(...args) { }
}