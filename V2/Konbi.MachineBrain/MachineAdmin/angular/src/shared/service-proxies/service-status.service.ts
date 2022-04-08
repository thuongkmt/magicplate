import { Injectable, Optional, Inject } from '@angular/core';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { HttpHeaders, HttpResponseBase, HttpResponse, HttpClient } from '@angular/common/http';
import { SwaggerException, ListResultDtoOfPermissionDto, API_BASE_URL } from './service-proxies';



@Injectable({
    providedIn: 'root'
})
export class ServiceStatusService {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    getAllServices(): Observable<ListResultDtoOfServiceStatus> {
        let url_ = this.baseUrl + "/api/services/app/ServiceStatus/GetAllServices";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetAllServiceStatus(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllServiceStatus(<any>response_);
                } catch (e) {
                    return <Observable<ListResultDtoOfServiceStatus>><any>Observable.throw(e);
                }
            } else
                return <Observable<ListResultDtoOfServiceStatus>><any>Observable.throw(response_);
        });
    }

    protected processGetAllServiceStatus(response: HttpResponseBase): Observable<ListResultDtoOfServiceStatus> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ListResultDtoOfServiceStatus.fromJS(resultData200) : new ListResultDtoOfServiceStatus();
                return Observable.of(result200);
            });
        } else if (status === 401) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Observable.of<ListResultDtoOfServiceStatus>(<any>null);
    }

    updateService(input: ServiceStatusDto | null | undefined): Observable<ServiceStatusDto> {
        let url_ = this.baseUrl + "/api/services/app/ServiceStatus/UpdateService";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(input);

        let options_: any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("put", url_, options_).flatMap((response_: any) => {
            return this.processUpdateServiceStatus(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateServiceStatus(<any>response_);
                } catch (e) {
                    return <Observable<ServiceStatusDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<ServiceStatusDto>><any>Observable.throw(response_);
        });
    }

    protected processUpdateServiceStatus(response: HttpResponseBase): Observable<ServiceStatusDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? resultData200 : false;
                return Observable.of(result200);
            });
        } else if (status === 401) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Observable.of<ServiceStatusDto>(<any>null);
    }

    getServiceStatus(id: number): Observable<ServiceStatusResultDto> {
        let url_ = this.baseUrl + "/api/services/app/ServiceStatus/GetServiceStatus?";
        if (id === undefined || id === null)
            throw new Error("The parameter 'id' must be defined and cannot be null.");
        else
            url_ += "Id=" + encodeURIComponent("" + id) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).flatMap((response_: any) => {
            return this.processGetServiceStatus(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetServiceStatus(<any>response_);
                } catch (e) {
                    return <Observable<ServiceStatusResultDto>><any>Observable.throw(e);
                }
            } else
                return <Observable<ServiceStatusResultDto>><any>Observable.throw(response_);
        });
    }

    protected processGetServiceStatus(response: HttpResponseBase): Observable<ServiceStatusResultDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? resultData200 : false;
                return Observable.of(result200);
            });
        } else if (status === 401) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status === 403) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("A server error occurred.", status, _responseText, _headers);
            });
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).flatMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            });
        }
        return Observable.of<ServiceStatusResultDto>(<any>null);
    }
}

export interface IListResultDtoOfServiceStatus {
    items: ServiceStatusDto[] | undefined;
}

export class ListResultDtoOfServiceStatus implements IListResultDtoOfServiceStatus {
    items!: ServiceStatusDto[] | undefined;

    constructor(data?: IListResultDtoOfServiceStatus) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(ServiceStatusDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfServiceStatus {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfServiceStatus();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.items && this.items.constructor === Array) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item.toJSON());
        }
        return data;
    }
}

export interface IServiceStatusDto {
    id: number;
    name: string;
    type: string;
    isArchived: boolean;
    status?: boolean;
    message: string;
}

export class ServiceStatusDto implements IServiceStatusDto {
    id: number;
    name: string;
    type: string;
    isArchived: boolean;
    status?: boolean;
    message: string;

    constructor(data?: IServiceStatusDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.id = data["id"];
            this.name = data["name"];
            this.type = data["type"];
            this.isArchived = data["isArchived"];
            this.status = data["status"];
            this.message = data["message"];
        }
    }

    static fromJS(data: any): ServiceStatusDto {
        data = typeof data === 'object' ? data : {};
        let result = new ServiceStatusDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["type"] = this.type;
        data["isArchived"] = this.isArchived;
        data["message"] = this.message;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new ServiceStatusDto();
        result.init(json);
        return result;
    }
}

export interface IServiceStatusResultDto {
    status: boolean;
    message: string;
}

export class ServiceStatusResultDto implements IServiceStatusResultDto {
    status: boolean;
    message: string;

    constructor(data?: IServiceStatusDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.status = data["status"];
            this.message = data["message"];
        }
    }

    static fromJS(data: any): ServiceStatusDto {
        data = typeof data === 'object' ? data : {};
        let result = new ServiceStatusDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["status"] = this.status;
        data["message"] = this.message;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new ServiceStatusDto();
        result.init(json);
        return result;
    }
}
function throwException(message: string, status: number, response: string, headers: { [key: string]: any; }, result?: any): Observable<any> {
    if (result !== null && result !== undefined)
        return _observableThrow(result);
    else
        return _observableThrow(new SwaggerException(message, status, response, headers, null));
}
function blobToText(blob: any): Observable<string> {
    return new Observable<string>((observer: any) => {
        if (!blob) {
            observer.next("");
            observer.complete();
        } else {
            let reader = new FileReader();
            reader.onload = function () {
                observer.next(this.result);
                observer.complete();
            }
            reader.readAsText(blob);
        }
    });
}
