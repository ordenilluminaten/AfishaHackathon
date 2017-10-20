class RequestDefaultOptions {
    constructor() {
        this.async = true;
        this.method = Request.method.get;
        this.headers = {
            'Accept': '*/*',
            'Content-Type': 'application/x-www-form-urlencoded',
            'X-Requested-With': 'XMLHttpRequest'
        };
        this.data = null;
    }
}
class Request {
    static get method() {
        return {
            'get': 'get',
            'post': 'post'
        };
    }
    static get(data) {
        data.method = Request.method.get;
        return Request.ajax(data);
    }
    static post(data) {
        data.method = Request.method.post;
        return Request.ajax(data);
    }
    static ajax(options) {
        return new Promise((resolve, reject) => {
            ObjectClass.merge(options, Request.defaultOptions);
            let url = options.url;
            let data = null;

            if (options.data != null) {
                switch (options.method.toLowerCase()) {
                    case Request.method.get: {
                        url += `?${Object.keys(options.data)
                            .map((i) => i + '=' + encodeURIComponent(options.data[i]))
                            .join('&')}`;
                        break;
                    }
                    case Request.method.post: {
                        if (options.data != null)
                            data = ObjectClass.toFormData(options.data);
                        break;
                    }
                }
            }

            var xhr = new XMLHttpRequest();

            xhr.open(options.method, url, options.async);

            for (let header in options.headers)
                xhr.setRequestHeader(header, options.headers[header]);

            xhr.onreadystatechange = () => {
                if (xhr.readyState !== 4)
                    return;

                if (xhr.status !== 200) {
                    toastr.danger(`Произошла ошибка при запросе ${xhr.statusText}`, 3000);
                    reject(`${xhr.status}: ${xhr.statusText}`);
                } else {
                    const responseType = xhr.getResponseHeader('Content-Type');
                    //accepts: {
                    //    "*": allTypes,
                    //        text: "text/plain",
                    //        html: "text/html",
                    //        xml: "application/xml, text/xml",
                    //        json: "application/json, text/javascript"
                    //},
                    //contents: {
                    //    xml: /\bxml\b/,
                    //        html: /\bhtml/,
                    //        json: /\bjson\b/
                    //},
                    if (new RegExp(/\bjson\b/).test(responseType)) {
                        let jsonRes = JSON.parse(xhr.responseText);
                        if (jsonRes.error != null) {
                            toastr[jsonRes.error.type](jsonRes.error.msg, jsonRes.error.ms);
                            reject(jsonRes);
                        } else resolve(jsonRes);
                    } else if (typeof xhr.response !== 'string') {
                        resolve(xhr.response);
                    } else {
                        resolve(xhr.responseText);
                    }
                }
            }
            try {
                xhr.send(data);
            } catch (e) {
                toastr.danger('Произошла ошибка при запросе', 3000);
                reject(e);
            }
        });
    }
}
Request.defaultOptions = new RequestDefaultOptions();