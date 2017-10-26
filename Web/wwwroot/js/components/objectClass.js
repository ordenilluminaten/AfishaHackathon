class ObjectClass {
	/**
    * Глубокий мерж двух объектов
    * @param {{}} _target - итоговый объект
    * @param {{}} _sourse - объект с которым мержим
    */
	static applyChanges(_target, _sourse) {
		for (let sourcePropertyKey in _sourse) {
			if (_target[sourcePropertyKey] != null && typeof (_target[sourcePropertyKey]) === 'object' && !Array.isArray(_target[sourcePropertyKey])) {
				this.merge(_target[sourcePropertyKey], _sourse[sourcePropertyKey]);
			} else
				_target[sourcePropertyKey] = _sourse[sourcePropertyKey];
		}
	}

	/**
	* Поверхностный мерж двух объектов
	* @param {{}} _target - итоговый объект
	* @param {{}} _sourse - объект с которым мержим
	*/
	static shallowApplyChanges(_target, _sourse) {
		for (let soursePropertyKey in _sourse) {
			_target[soursePropertyKey] = _sourse[soursePropertyKey];
		}
	}

	/**
	* Глубокий мерж двух объектов
	* @param {{}} _target - итоговый объект
	* @param {{}} _sourse - объект с которым мержим
	*/
	static merge(_target, _sourse) {
		for (let sourcePropertyKey in _sourse) {
			if (_target[sourcePropertyKey] != null && typeof (_target[sourcePropertyKey]) === 'object' && !Array.isArray(_target[sourcePropertyKey])) {
				this.merge(_target[sourcePropertyKey], _sourse[sourcePropertyKey]);
			} else if (typeof _target[sourcePropertyKey] === 'undefined')
				_target[sourcePropertyKey] = _sourse[sourcePropertyKey];
		}
	}

	/**
     * Поверхностный мерж двух объектов
     * @param {{}} _target - итоговый объект
     * @param {{}} _sourse - объект с которым мержим
     */
	static shallowMerge(_target, _sourse) {
		for (let sourcePropertyKey in _sourse) {
			if (typeof _target[sourcePropertyKey] === 'undefined')
				_target[sourcePropertyKey] = _sourse[sourcePropertyKey];
		}
	}

	static toFormData(_object, _parentPath) {
		_parentPath = _parentPath == null || _parentPath.length === 0 ? '' : _parentPath;
		let resultString = '';
		for (let key in _object) {
			const objectValue = _object[key];
			if (objectValue == null)
				continue;
			const objectType = typeof (objectValue);
			const isArray = Array.isArray(objectValue);
			const isObject = objectType === 'object';
			if (!isObject && !isArray) {
				if (_parentPath.length !== 0) {
					resultString += `&${_parentPath}${encodeURIComponent('.')}${key}=${encodeURIComponent(objectValue)}`;
				} else {
					resultString += `&${key}=${encodeURIComponent(objectValue)}`;
				}
			} else if (isObject && isArray) {
				if (_parentPath.length !== 0) {
					for (let i = 0; i < objectValue.length; i++) {
						resultString += `&${_parentPath}${encodeURIComponent('.')}${key}[${i}]=${encodeURIComponent(objectValue[i])}`;
					}
				} else {
					for (let i = 0; i < objectValue.length; i++) {
						resultString += `&${_parentPath}${key}[${i}]=${encodeURIComponent(objectValue[i])}`;
					};
				}
			} else if (isObject && !isArray) {
				const childParentPath = _parentPath.length === 0 ? key : `${_parentPath}.${key}`;
				resultString += `&${ObjectClass.toFormData(objectValue, childParentPath)}`;
			}
		}
		if (resultString[0] === '&')
			resultString = resultString.substr(1, resultString.length - 1);
		return resultString;
	}
};