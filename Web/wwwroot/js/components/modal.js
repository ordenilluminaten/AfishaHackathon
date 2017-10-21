const ModalType = {
	alert: 'alert',
	confirm: 'confirm',
	custom: 'custom'
};

class Modal {

	constructor(opts) {
		this.options = opts;
		ObjectClass.merge(opts, this.defaultOptions);

		this._init().then(() => {
			this.options.onLoaded();
			 if (this.options.content.ractiveOptions.enable === true) {
			 	this._createRactive();
			 }
		});
		this.show();
	}

    get defaultOptions() {
        const id = new Date().getTime();
		return {
			id: `modal-${id}`,
			title: '',
			class: 'modal',
			//useOverlay: true,
			styles: {
				width: '600px'
			},
			escToClose: true,
			clickOverlay: true,
			onClosed: () => { }, //модал закрыт
			onShown: () => { }, //модал открыт
			onLoaded: () => { }, //все данные загружены
			closeConfirm: {
				enable: false,
				conditions: () => true,
				text: 'Вы уверены что хотите закрыть?'
			},
			content: {
				html: null,
				ractiveOptions: {
					enable: false,
					wrapper: `wrapper-${id}`,
					template: `template-${id}`,
					data: null,
					events: {
						init: () => {

						},
						complete: () => {

						}
					},
					parentRactive: null,
					ractive: null
				},
				request: {
					method: 'GET',
					async: true,
					url: null,
					data: {

					}
				}
			},
			type: ModalType.alert,
			types: {
				confirm: {
					buttons: {
						accept: {
							class: 'btn-confirm',
							text: 'Да',
							attrs: {},
							callback: () => {

							}
						},
						cancel: {
							class: 'btn-cancel',
							text: 'Нет',
							attrs: {},
							callback: () => {
								this.close();
							}
						}
					}
				},
				alert: {
					buttons: {
						close: {
							class: null,
							text: 'Закрыть',
							attrs: {},
							callback: () => {
								this.close();
							}
						}
					}
				},
				custom: {
					buttons: {}
				}
			}
		}
	}

	_init() {
		return new Promise((resolve, reject) => {
			const modal = document.createElement('div');
			modal.setAttribute('id', this.options.id);
			modal.classList.add(this.options.class);

			const closeEl = document.createElement('div');
			closeEl.classList.add('close-modal');
			closeEl.innerHTML = '<span>&#43;</span>';
			closeEl.addEventListener('click', () => {
				this.close();
			});

			modal.appendChild(closeEl);
			const content = document.createElement('div');
			content.classList.add('content');

			const title = document.createElement('h4');
			title.innerHTML = this.options.title;

			content.appendChild(title);

			if (this.options.content.html != null) {
				this._insertHtml(content, this.options.content.html);
			} else {
				this._downloadContent().then(data => {
					if (data != null)
						this._insertHtml(content, data);
					resolve();
				}, e => {
					content.innerHTML += 'Ошибка загрузки контента';
					Logger.Log(e);
					reject();
				});
			}
			modal.appendChild(content);
			const buttons = Object.values(this.options.types[this.options.type].buttons);
			if (buttons.length > 0) {
				const buttonsEl = document.createElement('div');
				buttonsEl.classList.add('buttons');

				for (let btn of buttons) {
					let btnEl = document.createElement('button');
					btnEl.innerHTML = btn.text;

					if (btn.class != null)
						btnEl.classList.add(btn.class);
					if (btn.attrs != null) {
						for (let attr in btn.attrs) {
							btnEl.setAttribute(attr, btn.attrs[attr]);
						}
					}
					if (typeof btn.callback === 'function')
						btnEl.addEventListener('click', () => {
							btn.callback();
						});

					buttonsEl.appendChild(btnEl);
				}
				modal.appendChild(buttonsEl);
			}

			this._initStyles(modal);

			const modalOverlay = document.createElement('div');
			modalOverlay.classList.add('modal-overlay');
			modalOverlay.appendChild(modal);

			if (this.options.clickOverlay === true)
				modalOverlay.addEventListener('click',
                    (e) => {
                    	if (e.target !== modalOverlay)
                    		return false;
                    	this.close();
                    });


			this.body = modalOverlay;

			Modal.modals[this.options.id] = this;

			if (this.options.content.request.url == null)
				resolve();
		});
	}

	_initStyles(modal) {
		for (let propKey in this.options.styles)
			modal.style[propKey] = this.options.styles[propKey];
	}

