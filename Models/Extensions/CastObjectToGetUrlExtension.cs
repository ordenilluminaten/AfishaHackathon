using System;
using System.Collections;
using System.Reflection;

namespace Models.Extensions {
    public static class CastObjectToGetUrlExtension {
        private static readonly BindingFlags p_bindingMask = BindingFlags.Instance | BindingFlags.Public;

        /// <summary>
        /// Функция преобразования объекта в форм дату
        /// </summary>
        /// <param name="_object">Объект для преобразования</param>
        /// <param name="_firstObjectName"></param>
        /// <param name="_parentPath">Путь относительно родительского свойства</param>
        /// <param name="_isField">Является ли объект полем структуры</param>
        /// <returns>Список спар ключ - значение полученный из объекта</returns>
        public static string ToGetUrlQuery(this object _object, string _firstObjectName = "", string _parentPath = "", bool _isField = false) {
            var resultString = "";
            var mainObjectType = _object.GetType();
            var props = mainObjectType.GetProperties(p_bindingMask);
            var fields = mainObjectType.GetFields(p_bindingMask);

            if (props.Length == 0 && fields.Length == 0) {
                var url = "";
                //Если тип по значению то приводим к строке
                if (_object.AddFormDataAsValueType(_parentPath, null, ref url))
                    return resultString + url;
                //Если тип строковый то явно приводим к строке
                if (_object.AddFormDataAsString(_parentPath, null, ref url))
                    return resultString + url;
            } else if (!_isField) {
                if (string.IsNullOrWhiteSpace(_parentPath)) {
                    _parentPath = string.IsNullOrEmpty(_firstObjectName) ? "" : _firstObjectName;
                } else {
                    if (!mainObjectType.IsAnonymousType()) {
                        _parentPath = $"{_parentPath}.{mainObjectType.Name}";
                    }
                }
            }
            var propEnumerator = props.GetEnumerator();
            while (propEnumerator.MoveNext()) {
                var propertyInfo = (PropertyInfo)propEnumerator.Current;
                var type = propertyInfo.GetType();
                var value = propertyInfo.GetValue(_object);
                value.ProcessObjectToFormDataCast(type, _parentPath, propertyInfo.Name, ref resultString);
            }
            var fieldsEnumerator = fields.GetEnumerator();
            while (fieldsEnumerator.MoveNext()) {
                var fieldInfo = (FieldInfo)fieldsEnumerator.Current;
                var type = fieldInfo.GetType();
                var value = fieldInfo.GetValue(_object);
                value.ProcessObjectToFormDataCast(type, _parentPath, fieldInfo.Name, ref resultString);
            }
            return resultString;
        }

