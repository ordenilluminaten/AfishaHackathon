//dropdown
var dropdown = {
    listeners: {},
    show: (el, id, onShowCb, onCloseCb) => {
     
        if(!el.classList.contains('selected')) {
            dropdown.listeners[id] = function (e) {
                //isClickInside?
                debugger;
                if (e.target.closest('.dropdown-content') == null && !el.contains(e.target))
                    dropdown.hide(el, id, onCloseCb);
            };
            el.classList.add('selected');
            document.addEventListener('click', dropdown.listeners[id]);
            if(onShowCb != null)
                onShowCb();
        }
        else if(event.target.closest('.dropdown-content') == null) {            
            dropdown.hide(el, id, onCloseCb);
        }
    },
    hide: (el, id, onCloseCb) => {
        debugger;
        document.removeEventListener('click', dropdown.listeners[id]);
        delete dropdown.listeners[id];
        el.classList.remove('selected');

        if(onCloseCb != null)
            onCloseCb();
    },
    toggle: (el, e, id, onShowCb, onCloseCb) => {        
        e.preventDefault();
        e.stopPropagation();

        const menu = document.getElementById(id);
        if (menu == null)
            return;

        const isVisible = menu.classList.contains('visible');
        isVisible 
            ? dropdown.hide(el, id, onShowCb, onCloseCb) 
            : dropdown.show(el, id, onShowCb, onCloseCb);
    }
}

var afishaDatetimePicker = (targets, options = {}) => {
    var defaultOptions = {
        locale: 'ru',
        defaultDate: new Date(),
        dateFormat: 'd M Y',
        enableTime: true,
        time_24hr: true
    };
    if (options == null) options = {};
    ObjectClass.merge(options, defaultOptions);

    return flatpickr(targets, options);
};

//router
var router = {
    current: null,
    ractive: null,
    navigate: (idPage, ...args) => {
        if (router.pages[idPage] == null)
            return;
        let prevPage = router.current;
        if (router.current != null) {
            if (router.current.id === idPage)
                return;
            appRactive.detachChild(router.ractive);
            router.current = null;
            router.ractive = null;
        }
        router.current = router.pages[idPage](prevPage, args);
        router.ractive = router.current.render();

        appRactive.attachChild(router.ractive, { target: 'page-data' });
        appRactive.set('currentPage', router.current);
    },
    pages: {
        places: () => new PlacesPage(),
        place: (parentPage, args) => new PlacePage(args[0], parentPage)
    }
}

//toastr
var toastr = {
    toasts: {},
    info: (msg, ms = 0) => {
        const id = toastr._createToast('info', msg);
        toastr._show(id);
        toastr._closeTimeout(id, ms);
        return id;
    },
    success: (msg, ms = 0) => {
        const id = toastr._createToast('success', msg);
        toastr._show(id);
        toastr._closeTimeout(id, ms);
        return id;
    },
    warning: (msg, ms = 0) => {
        const id = toastr._createToast('warning', msg);
        toastr._show(id);
        toastr._closeTimeout(id, ms);
        return id;
    },
    danger: (msg, ms = 0) => {
        const id = toastr._createToast('danger', msg);
        toastr._show(id);
        toastr._closeTimeout(id, ms);
        return id;
    },
    close: (id) => {
        let toast = toastr.toasts[id];
        if (toast == null)
            return;
        delete toastr.toasts[id];
        toast.classList.remove('show');
        toast.classList.add('closing');
        setTimeout(() => {
            const el = document.getElementById(`toastr_${id}`);
            if (el != null)
                el.remove();
        }, 450); //500ms - animation duration
    },
    _closeTimeout: (id, ms) => {
        if (ms > 0)
            setTimeout(() => toastr.close(id), ms);
        else setTimeout(() => toastr.close(id), 60000); //max timeout
    },
    _show: (id) => {
        let toast = toastr.toasts[id];
        document.body.appendChild(toast);
        toast.classList.add('show');
    },
    _createToast: (type, msg) => {
        const id = new Date().getTime();
        const toast = document.createElement('div');
        toast.innerHTML = msg;
        toast.setAttribute('id', `toastr_${id}`);
        toast.classList.add('toast');
        toast.classList.add(type);
        toastr.toasts[id] = toast;
        return id;
    }
}