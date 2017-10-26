using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Models.Extensions;

namespace Models.Extensions {
    public static class CastToFormDataExtension {

        private static readonly BindingFlags p_bindingMask = BindingFlags.Instance | BindingFlags.Public;
        /// <summary>
        /// Функция преобразования объекта в форм дату
        /// </summary>
        /// <param name="_object">Объект для преобразования</param>
        /// <param name="_firstObjectName"></param>
        /// <param name="_formDataList">Список пар ключ - значение</param>
        /// <param name="_parentPath">Путь относительно родительского свойства</param>
        /// <param name="_isField">Является ли объект полем структуры</param>
        /// <returns>Список спар ключ - значение полученный из объекта</returns>
        public static List<KeyValuePair<string, string>> ToFormData(this object _object, string _firstObjectName = "", List<KeyValuePair<string, string>> _formDataList = null, string _parentPath = "", bool _isField = false) {

            var mainObjectType = _object.GetType();
            var props = mainObjectType.GetProperties(p_bindingMask);
            var fields = mainObjectType.GetFields(p_bindingMask);

            if (props.Length == 0 && fields.Length == 0) {
                //Если тип по значению то приводим к строке
                if (_object.AddFormDataAsValueType(_parentPath, null, _formDataList))
                    return _formDataList;
                //Если тип строковый то явно приводим к строке
                if (_object.AddFormDataAsString(_parentPath, null, _formDataList))
                    return _formDataList;
            } else if (!_isField) {
                if (string.IsNullOrWhiteSpace(_parentPath)) {
                    _parentPath = string.IsNullOrEmpty(_firstObjectName) ? "" : _firstObjectName;
                } else {
                    if (!mainObjectType.IsAnonymousType()) {
                        _parentPath = $"{_parentPath}.{mainObjectType.Name}";
                    }
                }
            }

            if (_formDataList == null)
                _formDataList = new List<KeyValuePair<string, string>>(props.Length);
            else
                _formDataList.Capacity += props.Length;

            var propEnumerator = props.GetEnumerator();
            while (propEnumerator.MoveNext()) {
                var propertyInfo = (PropertyInfo)propEnumerator.Current;
                var type = propertyInfo.GetType();
                var value = propertyInfo.GetValue(_object);
                value.ProcessObjectToFormDataCast(type, _parentPath, propertyInfo.Name, _formDataList);
            }
            var fieldsEnumerator = fields.GetEnumerator();
            while (fieldsEnumerator.MoveNext()) {
                var fieldInfo = (FieldInfo)fieldsEnumerator.Current;
                var type = fieldInfo.GetType();
                var value = fieldInfo.GetValue(_object);
                value.ProcessObjectToFormDataCast(type, _parentPath, fieldInfo.Name, _formDataList);
            }
            return _formDataList;
        }