        private static void ProcessObjectToFormDataCast(this object _value, Type _type, string _parentPath, string _valueName, ref string _url) {
            if (_value == null)
                return;
            //Если тип по значению то приводим к строке
            if (_value.AddFormDataAsValueType(_valueName, _parentPath, ref _url))
                return;
            //Если тип строковый то явно приводим к строке
            if (_value.AddFormDataAsString(_valueName, _parentPath, ref _url))
                return;
            var isParentPathNull = string.IsNullOrWhiteSpace(_parentPath);
            //Если тип словарь то для каждого элемента вызываем рекурсию
            var valAsIDictionary = _value as IDictionary;
            if (valAsIDictionary != null) {
                var valueEnumerator = valAsIDictionary.GetEnumerator();
                while (valueEnumerator.MoveNext()) {
                    var keyValue = (DictionaryEntry)valueEnumerator.Current;
                    var path = isParentPathNull ? $"{_valueName}[{keyValue.Key}]" : $"{_parentPath}.{_valueName}[{keyValue.Key}]";
                    _url += $"{(_url.Length == 0 ? "" : "&")}" + keyValue.Value.ToGetUrlQuery(null, path);
                }
                return;
            }
            //Если тип перечисление то для каждого элемента вызываем рекурсию
            var valAsIEnumerable = _value as IEnumerable;
            if (valAsIEnumerable != null) {
                var valueEnumerator = valAsIEnumerable.GetEnumerator();
                var i = 0;
                while (valueEnumerator.MoveNext()) {
                    var path = isParentPathNull ? $"{_valueName}[{i}]" : $"{_parentPath}.{_valueName}[{i}]";
                    _url += $"{(_url.Length == 0 ? "" : "&")}" + valueEnumerator.Current.ToGetUrlQuery(null, path);
                    i++;
                }
                return;
            }
            //Если тип классы то вызываем рекурсию для класса
            _parentPath = isParentPathNull ? _valueName : $"{_parentPath}.{_valueName}";
            if (!_type.IsClass)
                throw new InvalidOperationException("can`t cast object to form data");
            _url += $"{(_url.Length == 0 ? "" : "&")}" + _value.ToGetUrlQuery(null, _parentPath);
        }
        /// <summary>
        /// Добавить пару ключ - значение с значением типа передаваемого по значению
        /// </summary>
        /// <param name="_value">Объект, который нужно представить</param>
        /// <param name="_propertyName">Имя свойства</param>
        /// <param name="_parentPath">Путь до свойства родителя</param>
        /// <param name="_formDataList">Список пар ключ - значение</param>
        /// <returns>True - если преобразование успешно и объект добавлен, false - если преобразование не удалось</returns>
        private static bool AddFormDataAsValueType(this object _value, string _propertyName, string _parentPath, ref string _url) {
            var valAsValueType = _value as ValueType;
            if (valAsValueType == null)
                return false;
            _propertyName = string.IsNullOrWhiteSpace(_parentPath) ? _propertyName : $"{_parentPath}.{_propertyName}";
            var valueType = _value.GetType();
            if (valueType.Name.Equals("DateTime")) {
                if (_url.Length == 0)
                    _url += $"{_propertyName}={_value}";
                else
                    _url += $"&{_propertyName}={_value}";
                return true;
            }
            var fields = valueType.GetFields(p_bindingMask);
            var properties = valueType.GetProperties(p_bindingMask);
            if (!valueType.IsPrimitive && !valueType.IsEnum && !valueType.Name.Equals("Decimal") && fields.Length != 0) {
                var fieldsEnumerator = fields.GetEnumerator();
                while (fieldsEnumerator.MoveNext()) {
                    var info = (FieldInfo)fieldsEnumerator.Current;
                    var fieldPath = $"{_propertyName}.{info.Name}";
                    _url += $"{(_url.Length == 0 ? "" : "&")}" + info.GetValue(_value).ToGetUrlQuery(null, fieldPath, true);
                }
            }
            if (!valueType.IsPrimitive && !valueType.IsEnum && !valueType.Name.Equals("Decimal") && properties.Length != 0) {
                var propertiesEnumerator = properties.GetEnumerator();
                while (propertiesEnumerator.MoveNext()) {
                    var propertyInfo = (PropertyInfo)propertiesEnumerator.Current;
                    var fieldPath = $"{_propertyName}.{propertyInfo.Name}";
                    _url += $"{(_url.Length == 0 ? "" : "&")}" + propertyInfo.GetValue(_value).ToGetUrlQuery(null, fieldPath, true);
                }
                return true;
            }
            if (fields.Length != 0 || properties.Length != 0)
                return false;

            if (_url.Length == 0)
                _url += $"{_propertyName}={_value}";
            else
                _url += $"&{_propertyName}={_value}";

            return true;
        }

        /// <summary>
        /// Добавить пару ключ - значение с значением типа строки
        /// </summary>
        /// <param name="_value">Объект, который нужно представить</param>
        /// <param name="_propertyName">Имя свойства</param>
        /// <param name="_parentPath">Путь до свойства родителя</param>
        /// <param name="_formDataList">Список пар ключ - значение</param>
        /// <returns>True - если преобразование успешно и объект добавлен, false - если преобразование не удалось</returns>
        private static bool AddFormDataAsString(this object _value, string _propertyName, string _parentPath, ref string _url) {
            var valAsString = _value as string;
            if (valAsString == null)
                return false;
            _propertyName = string.IsNullOrWhiteSpace(_parentPath) ? _propertyName : $"{_parentPath}.{_propertyName}";

            if (_url.Length == 0)
                _url += $"{_propertyName}={valAsString}";
            else
                _url += $"&{_propertyName}={valAsString}";

            return true;
        }
    }
}
