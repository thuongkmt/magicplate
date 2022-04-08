import { ProductDto, API_BASE_URL, SessionDto } from './service-proxies';
import { blobToText, throwException } from './service-base';
import { CreateOrEditPlateDto } from './plate-service-proxies';

import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import * as moment from 'moment';


@Injectable()
export class PlateMenuServiceProxy{
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }
    /**
     * @return Success
     */

    GetAllProductMenus(
        date: Date | null | undefined,
        session: string | null | undefined,
        name: string | null | undefined,
        code: string | null | undefined,
        category: string | null | undefined,
        skuFilter: string | null | undefined,
        sorting: string | null | undefined,
        maxResultCount: number | null | undefined,
        skipCount: number | null | undefined
    ): Observable<PagedResultDtoOfPlateMenuDto> {

        let url_ = this.baseUrl + '/api/services/app/ProductMenus/GetAllProductMenus?';
        if (date !== undefined) {
            url_ += 'DateFilter=' + encodeURIComponent('' + date) + '&';
        }
        if (session !== undefined) {
            url_ += 'SessionFilter=' + encodeURIComponent('' + session) + '&';
        }
        if (name !== undefined) {
            url_ += 'NameFilter=' + encodeURIComponent('' + name) + '&';
        }
        if (code !== undefined) {
            url_ += 'CodeFilter=' + encodeURIComponent('' + code) + '&';
        }
        if (category !== undefined) {
            url_ += 'CategoryFilter=' + encodeURIComponent('' + category) + '&';
        }
        if (skuFilter !== undefined) {
            url_ += 'SKUFilter=' + encodeURIComponent('' + skuFilter) + '&';
        }
        if (sorting !== undefined) {
            url_ += 'Sorting=' + encodeURIComponent('' + sorting) + '&';
        }
        if (maxResultCount !== undefined) {
            url_ += 'MaxResultCount=' + encodeURIComponent('' + maxResultCount) + '&';
        }
        if (skipCount !== undefined) {
            url_ += 'SkipCount=' + encodeURIComponent('' + skipCount) + '&';
        }
        url_ = url_.replace(/[?&]$/, '');

        let options_: any = {
            observe: 'response',
            responseType: 'blob',
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetAllProductMenus(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAllProductMenus(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfPlateMenuDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfPlateMenuDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllProductMenus(response: HttpResponseBase): Observable<PagedResultDtoOfPlateMenuDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfPlateMenuDto.fromJS(resultData200) : new PagedResultDtoOfPlateMenuDto();
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
        return Observable.of<PagedResultDtoOfPlateMenuDto>(<any>null);
    }

    updateProduct(input: PlateMenuInputDto): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdateProduct";
        url_ = url_.replace(/[?&]$/, "");
        var content_ = {};
        input.toJSON(content_);

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
            return this.processUpdateProduct(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateProduct(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }

    protected processUpdateProduct(response: HttpResponseBase): Observable<boolean> {
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
        return Observable.of<boolean>(<any>null);
    }

    updatePrice(input: PlateMenuInputDto): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdatePrice";
        url_ = url_.replace(/[?&]$/, "");
        var content_ = {};
        input.toJSON(content_);

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
            return this.processUpdatePrice(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdatePrice(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }
    protected processUpdatePrice(response: HttpResponseBase): Observable<boolean> {
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
        return Observable.of<boolean>(<any>null);
    }

    //
    updateDisplayOrder(input: PlateMenuInputDto): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdateDisplayOrder";
        url_ = url_.replace(/[?&]$/, "");
        var content_ = {};
        input.toJSON(content_);

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
            return this.processUpdateDisplayOrder(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateDisplayOrder(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }
    protected processUpdateDisplayOrder(response: HttpResponseBase): Observable<boolean> {
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
        return Observable.of<boolean>(<any>null);
    }
    //

    updatePriceStrategy(input: PlateMenuInputDto): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdatePriceStrategy";
        url_ = url_.replace(/[?&]$/, "");
        var content_ = {};
        input.toJSON(content_);

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
            return this.processUpdatePriceStrategy(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdatePriceStrategy(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }
    protected processUpdatePriceStrategy(response: HttpResponseBase): Observable<boolean> {
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
        return Observable.of<boolean>(<any>null);
    }

    importPlateMenu(input: ImportData[]): Observable<ImportResult> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/ImportPlateMenu";
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

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processImportPlateMenu(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processImportPlateMenu(<any>response_);
                } catch (e) {
                    return <Observable<ImportResult>><any>Observable.throw(e);
                }
            } else
                return <Observable<ImportResult>><any>Observable.throw(response_);
        });
    }
    protected processImportPlateMenu(response: HttpResponseBase): Observable<ImportResult> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ImportResult.fromJS(resultData200) : new ImportResult();
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
        return Observable.of<ImportResult>(<any>null);
    }

    replicateData(input: ReplicateInput): Observable<ImportResult> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/ReplicatePlateMenu";
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

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processReplicateData(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processImportPlateMenu(<any>response_);
                } catch (e) {
                    return <Observable<ImportResult>><any>Observable.throw(e);
                }
            } else
                return <Observable<ImportResult>><any>Observable.throw(response_);
        });
    }
    protected processReplicateData(response: HttpResponseBase): Observable<ImportResult> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).flatMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ImportResult.fromJS(resultData200) : new ImportResult();
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
        return Observable.of<ImportResult>(<any>null);
    }

    GenerateProductMenu(input: ReplicateInput): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GenerateProductMenu";
        url_ = url_.replace(/[?&]$/, "");
        var content_ = JSON.stringify(input);

        let options_: any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).flatMap((response_: any) => {
            return this.processGenerateProductMenu(response_);
        }).catch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGenerateProductMenu(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>Observable.throw(e);
                }
            } else
                return <Observable<boolean>><any>Observable.throw(response_);
        });
    }
    protected processGenerateProductMenu(response: HttpResponseBase): Observable<boolean> {
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
        return Observable.of<boolean>(<any>null);
    }

        //plate menu calendar
        GetPlateMenuByDay(selectedDate: String): Observable<PlateMenuDayResult[]> {

            let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetDayPlateMenusDetail?";
            url_ += "inputDate=" + encodeURIComponent("" + selectedDate);
            url_ = url_.replace(/[?&]$/, "");
    
            let options_: any = {
                observe: "response",
                responseType: "blob",
                headers: new HttpHeaders({
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                })
            };
    
            return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
                return this.processGetPlateMenuByDay(response_);
            })).pipe(_observableCatch((response_: any) => {
                if (response_ instanceof HttpResponseBase) {
                    try {
                        return this.processGetPlateMenuByDay(<any>response_);
                    } catch (e) {
                        return <Observable<PlateMenuDayResult[]>><any>_observableThrow(e);
                    }
                } else
                    return <Observable<PlateMenuDayResult[]>><any>_observableThrow(response_);
            }));
        }
    
        protected processGetPlateMenuByDay(response: HttpResponseBase): Observable<PlateMenuDayResult[]> {
            const status = response.status;
            const responseBlob =
                response instanceof HttpResponse ? response.body :
                    (<any>response).error instanceof Blob ? (<any>response).error : undefined;
    
            let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
            if (status === 200) {
                return blobToText(responseBlob).flatMap(_responseText => {
                    let result200: any = null;
                    let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                    result200 = resultData200 ? resultData200 : new Array<PlateMenuDayResult>();
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
            return Observable.of<PlateMenuDayResult[]>(<any>null);
        }
    
        GetWeekPlateMenus(): Observable<String[]> {
            let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetWeekPlateMenus";
            let options_: any = {
                observe: "response",
                responseType: "blob",
                headers: new HttpHeaders({
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                })
            };
            return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
                return this.processGetWeekPlateMenus(response_);
            })).pipe(_observableCatch((response_: any) => {
                if (response_ instanceof HttpResponseBase) {
                    try {
                        return this.processGetWeekPlateMenus(<any>response_);
                    } catch (e) {
                        return <Observable<String[]>><any>_observableThrow(e);
                    }
                } else
                    return <Observable<String[]>><any>_observableThrow(response_);
            }));
        }
    
        protected processGetWeekPlateMenus(response: HttpResponseBase): Observable<String[]> {
            const status = response.status;
            const responseBlob =
                response instanceof HttpResponse ? response.body :
                    (<any>response).error instanceof Blob ? (<any>response).error : undefined;
    
            let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
            if (status === 200) {
                return blobToText(responseBlob).flatMap(_responseText => {
                    let result200: any = null;
                    let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                    result200 = resultData200 ? resultData200 : new Observable<String[]>();
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
            return Observable.of<String[]>(<any>null);
        }
    
    
        GetMonthPlateMenus(selectedDate: String): Observable<PlateMenuMonthResult[]> {
    
            let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetMonthPlateMenus?";
            url_ += "inputDate=" + encodeURIComponent("" + selectedDate);
            url_ = url_.replace(/[?&]$/, "");
    
            let options_: any = {
                observe: "response",
                responseType: "blob",
                headers: new HttpHeaders({
                    "Content-Type": "application/json",
                    "Accept": "application/json"
                })
            };
    
            return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
                return this.processGetMonthPlateMenus(response_);
            })).pipe(_observableCatch((response_: any) => {
                if (response_ instanceof HttpResponseBase) {
                    try {
                        return this.processGetMonthPlateMenus(<any>response_);
                    } catch (e) {
                        return <Observable<PlateMenuMonthResult[]>><any>_observableThrow(e);
                    }
                } else
                    return <Observable<PlateMenuMonthResult[]>><any>_observableThrow(response_);
            }));
        }
    
        protected processGetMonthPlateMenus(response: HttpResponseBase): Observable<PlateMenuMonthResult[]> {
            const status = response.status;
            const responseBlob =
                response instanceof HttpResponse ? response.body :
                    (<any>response).error instanceof Blob ? (<any>response).error : undefined;
    
            let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
            if (status === 200) {
                return blobToText(responseBlob).flatMap(_responseText => {
                    let result200: any = null;
                    let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                    result200 = resultData200 ? resultData200 : new Array<PlateMenuMonthResult>();
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
            return Observable.of<PlateMenuMonthResult[]>(<any>null);
        }


}

export class PlateMenuDayResult {
    sessionId: string;
    sessionName: string;
    totalSetPrice: number;
    totalNoPrice: number;
    activeFlg: boolean;

    constructor(data?: PlateMenuDayResult) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.sessionId = data["sessionId"];
            this.sessionName = data["sessionName"];
            this.totalSetPrice = data["totalSetPrice"];
            this.totalNoPrice = data["totalNoPrice"];
            this.activeFlg = data["activeFlg"];
        }
    }

    static fromJS(data: any): PlateMenuDayResult {
        data = typeof data === 'object' ? data : {};
        let result = new PlateMenuDayResult();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sessionId"] = this.sessionId;
        data["sessionName"] = this.sessionName;
        data["totalSetPrice"] = this.totalSetPrice;
        data["totalNoPrice"] = this.totalNoPrice;
        data["activeFlg"] = this.activeFlg;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new PlateMenuDayResult();
        result.init(json);
        return result;
    }
}