        private static void ProcessObjectToFormDataCast(this object _value, Type _type, string _parentPath, string _valueName, List<KeyValuePair<string, string>> _formDataList) {
            if (_value == null)
                return;
            //Если тип по значению то приводим к строке
            if (_value.AddFormDataAsValueType(_valueName, _parentPath, _formDataList))
                return;
            //Если тип строковый то явно приводим к строке
            if (_value.AddFormDataAsString(_valueName, _parentPath, _formDataList))
                return;
            var isParentPathNull = string.IsNullOrWhiteSpace(_parentPath);
            //Если тип словарь то для каждого элемента вызываем рекурсию
            var valAsIDictionary = _value as IDictionary;
            if (valAsIDictionary != null) {
                var valueEnumerator = valAsIDictionary.GetEnumerator();
                while (valueEnumerator.MoveNext()) {
                    var keyValue = (DictionaryEntry)valueEnumerator.Current;
                    var path = isParentPathNull ? $"{_valueName}[{keyValue.Key}]" : $"{_parentPath}.{_valueName}[{keyValue.Key}]";
                    keyValue.Value.ToFormData(null, _formDataList, path);
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
                    valueEnumerator.Current.ToFormData(null, _formDataList, path);
                    i++;
                }
                return;
            }
            //Если тип классы то вызываем рекурсию для класса
            _parentPath = isParentPathNull ? _valueName : $"{_parentPath}.{_valueName}";
            if (!_type.IsClass)
                throw new InvalidOperationException("can`t cast object to form data");
            _value.ToFormData(null, _formDataList, _parentPath);
        }
        /// <summary>
        /// Добавить пару ключ - значение с значением типа передаваемого по значению
        /// </summary>
        /// <param name="_value">Объект, который нужно представить</param>
        /// <param name="_propertyName">Имя свойства</param>
        /// <param name="_parentPath">Путь до свойства родителя</param>
        /// <param name="_formDataList">Список пар ключ - значение</param>
        /// <returns>True - если преобразование успешно и объект добавлен, false - если преобразование не удалось</returns>
        private static bool AddFormDataAsValueType(this object _value, string _propertyName, string _parentPath, List<KeyValuePair<string, string>> _formDataList) {
            var valAsValueType = _value as ValueType;
            if (valAsValueType == null)
                return false;
            _propertyName = string.IsNullOrWhiteSpace(_parentPath) ? _propertyName : $"{_parentPath}.{_propertyName}";
            var valueType = _value.GetType();
            if (valueType.Name.Equals("DateTime")) {
                _formDataList.Add(new KeyValuePair<string, string>(_propertyName, _value.ToString()));
                return true;
            }
            var fields = valueType.GetFields(p_bindingMask);
            var properties = valueType.GetProperties(p_bindingMask);
            if (!valueType.IsPrimitive && !valueType.IsEnum && !valueType.Name.Equals("Decimal") && fields.Length != 0) {
                var fieldsEnumerator = fields.GetEnumerator();
                while (fieldsEnumerator.MoveNext()) {
                    var info = (FieldInfo)fieldsEnumerator.Current;
                    var fieldPath = $"{_propertyName}.{info.Name}";
                    info.GetValue(_value).ToFormData(null, _formDataList, fieldPath, true);
                }
            }
            if (!valueType.IsPrimitive && !valueType.IsEnum && !valueType.Name.Equals("Decimal") && properties.Length != 0) {
                var propertiesEnumerator = properties.GetEnumerator();
                while (propertiesEnumerator.MoveNext()) {
                    var info = (PropertyInfo)propertiesEnumerator.Current;
                    var fieldPath = $"{_propertyName}.{info.Name}";
                    info.GetValue(_value).ToFormData(null, _formDataList, fieldPath, true);
                }
                return true;
            }
            if (fields.Length == 0 && properties.Length == 0) {
                _formDataList.Add(new KeyValuePair<string, string>(_propertyName, _value.ToString()));
                return true;
            }

            return false;
        }

        /// <summary>
        /// Добавить пару ключ - значение с значением типа строки
        /// </summary>
        /// <param name="_value">Объект, который нужно представить</param>
        /// <param name="_propertyName">Имя свойства</param>
        /// <param name="_parentPath">Путь до свойства родителя</param>
        /// <param name="_formDataList">Список пар ключ - значение</param>
        /// <returns>True - если преобразование успешно и объект добавлен, false - если преобразование не удалось</returns>
        private static bool AddFormDataAsString(this object _value, string _propertyName, string _parentPath, ICollection<KeyValuePair<string, string>> _formDataList) {
            var valAsString = _value as string;
            if (valAsString == null)
                return false;
            _propertyName = string.IsNullOrWhiteSpace(_parentPath) ? _propertyName : $"{_parentPath}.{_propertyName}";
            _formDataList.Add(new KeyValuePair<string, string>(_propertyName, valAsString));
            return true;
        }
        /*
                     var filter = new MovieApiFilter { Search = "test", Skip = 50 };
            Dictionary<int, int> dict2 = new Dictionary<int, int>() { { 2, 2 }, { 5, 33 } };
            filter.dict2 = dict2;
            filter.obj = new {
                c = 10,
                t = 10
            };
            var g = new g() {
                a = new {
                    c = 10
                },
                b = 9,
                filter = filter
            };
            Dictionary<int, int> dict = new Dictionary<int, int>() { { 1, 1 }, { 2, 2 } };

            var newOBject = new {
                filter = filter,
                list = new List<int> { 1, 2, 3, 4, 5 },
                objectsList = new List<object> { filter, filter, filter },
                text = "asdasdasdasdasd",
                test = 1,
                g,
                Decimal = decimal.Zero + 1,
                dict,
                enumObj = abc.b,
                dict2
            };
         */
    }
}