	_createRactive() {
		debugger;
		this.options.content.ractiveOptions.ractive = new Ractive({
			el: `#${this.options.content.ractiveOptions.wrapper}`,
			template: `#${this.options.content.ractiveOptions.template}`,
			data: () => {
				return this.options.content.ractiveOptions.data;
			},
			on: this.options.content.ractiveOptions.events
		});
		if (this.options.content.ractiveOptions.parentRactive != null) {
			this.options.content.ractiveOptions.parentRactive
                .attachChild(this.options.content.ractiveOptions.ractive);
		}
	}

	_loadScript(el, script) {
		return new Promise(function (resolve, reject) {
			let src = script.getAttribute('src');
			let type = script.getAttribute('type');
			let id = script.getAttribute('id');

		    if (type == null)
		        type = 'text/javascript';

			let newScript = document.createElement('script');
			if (src != null && src !== '')
				newScript.src = src;

			if (id != null && id !== '')
				newScript.id = id;

			newScript.type = type;
			newScript.innerHTML = script.innerHTML;
			newScript.onload = resolve;
			newScript.onerror = reject;

			script.replaceWith(newScript);
		});
	}

	_insertHtml(el, html) {
		if (this.options.content.ractiveOptions.enable === true) {
			const ractiveWrapper = document.createElement('div');
			ractiveWrapper.setAttribute('id', this.options.content.ractiveOptions.wrapper);

			const ractiveTemplate = document.createElement('script');
			ractiveTemplate.setAttribute('type', 'text/ractive');
			ractiveTemplate.setAttribute('id', this.options.content.ractiveOptions.template);

			ractiveTemplate.innerHTML += html;
			ractiveWrapper.appendChild(ractiveTemplate);
			el.appendChild(ractiveWrapper);
		}
		else {
			console.time("a");
			el.innerHTML += html;
			let scripts = el.getElementsByTagName("script");
			let promises = [];

			for (let i = 0; i < scripts.length; i++)
				promises.push(this._loadScript(el, scripts[i]));

			Promise.all(promises);
			console.timeEnd("a");
		}
	}

	_downloadContent() {
		return new Promise((resolve, reject) => {
			if (this.options.content.request.url == null) {
				resolve();
				return;
			}

			Request.ajax({
				url: this.options.content.request.url,
				method: this.options.content.request.method,
				data: this.options.content.request.data
			}).then((response) => {
				resolve(response);
			}, (error) => {
				reject(error);
			});
		});
	}

	_showBodyScrollbar() {
		document.body.style.paddingRight = 0;
		document.body.style.overflowY = 'auto';
	}

	_hideBodyScrollbar() {
		document.body.style.paddingRight = window.innerWidth - document.body.offsetWidth + 'px';
		document.body.style.overflowY = 'hidden';
	}

	destroy() {
		this._showBodyScrollbar();
		this.body.remove();
		this.options.onClosed();
		if (this.options.content.ractiveOptions.enable === true) {
			if (this.options.content.ractiveOptions.parentRactive != null)
				this.options.content.ractiveOptions.parentRactive
                    .detachChild(this.options.content.ractiveOptions.ractive);
		}

		delete Modal.modals[this.options.id];
	}

	show() {
		this._hideBodyScrollbar();
		document.body.appendChild(this.body);
		this.options.onShown();
	}

	close() {
		if (this.options.closeConfirm.enable === true && this.options.closeConfirm.conditions() === true) {
			const _this = this;
			this.body.style.overflowY = 'hidden';

			const confirmModal = new Modal({
				title: _this.options.closeConfirm.text,
				type: ModalType.confirm,
				onClosed: () => {
					_this.body.style.overflowY = 'auto';
				},
				types: {
					confirm: {
						buttons:
                        {
                        	accept: {
                        		callback: () => {
                        			confirmModal.close();
                        			_this.destroy();
                        		}
                        	},
                        	cancel: {
                        		callback: () => {
                        			confirmModal.close();
                        			_this.body.style.overflowY = 'auto';
                        		}
                        	}
                        }
					}
				}
			});
		} else {
			this.destroy();
		}
	}

	static get(id) {
		return Modal.modals[id];
	}

	static close(id) {
		Modal.get(id).close();
	}
}

Modal.modals = {};
window.addEventListener('keydown', function (e) {
	if (e.key == 'Escape' || e.key == 'Esc' || e.keyCode == 27) {
		const modalKeys = Object.keys(Modal.modals);
		if (modalKeys.length > 0) {
			const lastModal = Modal.get(modalKeys[modalKeys.length - 1]);
			if (lastModal.options.escToClose === true)
				lastModal.close();
		}
		e.preventDefault();
		return false;
	}
}, true);