export class PlateMenuMonthResult {
    day: string;
    totalSetPrice: number;
    totalNoPrice: number;

    constructor(data?: PlateMenuMonthResult) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.day = data["day"];
            this.totalSetPrice = data["totalSetPrice"];
            this.totalNoPrice = data["totalNoPrice"];
        }
    }

    static fromJS(data: any): PlateMenuMonthResult {
        data = typeof data === 'object' ? data : {};
        let result = new PlateMenuMonthResult();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["day"] = this.day;
        data["totalSetPrice"] = this.totalSetPrice;
        data["totalNoPrice"] = this.totalNoPrice;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new PlateMenuMonthResult();
        result.init(json);
        return result;
    }
}


export class PlateMenuDto{
    id: string;
    plateId: string;
    plate: CreateOrEditPlateDto;
    productId: string;
    product: ProductDto;
    price: number;
    session: SessionDto;
    selectedDate: Date;
    categoryName: string;
    priceStrategyId: string;
    priceStrategy: number;
    sessionName: string;
    plateCode: string;
    displayOrder: number | undefined;

    constructor(data?: PlateMenuDto) {
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
            this.plateId = data["plateId"];
            this.plate = data["plate"];
            this.productId = data["productId"];
            this.product = data["product"];
            this.price = data["price"];
            this.session = data["session"];
            this.selectedDate = data["selectedDate"];
            this.categoryName = data["categoryName"];
            this.priceStrategyId = data["priceStrategyId"];
            this.priceStrategy = data["priceStrategy"];
            this.sessionName = data["sessionName"];
            this.plateCode = data["plateCode"];
            this.displayOrder = data["displayOrder"];
        }
    }

    static fromJS(data: any): PlateMenuDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateMenuDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["plateId"] = this.plateId;
        data["plate"] = this.plate;
        data["productId"] = this.productId;
        data["product"] = this.product;
        data["price"] = this.price;
        data["session"] = this.session;
        data["selectedDate"] = this.selectedDate;
        data["categoryName"] = this.categoryName;
        data["priceStrategyId"] = this.priceStrategyId;
        data["priceStrategy"] = this.priceStrategy;
        data["sessionName"] = this.sessionName;
        data["plateCode"] = this.plateCode;
        data["displayOrder"] = this.displayOrder;
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new PlateMenuDto();
        result.init(json);
        return result;
    }
}

export class ImportData{
    selectedDate: string | undefined | null;
    price: string | undefined | null;
    contractorPrice: string | undefined | null;
    sessionName: string | undefined | null;
    plateCode: string | undefined | null;
    sku: string | undefined | null;
    barCode: string | undefined | null;
    productName: string | undefined | null;
}
export class ReplicateInput{
    dateFilter: Date | undefined | null;
    days: number;
}
export class PagedResultDtoOfPlateMenuDto {
    totalCount: number | undefined;
    items: PlateMenuDto[] | undefined;

    constructor(data?: PagedResultDtoOfPlateMenuDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.totalCount = data["totalCount"];
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(PlateMenuDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfPlateMenuDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfPlateMenuDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalCount"] = this.totalCount;
        if (this.items && this.items.constructor === Array) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item.toJSON());
        }
        return data;
    }

    clone() {
        const json = this.toJSON();
        let result = new PagedResultDtoOfPlateMenuDto();
        result.init(json);
        return result;
    }
}

export class PlateMenuInputDto {
    id: string;
    price: number;
    productId: string;
    priceStrategyId: string;
    priceStrategy: number;
    displayOrder: number | undefined;

    constructor(data?: PlateMenuInputDto) {
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
            this.productId = data["productId"];
            this.price = data["price"];
            this.priceStrategyId = data["priceStrategyId"];
            this.priceStrategy = data["priceStrategy"];
            this.displayOrder = data["displayOrder"];
        }
    }

    static fromJS(data: any): PlateMenuInputDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateMenuInputDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["productId"] = this.productId;
        data["price"] = this.price;
        data["priceStrategyId"] = this.priceStrategyId;
        data["priceStrategy"] = this.priceStrategy;
        data["displayOrder"] = this.displayOrder;
    }

    clone() {
        const json = this.toJSON();
        let result = new PlateMenuInputDto();
        result.init(json);
        return result;
    }
}

export class ImportResult{
    errorList: string;
    errorCount: number;
    successCount: number;

    constructor(data?: ImportResult) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.errorList = data["errorList"];
            this.errorCount = data["errorCount"];
            this.successCount = data["successCount"];
        }
    }

    static fromJS(data: any): ImportResult {
        data = typeof data === 'object' ? data : {};
        let result = new ImportResult();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["errorList"] = this.errorList;
        data["errorCount"] = this.errorCount;
        data["successCount"] = this.successCount;
    }

    clone() {
        const json = this.toJSON();
        let result = new ImportResult();
        result.init(json);
        return result;
    }
}

