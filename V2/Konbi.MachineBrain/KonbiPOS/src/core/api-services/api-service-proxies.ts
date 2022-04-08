import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import * as moment from 'moment';
export const POS_API_BASE_URL = new InjectionToken<string>('POS_API_BASE_URL');

@Injectable()
export class CategoryServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(POS_API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param nameFilter (optional) 
     * @param descFilter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAll(filter: string | null | undefined, nameFilter: string | null | undefined, descFilter: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfGetCategoryForView> {
        let url_ = this.baseUrl + "/api/services/app/Category/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (descFilter !== undefined)
            url_ += "DescFilter=" + encodeURIComponent("" + descFilter) + "&";
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetAll(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetCategoryForView>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetCategoryForView>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfGetCategoryForView> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                const { result } = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                const result200 = result ? PagedResultDtoOfGetCategoryForView.fromJS(result) : new PagedResultDtoOfGetCategoryForView();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfGetCategoryForView>(<any>null);
    }
}

@Injectable()
export class ProductServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(POS_API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param nameFilter (optional) 
     * @param barcodeFilter (optional) 
     * @param sKUFilter (optional) 
     * @param categoryNameFilter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAll(filter: string | null | undefined, nameFilter: string | null | undefined, barcodeFilter: string | null | undefined, sKUFilter: string | null | undefined, categoryNameFilter: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfGetProductForView> {
        let url_ = this.baseUrl + "/api/services/app/Product/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (barcodeFilter !== undefined)
            url_ += "BarcodeFilter=" + encodeURIComponent("" + barcodeFilter) + "&";
        if (sKUFilter !== undefined)
            url_ += "SKUFilter=" + encodeURIComponent("" + sKUFilter) + "&";
        if (categoryNameFilter !== undefined)
            url_ += "CategoryNameFilter=" + encodeURIComponent("" + categoryNameFilter) + "&";
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetAll(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetProductForView>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetProductForView>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfGetProductForView> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? PagedResultDtoOfGetProductForView.fromJS(resultData200) : new PagedResultDtoOfGetProductForView();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfGetProductForView>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    getPlateForEdit(id: string | null | undefined): Observable<GetProductForEditOutput> {
        let url_ = this.baseUrl + "/api/services/app/Product/GetPlateForEdit?";
        if (id !== undefined)
            url_ += "Id=" + encodeURIComponent("" + id) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetPlateForEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetPlateForEdit(<any>response_);
                } catch (e) {
                    return <Observable<GetProductForEditOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<GetProductForEditOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetPlateForEdit(response: HttpResponseBase): Observable<GetProductForEditOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? GetProductForEditOutput.fromJS(resultData200) : new GetProductForEditOutput();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<GetProductForEditOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    createOrEdit(input: GetProductForEditOutput | null | undefined): Observable<ProductMessage> {
        let url_ = this.baseUrl + "/api/services/app/Product/CreateOrEdit";
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

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processCreateOrEdit(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processCreateOrEdit(<any>response_);
                } catch (e) {
                    return <Observable<ProductMessage>><any>_observableThrow(e);
                }
            } else
                return <Observable<ProductMessage>><any>_observableThrow(response_);
        }));
    }

    protected processCreateOrEdit(response: HttpResponseBase): Observable<ProductMessage> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ProductMessage.fromJS(resultData200) : new ProductMessage();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<ProductMessage>(<any>null);
    }

    /**
     * @param id (optional) 
     * @return Success
     */
    delete(id: string | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/Product/Delete?";
        if (id !== undefined)
            url_ += "Id=" + encodeURIComponent("" + id) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
            })
        };

        return this.http.request("delete", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processDelete(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processDelete(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processDelete(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return _observableOf<void>(<any>null);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updatePrice(input: ProductDto | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/Product/UpdatePrice";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processUpdatePrice(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdatePrice(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processUpdatePrice(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updateContractorPrice(input: ProductDto | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/Product/UpdateContractorPrice";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processUpdateContractorPrice(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateContractorPrice(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processUpdateContractorPrice(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updatePlate(input: ProductDto | null | undefined): Observable<string> {
        let url_ = this.baseUrl + "/api/services/app/Product/UpdatePlate";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processUpdatePlate(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdatePlate(<any>response_);
                } catch (e) {
                    return <Observable<string>><any>_observableThrow(e);
                }
            } else
                return <Observable<string>><any>_observableThrow(response_);
        }));
    }

    protected processUpdatePlate(response: HttpResponseBase): Observable<string> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<string>(<any>null);
    }
}

@Injectable()
export class ProductMenusServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(POS_API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param nameFilter (optional) 
     * @param codeFilter (optional) 
     * @param categoryFilter (optional) 
     * @param sKUFilter (optional) 
     * @param dateFilter (optional) 
     * @param sessionFilter (optional) 
     * @param id (optional) 
     * @param price (optional) 
     * @param priceStrategyId (optional) 
     * @param priceStrategy (optional) 
     * @param productId (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAllProductMenus(nameFilter: string | null | undefined, codeFilter: string | null | undefined, categoryFilter: string | null | undefined, sKUFilter: string | null | undefined, dateFilter: moment.Moment | null | undefined, sessionFilter: string | null | undefined, id: string | null | undefined, price: number | null | undefined, priceStrategyId: string | null | undefined, priceStrategy: number | null | undefined, productId: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfProductMenuDto> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetPOSProductMenuList?";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (codeFilter !== undefined)
            url_ += "CodeFilter=" + encodeURIComponent("" + codeFilter) + "&";
        if (categoryFilter !== undefined)
            url_ += "CategoryFilter=" + encodeURIComponent("" + categoryFilter) + "&";
        if (sKUFilter !== undefined)
            url_ += "SKUFilter=" + encodeURIComponent("" + sKUFilter) + "&";
        if (dateFilter !== undefined)
            url_ += "DateFilter=" + encodeURIComponent(dateFilter ? "" + dateFilter.toJSON() : "") + "&";
        if (sessionFilter !== undefined)
            url_ += "SessionFilter=" + encodeURIComponent("" + sessionFilter) + "&";
        if (id !== undefined)
            url_ += "Id=" + encodeURIComponent("" + id) + "&";
        if (price !== undefined)
            url_ += "Price=" + encodeURIComponent("" + price) + "&";
        if (priceStrategyId !== undefined)
            url_ += "PriceStrategyId=" + encodeURIComponent("" + priceStrategyId) + "&";
        if (priceStrategy !== undefined)
            url_ += "PriceStrategy=" + encodeURIComponent("" + priceStrategy) + "&";
        if (productId !== undefined)
            url_ += "ProductId=" + encodeURIComponent("" + productId) + "&";
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
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
                    return <Observable<PagedResultDtoOfProductMenuDto>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfProductMenuDto>><any>_observableThrow(response_);
        }));
    }

    protected processGetAllProductMenus(response: HttpResponseBase): Observable<PagedResultDtoOfProductMenuDto> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                const { result } = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                const result200 = result ? PagedResultDtoOfProductMenuDto.fromJS(result) : new PagedResultDtoOfProductMenuDto();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfProductMenuDto>(<any>null);
    }

    /**
     * @param categoryId (optional) 
     * @param sessionId (optional) 
     * @return Success
     */
    getPOSMenu(categoryId: string | null | undefined, sessionId: string | null | undefined): Observable<ListResultDtoOfPosProductMenuOutput> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetPOSMenu?";
        if (categoryId !== undefined)
            url_ += "CategoryId=" + encodeURIComponent("" + categoryId) + "&";
        if (sessionId !== undefined)
            url_ += "SessionId=" + encodeURIComponent("" + sessionId) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetPOSMenu(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetPOSMenu(<any>response_);
                } catch (e) {
                    return <Observable<ListResultDtoOfPosProductMenuOutput>><any>_observableThrow(e);
                }
            } else
                return <Observable<ListResultDtoOfPosProductMenuOutput>><any>_observableThrow(response_);
        }));
    }

    protected processGetPOSMenu(response: HttpResponseBase): Observable<ListResultDtoOfPosProductMenuOutput> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                const { result } = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                const result200 = result ? ListResultDtoOfPosProductMenuOutput.fromJS(result) : new ListResultDtoOfPosProductMenuOutput();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<ListResultDtoOfPosProductMenuOutput>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    generateProductMenu(input: ReplicateInput | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GenerateProductMenu";
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

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGenerateProductMenu(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGenerateProductMenu(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processGenerateProductMenu(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updatePrice(input: ProductMenusInput | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdatePrice";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processUpdatePrice(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdatePrice(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processUpdatePrice(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updateProduct(input: ProductMenusInput | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdateProduct";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processUpdateProduct(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdateProduct(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processUpdateProduct(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    updatePriceStrategy(input: ProductMenusInput | null | undefined): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/UpdatePriceStrategy";
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

        return this.http.request("put", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processUpdatePriceStrategy(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processUpdatePriceStrategy(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processUpdatePriceStrategy(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    importProductMenu(input: ImportData[] | null | undefined): Observable<ImportResult> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/ImportProductMenu";
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

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processImportProductMenu(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processImportProductMenu(<any>response_);
                } catch (e) {
                    return <Observable<ImportResult>><any>_observableThrow(e);
                }
            } else
                return <Observable<ImportResult>><any>_observableThrow(response_);
        }));
    }

    protected processImportProductMenu(response: HttpResponseBase): Observable<ImportResult> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 ? ImportResult.fromJS(resultData200) : new ImportResult();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<ImportResult>(<any>null);
    }

    /**
     * @return Success
     */
    syncProductMenuData(): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/SyncProductMenuData";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processSyncProductMenuData(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processSyncProductMenuData(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processSyncProductMenuData(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }

    /**
     * @param input (optional) 
     * @return Success
     */
    replicateProductMenu(input: ReplicateInput | null | undefined): Observable<void> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/ReplicateProductMenu";
        url_ = url_.replace(/[?&]$/, "");

        const content_ = JSON.stringify(input);

        let options_: any = {
            body: content_,
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Content-Type": "application/json",
            })
        };

        return this.http.request("post", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processReplicateProductMenu(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processReplicateProductMenu(<any>response_);
                } catch (e) {
                    return <Observable<void>><any>_observableThrow(e);
                }
            } else
                return <Observable<void>><any>_observableThrow(response_);
        }));
    }

    protected processReplicateProductMenu(response: HttpResponseBase): Observable<void> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return _observableOf<void>(<any>null);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<void>(<any>null);
    }

    /**
     * @param inputDate (optional) 
     * @return Success
     */
    getDayPlateMenusDetail(inputDate: moment.Moment | null | undefined): Observable<PlateMenuDayResult[]> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetDayPlateMenusDetail?";
        if (inputDate !== undefined)
            url_ += "inputDate=" + encodeURIComponent(inputDate ? "" + inputDate.toJSON() : "") + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetDayPlateMenusDetail(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetDayPlateMenusDetail(<any>response_);
                } catch (e) {
                    return <Observable<PlateMenuDayResult[]>><any>_observableThrow(e);
                }
            } else
                return <Observable<PlateMenuDayResult[]>><any>_observableThrow(response_);
        }));
    }

    protected processGetDayPlateMenusDetail(response: HttpResponseBase): Observable<PlateMenuDayResult[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                if (resultData200 && resultData200.constructor === Array) {
                    result200 = [];
                    for (let item of resultData200)
                        result200.push(PlateMenuDayResult.fromJS(item));
                }
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PlateMenuDayResult[]>(<any>null);
    }

    /**
     * @return Success
     */
    getWeekPlateMenus(): Observable<string[]> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetWeekPlateMenus";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
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
                    return <Observable<string[]>><any>_observableThrow(e);
                }
            } else
                return <Observable<string[]>><any>_observableThrow(response_);
        }));
    }

    protected processGetWeekPlateMenus(response: HttpResponseBase): Observable<string[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                if (resultData200 && resultData200.constructor === Array) {
                    result200 = [];
                    for (let item of resultData200)
                        result200.push(item);
                }
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<string[]>(<any>null);
    }

    /**
     * @param inputDate (optional) 
     * @return Success
     */
    getMonthPlateMenus(inputDate: moment.Moment | null | undefined): Observable<ProductMenuMonthResult[]> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetMonthPlateMenus?";
        if (inputDate !== undefined)
            url_ += "inputDate=" + encodeURIComponent(inputDate ? "" + inputDate.toJSON() : "") + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
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
                    return <Observable<ProductMenuMonthResult[]>><any>_observableThrow(e);
                }
            } else
                return <Observable<ProductMenuMonthResult[]>><any>_observableThrow(response_);
        }));
    }

    protected processGetMonthPlateMenus(response: HttpResponseBase): Observable<ProductMenuMonthResult[]> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                if (resultData200 && resultData200.constructor === Array) {
                    result200 = [];
                    for (let item of resultData200)
                        result200.push(ProductMenuMonthResult.fromJS(item));
                }
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<ProductMenuMonthResult[]>(<any>null);
    }

    /**
     * @return Success
     */
    getPlateMenuJob(): Observable<boolean> {
        let url_ = this.baseUrl + "/api/services/app/ProductMenus/GetPlateMenuJob";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetPlateMenuJob(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetPlateMenuJob(<any>response_);
                } catch (e) {
                    return <Observable<boolean>><any>_observableThrow(e);
                }
            } else
                return <Observable<boolean>><any>_observableThrow(response_);
        }));
    }

    protected processGetPlateMenuJob(response: HttpResponseBase): Observable<boolean> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                let result200: any = null;
                let resultData200 = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                result200 = resultData200 !== undefined ? resultData200 : <any>null;
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<boolean>(<any>null);
    }
}

@Injectable()
export class SessionsServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(POS_API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }

    /**
     * @param filter (optional) 
     * @param nameFilter (optional) 
     * @param fromHrsFilter (optional) 
     * @param toHrsFilter (optional) 
     * @param sorting (optional) 
     * @param skipCount (optional) 
     * @param maxResultCount (optional) 
     * @return Success
     */
    getAll(filter: string | null | undefined, nameFilter: string | null | undefined, fromHrsFilter: string | null | undefined, toHrsFilter: string | null | undefined, activeFilter: string | null | undefined, sorting: string | null | undefined, skipCount: number | null | undefined, maxResultCount: number | null | undefined): Observable<PagedResultDtoOfGetSessionForView> {
        let url_ = this.baseUrl + "/api/services/app/Sessions/GetAll?";
        if (filter !== undefined)
            url_ += "Filter=" + encodeURIComponent("" + filter) + "&";
        if (nameFilter !== undefined)
            url_ += "NameFilter=" + encodeURIComponent("" + nameFilter) + "&";
        if (fromHrsFilter !== undefined)
            url_ += "FromHrsFilter=" + encodeURIComponent("" + fromHrsFilter) + "&";
        if (toHrsFilter !== undefined)
            url_ += "ToHrsFilter=" + encodeURIComponent("" + toHrsFilter) + "&";
        if (activeFilter !== undefined)
            url_ += "ActiveFlgFilter=" + encodeURIComponent("" + activeFilter) + "&";
        if (sorting !== undefined)
            url_ += "Sorting=" + encodeURIComponent("" + sorting) + "&";
        if (skipCount !== undefined)
            url_ += "SkipCount=" + encodeURIComponent("" + skipCount) + "&";
        if (maxResultCount !== undefined)
            url_ += "MaxResultCount=" + encodeURIComponent("" + maxResultCount) + "&";
        url_ = url_.replace(/[?&]$/, "");

        let options_: any = {
            observe: "response",
            responseType: "blob",
            headers: new HttpHeaders({
                "Accept": "application/json"
            })
        };

        return this.http.request("get", url_, options_).pipe(_observableMergeMap((response_: any) => {
            return this.processGetAll(response_);
        })).pipe(_observableCatch((response_: any) => {
            if (response_ instanceof HttpResponseBase) {
                try {
                    return this.processGetAll(<any>response_);
                } catch (e) {
                    return <Observable<PagedResultDtoOfGetSessionForView>><any>_observableThrow(e);
                }
            } else
                return <Observable<PagedResultDtoOfGetSessionForView>><any>_observableThrow(response_);
        }));
    }

    protected processGetAll(response: HttpResponseBase): Observable<PagedResultDtoOfGetSessionForView> {
        const status = response.status;
        const responseBlob =
            response instanceof HttpResponse ? response.body :
                (<any>response).error instanceof Blob ? (<any>response).error : undefined;

        let _headers: any = {}; if (response.headers) { for (let key of response.headers.keys()) { _headers[key] = response.headers.get(key); } };
        if (status === 200) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                const { result } = _responseText === "" ? null : JSON.parse(_responseText, this.jsonParseReviver);
                const result200 = result ? PagedResultDtoOfGetSessionForView.fromJS(result) : new PagedResultDtoOfGetSessionForView();
                return _observableOf(result200);
            }));
        } else if (status !== 200 && status !== 204) {
            return blobToText(responseBlob).pipe(_observableMergeMap(_responseText => {
                return throwException("An unexpected server error occurred.", status, _responseText, _headers);
            }));
        }
        return _observableOf<PagedResultDtoOfGetSessionForView>(<any>null);
    }
}

export class IsTenantAvailableInput implements IIsTenantAvailableInput {
    tenancyName!: string;

    constructor(data?: IIsTenantAvailableInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
        }
    }

    static fromJS(data: any): IsTenantAvailableInput {
        data = typeof data === 'object' ? data : {};
        let result = new IsTenantAvailableInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        return data;
    }
}

export interface IIsTenantAvailableInput {
    tenancyName: string;
}

export class IsTenantAvailableOutput implements IIsTenantAvailableOutput {
    state!: IsTenantAvailableOutputState | undefined;
    tenantId!: number | undefined;
    serverRootAddress!: string | undefined;

    constructor(data?: IIsTenantAvailableOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.state = data["state"];
            this.tenantId = data["tenantId"];
            this.serverRootAddress = data["serverRootAddress"];
        }
    }

    static fromJS(data: any): IsTenantAvailableOutput {
        data = typeof data === 'object' ? data : {};
        let result = new IsTenantAvailableOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["state"] = this.state;
        data["tenantId"] = this.tenantId;
        data["serverRootAddress"] = this.serverRootAddress;
        return data;
    }
}

export interface IIsTenantAvailableOutput {
    state: IsTenantAvailableOutputState | undefined;
    tenantId: number | undefined;
    serverRootAddress: string | undefined;
}

export class ResolveTenantIdInput implements IResolveTenantIdInput {
    c!: string | undefined;

    constructor(data?: IResolveTenantIdInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.c = data["c"];
        }
    }

    static fromJS(data: any): ResolveTenantIdInput {
        data = typeof data === 'object' ? data : {};
        let result = new ResolveTenantIdInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["c"] = this.c;
        return data;
    }
}

export interface IResolveTenantIdInput {
    c: string | undefined;
}

export class RegisterInput implements IRegisterInput {
    name!: string;
    surname!: string;
    userName!: string;
    emailAddress!: string;
    password!: string;
    captchaResponse!: string | undefined;

    constructor(data?: IRegisterInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.surname = data["surname"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.password = data["password"];
            this.captchaResponse = data["captchaResponse"];
        }
    }

    static fromJS(data: any): RegisterInput {
        data = typeof data === 'object' ? data : {};
        let result = new RegisterInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["password"] = this.password;
        data["captchaResponse"] = this.captchaResponse;
        return data;
    }
}

export interface IRegisterInput {
    name: string;
    surname: string;
    userName: string;
    emailAddress: string;
    password: string;
    captchaResponse: string | undefined;
}

export class RegisterOutput implements IRegisterOutput {
    canLogin!: boolean | undefined;

    constructor(data?: IRegisterOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.canLogin = data["canLogin"];
        }
    }

    static fromJS(data: any): RegisterOutput {
        data = typeof data === 'object' ? data : {};
        let result = new RegisterOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["canLogin"] = this.canLogin;
        return data;
    }
}

export interface IRegisterOutput {
    canLogin: boolean | undefined;
}

export class SendPasswordResetCodeInput implements ISendPasswordResetCodeInput {
    emailAddress!: string;

    constructor(data?: ISendPasswordResetCodeInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.emailAddress = data["emailAddress"];
        }
    }

    static fromJS(data: any): SendPasswordResetCodeInput {
        data = typeof data === 'object' ? data : {};
        let result = new SendPasswordResetCodeInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["emailAddress"] = this.emailAddress;
        return data;
    }
}

export interface ISendPasswordResetCodeInput {
    emailAddress: string;
}

export class ResetPasswordInput implements IResetPasswordInput {
    userId!: number | undefined;
    resetCode!: string | undefined;
    password!: string | undefined;
    returnUrl!: string | undefined;
    singleSignIn!: string | undefined;
    c!: string | undefined;

    constructor(data?: IResetPasswordInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.resetCode = data["resetCode"];
            this.password = data["password"];
            this.returnUrl = data["returnUrl"];
            this.singleSignIn = data["singleSignIn"];
            this.c = data["c"];
        }
    }

    static fromJS(data: any): ResetPasswordInput {
        data = typeof data === 'object' ? data : {};
        let result = new ResetPasswordInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["resetCode"] = this.resetCode;
        data["password"] = this.password;
        data["returnUrl"] = this.returnUrl;
        data["singleSignIn"] = this.singleSignIn;
        data["c"] = this.c;
        return data;
    }
}

export interface IResetPasswordInput {
    userId: number | undefined;
    resetCode: string | undefined;
    password: string | undefined;
    returnUrl: string | undefined;
    singleSignIn: string | undefined;
    c: string | undefined;
}

export class ResetPasswordOutput implements IResetPasswordOutput {
    canLogin!: boolean | undefined;
    userName!: string | undefined;

    constructor(data?: IResetPasswordOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.canLogin = data["canLogin"];
            this.userName = data["userName"];
        }
    }

    static fromJS(data: any): ResetPasswordOutput {
        data = typeof data === 'object' ? data : {};
        let result = new ResetPasswordOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["canLogin"] = this.canLogin;
        data["userName"] = this.userName;
        return data;
    }
}

export interface IResetPasswordOutput {
    canLogin: boolean | undefined;
    userName: string | undefined;
}

export class SendEmailActivationLinkInput implements ISendEmailActivationLinkInput {
    emailAddress!: string;

    constructor(data?: ISendEmailActivationLinkInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.emailAddress = data["emailAddress"];
        }
    }

    static fromJS(data: any): SendEmailActivationLinkInput {
        data = typeof data === 'object' ? data : {};
        let result = new SendEmailActivationLinkInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["emailAddress"] = this.emailAddress;
        return data;
    }
}

export interface ISendEmailActivationLinkInput {
    emailAddress: string;
}

export class ActivateEmailInput implements IActivateEmailInput {
    userId!: number | undefined;
    confirmationCode!: string | undefined;
    c!: string | undefined;

    constructor(data?: IActivateEmailInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.confirmationCode = data["confirmationCode"];
            this.c = data["c"];
        }
    }

    static fromJS(data: any): ActivateEmailInput {
        data = typeof data === 'object' ? data : {};
        let result = new ActivateEmailInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["confirmationCode"] = this.confirmationCode;
        data["c"] = this.c;
        return data;
    }
}

export interface IActivateEmailInput {
    userId: number | undefined;
    confirmationCode: string | undefined;
    c: string | undefined;
}

export class ImpersonateInput implements IImpersonateInput {
    tenantId!: number | undefined;
    userId!: number | undefined;

    constructor(data?: IImpersonateInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.userId = data["userId"];
        }
    }

    static fromJS(data: any): ImpersonateInput {
        data = typeof data === 'object' ? data : {};
        let result = new ImpersonateInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["userId"] = this.userId;
        return data;
    }
}

export interface IImpersonateInput {
    tenantId: number | undefined;
    userId: number | undefined;
}

export class ImpersonateOutput implements IImpersonateOutput {
    impersonationToken!: string | undefined;
    tenancyName!: string | undefined;

    constructor(data?: IImpersonateOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.impersonationToken = data["impersonationToken"];
            this.tenancyName = data["tenancyName"];
        }
    }

    static fromJS(data: any): ImpersonateOutput {
        data = typeof data === 'object' ? data : {};
        let result = new ImpersonateOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["impersonationToken"] = this.impersonationToken;
        data["tenancyName"] = this.tenancyName;
        return data;
    }
}

export interface IImpersonateOutput {
    impersonationToken: string | undefined;
    tenancyName: string | undefined;
}

export class SwitchToLinkedAccountInput implements ISwitchToLinkedAccountInput {
    targetTenantId!: number | undefined;
    targetUserId!: number | undefined;

    constructor(data?: ISwitchToLinkedAccountInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.targetTenantId = data["targetTenantId"];
            this.targetUserId = data["targetUserId"];
        }
    }

    static fromJS(data: any): SwitchToLinkedAccountInput {
        data = typeof data === 'object' ? data : {};
        let result = new SwitchToLinkedAccountInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["targetTenantId"] = this.targetTenantId;
        data["targetUserId"] = this.targetUserId;
        return data;
    }
}

export interface ISwitchToLinkedAccountInput {
    targetTenantId: number | undefined;
    targetUserId: number | undefined;
}

export class SwitchToLinkedAccountOutput implements ISwitchToLinkedAccountOutput {
    switchAccountToken!: string | undefined;
    tenancyName!: string | undefined;

    constructor(data?: ISwitchToLinkedAccountOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.switchAccountToken = data["switchAccountToken"];
            this.tenancyName = data["tenancyName"];
        }
    }

    static fromJS(data: any): SwitchToLinkedAccountOutput {
        data = typeof data === 'object' ? data : {};
        let result = new SwitchToLinkedAccountOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["switchAccountToken"] = this.switchAccountToken;
        data["tenancyName"] = this.tenancyName;
        return data;
    }
}

export interface ISwitchToLinkedAccountOutput {
    switchAccountToken: string | undefined;
    tenancyName: string | undefined;
}

export class PagedResultDtoOfAuditLogListDto implements IPagedResultDtoOfAuditLogListDto {
    totalCount!: number | undefined;
    items!: AuditLogListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfAuditLogListDto) {
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
                    this.items.push(AuditLogListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfAuditLogListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfAuditLogListDto();
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
}

export interface IPagedResultDtoOfAuditLogListDto {
    totalCount: number | undefined;
    items: AuditLogListDto[] | undefined;
}

export class AuditLogListDto implements IAuditLogListDto {
    userId!: number | undefined;
    userName!: string | undefined;
    impersonatorTenantId!: number | undefined;
    impersonatorUserId!: number | undefined;
    serviceName!: string | undefined;
    methodName!: string | undefined;
    parameters!: string | undefined;
    executionTime!: moment.Moment | undefined;
    executionDuration!: number | undefined;
    clientIpAddress!: string | undefined;
    clientName!: string | undefined;
    browserInfo!: string | undefined;
    exception!: string | undefined;
    customData!: string | undefined;
    id!: number | undefined;

    constructor(data?: IAuditLogListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.userName = data["userName"];
            this.impersonatorTenantId = data["impersonatorTenantId"];
            this.impersonatorUserId = data["impersonatorUserId"];
            this.serviceName = data["serviceName"];
            this.methodName = data["methodName"];
            this.parameters = data["parameters"];
            this.executionTime = data["executionTime"] ? moment(data["executionTime"].toString()) : <any>undefined;
            this.executionDuration = data["executionDuration"];
            this.clientIpAddress = data["clientIpAddress"];
            this.clientName = data["clientName"];
            this.browserInfo = data["browserInfo"];
            this.exception = data["exception"];
            this.customData = data["customData"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): AuditLogListDto {
        data = typeof data === 'object' ? data : {};
        let result = new AuditLogListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["userName"] = this.userName;
        data["impersonatorTenantId"] = this.impersonatorTenantId;
        data["impersonatorUserId"] = this.impersonatorUserId;
        data["serviceName"] = this.serviceName;
        data["methodName"] = this.methodName;
        data["parameters"] = this.parameters;
        data["executionTime"] = this.executionTime ? this.executionTime.toISOString() : <any>undefined;
        data["executionDuration"] = this.executionDuration;
        data["clientIpAddress"] = this.clientIpAddress;
        data["clientName"] = this.clientName;
        data["browserInfo"] = this.browserInfo;
        data["exception"] = this.exception;
        data["customData"] = this.customData;
        data["id"] = this.id;
        return data;
    }
}

export interface IAuditLogListDto {
    userId: number | undefined;
    userName: string | undefined;
    impersonatorTenantId: number | undefined;
    impersonatorUserId: number | undefined;
    serviceName: string | undefined;
    methodName: string | undefined;
    parameters: string | undefined;
    executionTime: moment.Moment | undefined;
    executionDuration: number | undefined;
    clientIpAddress: string | undefined;
    clientName: string | undefined;
    browserInfo: string | undefined;
    exception: string | undefined;
    customData: string | undefined;
    id: number | undefined;
}

export class FileDto implements IFileDto {
    fileName!: string;
    fileType!: string | undefined;
    fileToken!: string;

    constructor(data?: IFileDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.fileName = data["fileName"];
            this.fileType = data["fileType"];
            this.fileToken = data["fileToken"];
        }
    }

    static fromJS(data: any): FileDto {
        data = typeof data === 'object' ? data : {};
        let result = new FileDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["fileName"] = this.fileName;
        data["fileType"] = this.fileType;
        data["fileToken"] = this.fileToken;
        return data;
    }
}

export interface IFileDto {
    fileName: string;
    fileType: string | undefined;
    fileToken: string;
}

export class NameValueDto implements INameValueDto {
    name!: string | undefined;
    value!: string | undefined;

    constructor(data?: INameValueDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): NameValueDto {
        data = typeof data === 'object' ? data : {};
        let result = new NameValueDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["value"] = this.value;
        return data;
    }
}

export interface INameValueDto {
    name: string | undefined;
    value: string | undefined;
}

export class PagedResultDtoOfEntityChangeListDto implements IPagedResultDtoOfEntityChangeListDto {
    totalCount!: number | undefined;
    items!: EntityChangeListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfEntityChangeListDto) {
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
                    this.items.push(EntityChangeListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfEntityChangeListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfEntityChangeListDto();
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
}

export interface IPagedResultDtoOfEntityChangeListDto {
    totalCount: number | undefined;
    items: EntityChangeListDto[] | undefined;
}

export class EntityChangeListDto implements IEntityChangeListDto {
    userId!: number | undefined;
    userName!: string | undefined;
    changeTime!: moment.Moment | undefined;
    entityTypeFullName!: string | undefined;
    changeType!: EntityChangeListDtoChangeType | undefined;
    changeTypeName!: string | undefined;
    entityChangeSetId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IEntityChangeListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.userName = data["userName"];
            this.changeTime = data["changeTime"] ? moment(data["changeTime"].toString()) : <any>undefined;
            this.entityTypeFullName = data["entityTypeFullName"];
            this.changeType = data["changeType"];
            this.changeTypeName = data["changeTypeName"];
            this.entityChangeSetId = data["entityChangeSetId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): EntityChangeListDto {
        data = typeof data === 'object' ? data : {};
        let result = new EntityChangeListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["userName"] = this.userName;
        data["changeTime"] = this.changeTime ? this.changeTime.toISOString() : <any>undefined;
        data["entityTypeFullName"] = this.entityTypeFullName;
        data["changeType"] = this.changeType;
        data["changeTypeName"] = this.changeTypeName;
        data["entityChangeSetId"] = this.entityChangeSetId;
        data["id"] = this.id;
        return data;
    }
}

export interface IEntityChangeListDto {
    userId: number | undefined;
    userName: string | undefined;
    changeTime: moment.Moment | undefined;
    entityTypeFullName: string | undefined;
    changeType: EntityChangeListDtoChangeType | undefined;
    changeTypeName: string | undefined;
    entityChangeSetId: number | undefined;
    id: number | undefined;
}

export class EntityPropertyChangeDto implements IEntityPropertyChangeDto {
    entityChangeId!: number | undefined;
    newValue!: string | undefined;
    originalValue!: string | undefined;
    propertyName!: string | undefined;
    propertyTypeFullName!: string | undefined;
    tenantId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IEntityPropertyChangeDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.entityChangeId = data["entityChangeId"];
            this.newValue = data["newValue"];
            this.originalValue = data["originalValue"];
            this.propertyName = data["propertyName"];
            this.propertyTypeFullName = data["propertyTypeFullName"];
            this.tenantId = data["tenantId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): EntityPropertyChangeDto {
        data = typeof data === 'object' ? data : {};
        let result = new EntityPropertyChangeDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["entityChangeId"] = this.entityChangeId;
        data["newValue"] = this.newValue;
        data["originalValue"] = this.originalValue;
        data["propertyName"] = this.propertyName;
        data["propertyTypeFullName"] = this.propertyTypeFullName;
        data["tenantId"] = this.tenantId;
        data["id"] = this.id;
        return data;
    }
}

export interface IEntityPropertyChangeDto {
    entityChangeId: number | undefined;
    newValue: string | undefined;
    originalValue: string | undefined;
    propertyName: string | undefined;
    propertyTypeFullName: string | undefined;
    tenantId: number | undefined;
    id: number | undefined;
}

export class ListResultDtoOfCacheDto implements IListResultDtoOfCacheDto {
    items!: CacheDto[] | undefined;

    constructor(data?: IListResultDtoOfCacheDto) {
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
                    this.items.push(CacheDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfCacheDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfCacheDto();
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

export interface IListResultDtoOfCacheDto {
    items: CacheDto[] | undefined;
}

export class CacheDto implements ICacheDto {
    name!: string | undefined;

    constructor(data?: ICacheDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
        }
    }

    static fromJS(data: any): CacheDto {
        data = typeof data === 'object' ? data : {};
        let result = new CacheDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        return data;
    }
}

export interface ICacheDto {
    name: string | undefined;
}

export class EntityDtoOfString implements IEntityDtoOfString {
    id!: string | undefined;

    constructor(data?: IEntityDtoOfString) {
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
        }
    }

    static fromJS(data: any): EntityDtoOfString {
        data = typeof data === 'object' ? data : {};
        let result = new EntityDtoOfString();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        return data;
    }
}

export interface IEntityDtoOfString {
    id: string | undefined;
}

export class PagedResultDtoOfGetCategoryForView implements IPagedResultDtoOfGetCategoryForView {
    totalCount!: number | undefined;
    items!: GetCategoryForView[] | undefined;

    constructor(data?: IPagedResultDtoOfGetCategoryForView) {
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
                    this.items.push(GetCategoryForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetCategoryForView {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetCategoryForView();
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
}

export interface IPagedResultDtoOfGetCategoryForView {
    totalCount: number | undefined;
    items: GetCategoryForView[] | undefined;
}

export class GetCategoryForView implements IGetCategoryForView {
    category!: CategoryDto | undefined;

    constructor(data?: IGetCategoryForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.category = data["category"] ? CategoryDto.fromJS(data["category"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetCategoryForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetCategoryForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["category"] = this.category ? this.category.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetCategoryForView {
    category: CategoryDto | undefined;
}

export class CategoryDto implements ICategoryDto {
    name!: string | undefined;
    description!: string | undefined;
    imageUrl!: string | undefined;
    products!: ProductDto[] | undefined;
    id!: string | undefined;

    constructor(data?: ICategoryDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.description = data["description"];
            this.imageUrl = data["imageUrl"];
            if (data["products"] && data["products"].constructor === Array) {
                this.products = [];
                for (let item of data["products"])
                    this.products.push(ProductDto.fromJS(item));
            }
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["description"] = this.description;
        data["imageUrl"] = this.imageUrl;
        if (this.products && this.products.constructor === Array) {
            data["products"] = [];
            for (let item of this.products)
                data["products"].push(item.toJSON());
        }
        data["id"] = this.id;
        return data;
    }
}

export interface ICategoryDto {
    name: string | undefined;
    description: string | undefined;
    imageUrl: string | undefined;
    products: ProductDto[] | undefined;
    id: string | undefined;
}

export class ProductDto implements IProductDto {
    sku!: string | undefined;
    name!: string | undefined;
    status!: number | undefined;
    unit!: string | undefined;
    price!: number | undefined;
    contractorPrice!: number | undefined;
    categoryId!: string | undefined;
    plateId!: string | undefined;
    imageUrl!: string | undefined;
    categories!: string[] | undefined;
    tenantId!: number | undefined;
    barcode!: string | undefined;
    fromDate!: moment.Moment | undefined;
    toDate!: moment.Moment | undefined;
    shortDesc1!: string | undefined;
    shortDesc2!: string | undefined;
    shortDesc3!: string | undefined;
    desc!: string | undefined;
    categoryIds!: string | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IProductDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.sku = data["sku"];
            this.name = data["name"];
            this.status = data["status"];
            this.unit = data["unit"];
            this.price = data["price"];
            this.contractorPrice = data["contractorPrice"];
            this.categoryId = data["categoryId"];
            this.plateId = data["plateId"];
            this.imageUrl = data["imageUrl"];
            if (data["categories"] && data["categories"].constructor === Array) {
                this.categories = [];
                for (let item of data["categories"])
                    this.categories.push(item);
            }
            this.tenantId = data["tenantId"];
            this.barcode = data["barcode"];
            this.fromDate = data["fromDate"] ? moment(data["fromDate"].toString()) : <any>undefined;
            this.toDate = data["toDate"] ? moment(data["toDate"].toString()) : <any>undefined;
            this.shortDesc1 = data["shortDesc1"];
            this.shortDesc2 = data["shortDesc2"];
            this.shortDesc3 = data["shortDesc3"];
            this.desc = data["desc"];
            this.categoryIds = data["categoryIds"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ProductDto {
        data = typeof data === 'object' ? data : {};
        let result = new ProductDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sku"] = this.sku;
        data["name"] = this.name;
        data["status"] = this.status;
        data["unit"] = this.unit;
        data["price"] = this.price;
        data["contractorPrice"] = this.contractorPrice;
        data["categoryId"] = this.categoryId;
        data["plateId"] = this.plateId;
        data["imageUrl"] = this.imageUrl;
        if (this.categories && this.categories.constructor === Array) {
            data["categories"] = [];
            for (let item of this.categories)
                data["categories"].push(item);
        }
        data["tenantId"] = this.tenantId;
        data["barcode"] = this.barcode;
        data["fromDate"] = this.fromDate ? this.fromDate.toISOString() : <any>undefined;
        data["toDate"] = this.toDate ? this.toDate.toISOString() : <any>undefined;
        data["shortDesc1"] = this.shortDesc1;
        data["shortDesc2"] = this.shortDesc2;
        data["shortDesc3"] = this.shortDesc3;
        data["desc"] = this.desc;
        data["categoryIds"] = this.categoryIds;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IProductDto {
    sku: string | undefined;
    name: string | undefined;
    status: number | undefined;
    unit: string | undefined;
    price: number | undefined;
    contractorPrice: number | undefined;
    categoryId: string | undefined;
    plateId: string | undefined;
    imageUrl: string | undefined;
    categories: string[] | undefined;
    tenantId: number | undefined;
    barcode: string | undefined;
    fromDate: moment.Moment | undefined;
    toDate: moment.Moment | undefined;
    shortDesc1: string | undefined;
    shortDesc2: string | undefined;
    shortDesc3: string | undefined;
    desc: string | undefined;
    categoryIds: string | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class GetCategoryForEditOutput implements IGetCategoryForEditOutput {
    category!: CreateOrEditCategoryDto | undefined;

    constructor(data?: IGetCategoryForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.category = data["category"] ? CreateOrEditCategoryDto.fromJS(data["category"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetCategoryForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetCategoryForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["category"] = this.category ? this.category.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetCategoryForEditOutput {
    category: CreateOrEditCategoryDto | undefined;
}

export class CreateOrEditCategoryDto implements ICreateOrEditCategoryDto {
    name!: string;
    description!: string | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditCategoryDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.description = data["description"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["description"] = this.description;
        data["id"] = this.id;
        return data;
    }
}

export interface ICreateOrEditCategoryDto {
    name: string;
    description: string | undefined;
    id: string | undefined;
}

export class GetUserChatFriendsWithSettingsOutput implements IGetUserChatFriendsWithSettingsOutput {
    serverTime!: moment.Moment | undefined;
    friends!: FriendDto[] | undefined;

    constructor(data?: IGetUserChatFriendsWithSettingsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.serverTime = data["serverTime"] ? moment(data["serverTime"].toString()) : <any>undefined;
            if (data["friends"] && data["friends"].constructor === Array) {
                this.friends = [];
                for (let item of data["friends"])
                    this.friends.push(FriendDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetUserChatFriendsWithSettingsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetUserChatFriendsWithSettingsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["serverTime"] = this.serverTime ? this.serverTime.toISOString() : <any>undefined;
        if (this.friends && this.friends.constructor === Array) {
            data["friends"] = [];
            for (let item of this.friends)
                data["friends"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetUserChatFriendsWithSettingsOutput {
    serverTime: moment.Moment | undefined;
    friends: FriendDto[] | undefined;
}

export class FriendDto implements IFriendDto {
    friendUserId!: number | undefined;
    friendTenantId!: number | undefined;
    friendUserName!: string | undefined;
    friendTenancyName!: string | undefined;
    friendProfilePictureId!: string | undefined;
    unreadMessageCount!: number | undefined;
    isOnline!: boolean | undefined;
    state!: FriendDtoState | undefined;

    constructor(data?: IFriendDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.friendUserId = data["friendUserId"];
            this.friendTenantId = data["friendTenantId"];
            this.friendUserName = data["friendUserName"];
            this.friendTenancyName = data["friendTenancyName"];
            this.friendProfilePictureId = data["friendProfilePictureId"];
            this.unreadMessageCount = data["unreadMessageCount"];
            this.isOnline = data["isOnline"];
            this.state = data["state"];
        }
    }

    static fromJS(data: any): FriendDto {
        data = typeof data === 'object' ? data : {};
        let result = new FriendDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["friendUserId"] = this.friendUserId;
        data["friendTenantId"] = this.friendTenantId;
        data["friendUserName"] = this.friendUserName;
        data["friendTenancyName"] = this.friendTenancyName;
        data["friendProfilePictureId"] = this.friendProfilePictureId;
        data["unreadMessageCount"] = this.unreadMessageCount;
        data["isOnline"] = this.isOnline;
        data["state"] = this.state;
        return data;
    }
}

export interface IFriendDto {
    friendUserId: number | undefined;
    friendTenantId: number | undefined;
    friendUserName: string | undefined;
    friendTenancyName: string | undefined;
    friendProfilePictureId: string | undefined;
    unreadMessageCount: number | undefined;
    isOnline: boolean | undefined;
    state: FriendDtoState | undefined;
}

export class ListResultDtoOfChatMessageDto implements IListResultDtoOfChatMessageDto {
    items!: ChatMessageDto[] | undefined;

    constructor(data?: IListResultDtoOfChatMessageDto) {
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
                    this.items.push(ChatMessageDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfChatMessageDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfChatMessageDto();
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

export interface IListResultDtoOfChatMessageDto {
    items: ChatMessageDto[] | undefined;
}

export class ChatMessageDto implements IChatMessageDto {
    userId!: number | undefined;
    tenantId!: number | undefined;
    targetUserId!: number | undefined;
    targetTenantId!: number | undefined;
    side!: ChatMessageDtoSide | undefined;
    readState!: ChatMessageDtoReadState | undefined;
    receiverReadState!: ChatMessageDtoReceiverReadState | undefined;
    message!: string | undefined;
    creationTime!: moment.Moment | undefined;
    sharedMessageId!: string | undefined;
    id!: number | undefined;

    constructor(data?: IChatMessageDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.tenantId = data["tenantId"];
            this.targetUserId = data["targetUserId"];
            this.targetTenantId = data["targetTenantId"];
            this.side = data["side"];
            this.readState = data["readState"];
            this.receiverReadState = data["receiverReadState"];
            this.message = data["message"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.sharedMessageId = data["sharedMessageId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ChatMessageDto {
        data = typeof data === 'object' ? data : {};
        let result = new ChatMessageDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["tenantId"] = this.tenantId;
        data["targetUserId"] = this.targetUserId;
        data["targetTenantId"] = this.targetTenantId;
        data["side"] = this.side;
        data["readState"] = this.readState;
        data["receiverReadState"] = this.receiverReadState;
        data["message"] = this.message;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["sharedMessageId"] = this.sharedMessageId;
        data["id"] = this.id;
        return data;
    }
}

export interface IChatMessageDto {
    userId: number | undefined;
    tenantId: number | undefined;
    targetUserId: number | undefined;
    targetTenantId: number | undefined;
    side: ChatMessageDtoSide | undefined;
    readState: ChatMessageDtoReadState | undefined;
    receiverReadState: ChatMessageDtoReceiverReadState | undefined;
    message: string | undefined;
    creationTime: moment.Moment | undefined;
    sharedMessageId: string | undefined;
    id: number | undefined;
}

export class MarkAllUnreadMessagesOfUserAsReadInput implements IMarkAllUnreadMessagesOfUserAsReadInput {
    tenantId!: number | undefined;
    userId!: number | undefined;

    constructor(data?: IMarkAllUnreadMessagesOfUserAsReadInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.userId = data["userId"];
        }
    }

    static fromJS(data: any): MarkAllUnreadMessagesOfUserAsReadInput {
        data = typeof data === 'object' ? data : {};
        let result = new MarkAllUnreadMessagesOfUserAsReadInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["userId"] = this.userId;
        return data;
    }
}

export interface IMarkAllUnreadMessagesOfUserAsReadInput {
    tenantId: number | undefined;
    userId: number | undefined;
}

export class ListResultDtoOfSubscribableEditionComboboxItemDto implements IListResultDtoOfSubscribableEditionComboboxItemDto {
    items!: SubscribableEditionComboboxItemDto[] | undefined;

    constructor(data?: IListResultDtoOfSubscribableEditionComboboxItemDto) {
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
                    this.items.push(SubscribableEditionComboboxItemDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfSubscribableEditionComboboxItemDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfSubscribableEditionComboboxItemDto();
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

export interface IListResultDtoOfSubscribableEditionComboboxItemDto {
    items: SubscribableEditionComboboxItemDto[] | undefined;
}

export class SubscribableEditionComboboxItemDto implements ISubscribableEditionComboboxItemDto {
    isFree!: boolean | undefined;
    value!: string | undefined;
    displayText!: string | undefined;
    isSelected!: boolean | undefined;

    constructor(data?: ISubscribableEditionComboboxItemDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.isFree = data["isFree"];
            this.value = data["value"];
            this.displayText = data["displayText"];
            this.isSelected = data["isSelected"];
        }
    }

    static fromJS(data: any): SubscribableEditionComboboxItemDto {
        data = typeof data === 'object' ? data : {};
        let result = new SubscribableEditionComboboxItemDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isFree"] = this.isFree;
        data["value"] = this.value;
        data["displayText"] = this.displayText;
        data["isSelected"] = this.isSelected;
        return data;
    }
}

export interface ISubscribableEditionComboboxItemDto {
    isFree: boolean | undefined;
    value: string | undefined;
    displayText: string | undefined;
    isSelected: boolean | undefined;
}

export class FindUsersInput implements IFindUsersInput {
    tenantId!: number | undefined;
    maxResultCount!: number | undefined;
    skipCount!: number | undefined;
    filter!: string | undefined;

    constructor(data?: IFindUsersInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.maxResultCount = data["maxResultCount"];
            this.skipCount = data["skipCount"];
            this.filter = data["filter"];
        }
    }

    static fromJS(data: any): FindUsersInput {
        data = typeof data === 'object' ? data : {};
        let result = new FindUsersInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["maxResultCount"] = this.maxResultCount;
        data["skipCount"] = this.skipCount;
        data["filter"] = this.filter;
        return data;
    }
}

export interface IFindUsersInput {
    tenantId: number | undefined;
    maxResultCount: number | undefined;
    skipCount: number | undefined;
    filter: string | undefined;
}

export class PagedResultDtoOfNameValueDto implements IPagedResultDtoOfNameValueDto {
    totalCount!: number | undefined;
    items!: NameValueDto[] | undefined;

    constructor(data?: IPagedResultDtoOfNameValueDto) {
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
                    this.items.push(NameValueDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfNameValueDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfNameValueDto();
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
}

export interface IPagedResultDtoOfNameValueDto {
    totalCount: number | undefined;
    items: NameValueDto[] | undefined;
}

export class GetDefaultEditionNameOutput implements IGetDefaultEditionNameOutput {
    name!: string | undefined;

    constructor(data?: IGetDefaultEditionNameOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
        }
    }

    static fromJS(data: any): GetDefaultEditionNameOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetDefaultEditionNameOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        return data;
    }
}

export interface IGetDefaultEditionNameOutput {
    name: string | undefined;
}

export class PublishNsqCommandDto implements IPublishNsqCommandDto {
    topic!: string | undefined;
    commandObj!: any | undefined;

    constructor(data?: IPublishNsqCommandDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.topic = data["topic"];
            this.commandObj = data["commandObj"];
        }
    }

    static fromJS(data: any): PublishNsqCommandDto {
        data = typeof data === 'object' ? data : {};
        let result = new PublishNsqCommandDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["topic"] = this.topic;
        data["commandObj"] = this.commandObj;
        return data;
    }
}

export interface IPublishNsqCommandDto {
    topic: string | undefined;
    commandObj: any | undefined;
}

export class GetComPortDto implements IGetComPortDto {
    port!: string | undefined;
    isSelected!: boolean | undefined;

    constructor(data?: IGetComPortDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.port = data["port"];
            this.isSelected = data["isSelected"];
        }
    }

    static fromJS(data: any): GetComPortDto {
        data = typeof data === 'object' ? data : {};
        let result = new GetComPortDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["port"] = this.port;
        data["isSelected"] = this.isSelected;
        return data;
    }
}

export interface IGetComPortDto {
    port: string | undefined;
    isSelected: boolean | undefined;
}

export class ReadLogDto implements IReadLogDto {
    logType!: string | undefined;
    date!: moment.Moment | undefined;

    constructor(data?: IReadLogDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.logType = data["logType"];
            this.date = data["date"] ? moment(data["date"].toString()) : <any>undefined;
        }
    }

    static fromJS(data: any): ReadLogDto {
        data = typeof data === 'object' ? data : {};
        let result = new ReadLogDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["logType"] = this.logType;
        data["date"] = this.date ? this.date.toISOString() : <any>undefined;
        return data;
    }
}

export interface IReadLogDto {
    logType: string | undefined;
    date: moment.Moment | undefined;
}

export class DashboardData implements IDashboardData {
    totalTransSale!: number | undefined;
    totalTransToday!: number | undefined;
    totalTransCurrentSession!: number | undefined;
    totalTransCurrentSessionSale!: number | undefined;
    salesSummary!: SalesData[] | undefined;
    sessionStat!: SessionStatData[] | undefined;

    constructor(data?: IDashboardData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.totalTransSale = data["totalTransSale"];
            this.totalTransToday = data["totalTransToday"];
            this.totalTransCurrentSession = data["totalTransCurrentSession"];
            this.totalTransCurrentSessionSale = data["totalTransCurrentSessionSale"];
            if (data["salesSummary"] && data["salesSummary"].constructor === Array) {
                this.salesSummary = [];
                for (let item of data["salesSummary"])
                    this.salesSummary.push(SalesData.fromJS(item));
            }
            if (data["sessionStat"] && data["sessionStat"].constructor === Array) {
                this.sessionStat = [];
                for (let item of data["sessionStat"])
                    this.sessionStat.push(SessionStatData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): DashboardData {
        data = typeof data === 'object' ? data : {};
        let result = new DashboardData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalTransSale"] = this.totalTransSale;
        data["totalTransToday"] = this.totalTransToday;
        data["totalTransCurrentSession"] = this.totalTransCurrentSession;
        data["totalTransCurrentSessionSale"] = this.totalTransCurrentSessionSale;
        if (this.salesSummary && this.salesSummary.constructor === Array) {
            data["salesSummary"] = [];
            for (let item of this.salesSummary)
                data["salesSummary"].push(item.toJSON());
        }
        if (this.sessionStat && this.sessionStat.constructor === Array) {
            data["sessionStat"] = [];
            for (let item of this.sessionStat)
                data["sessionStat"].push(item.toJSON());
        }
        return data;
    }
}

export interface IDashboardData {
    totalTransSale: number | undefined;
    totalTransToday: number | undefined;
    totalTransCurrentSession: number | undefined;
    totalTransCurrentSessionSale: number | undefined;
    salesSummary: SalesData[] | undefined;
    sessionStat: SessionStatData[] | undefined;
}

export class SalesData implements ISalesData {
    period!: string | undefined;
    sales!: number | undefined;
    trans!: number | undefined;

    constructor(data?: ISalesData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.period = data["period"];
            this.sales = data["sales"];
            this.trans = data["trans"];
        }
    }

    static fromJS(data: any): SalesData {
        data = typeof data === 'object' ? data : {};
        let result = new SalesData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["period"] = this.period;
        data["sales"] = this.sales;
        data["trans"] = this.trans;
        return data;
    }
}

export interface ISalesData {
    period: string | undefined;
    sales: number | undefined;
    trans: number | undefined;
}

export class SessionStatData implements ISessionStatData {
    sessionName!: string | undefined;
    totalTransaction!: number | undefined;
    totalSale!: number | undefined;

    constructor(data?: ISessionStatData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.sessionName = data["sessionName"];
            this.totalTransaction = data["totalTransaction"];
            this.totalSale = data["totalSale"];
        }
    }

    static fromJS(data: any): SessionStatData {
        data = typeof data === 'object' ? data : {};
        let result = new SessionStatData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sessionName"] = this.sessionName;
        data["totalTransaction"] = this.totalTransaction;
        data["totalSale"] = this.totalSale;
        return data;
    }
}

export interface ISessionStatData {
    sessionName: string | undefined;
    totalTransaction: number | undefined;
    totalSale: number | undefined;
}

export class SalesDataOuput implements ISalesDataOuput {
    salesSummary!: SalesData[] | undefined;

    constructor(data?: ISalesDataOuput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["salesSummary"] && data["salesSummary"].constructor === Array) {
                this.salesSummary = [];
                for (let item of data["salesSummary"])
                    this.salesSummary.push(SalesData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): SalesDataOuput {
        data = typeof data === 'object' ? data : {};
        let result = new SalesDataOuput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.salesSummary && this.salesSummary.constructor === Array) {
            data["salesSummary"] = [];
            for (let item of this.salesSummary)
                data["salesSummary"].push(item.toJSON());
        }
        return data;
    }
}

export interface ISalesDataOuput {
    salesSummary: SalesData[] | undefined;
}

export class SessionStatDataOutput implements ISessionStatDataOutput {
    sessionStat!: SessionStatData[] | undefined;

    constructor(data?: ISessionStatDataOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["sessionStat"] && data["sessionStat"].constructor === Array) {
                this.sessionStat = [];
                for (let item of data["sessionStat"])
                    this.sessionStat.push(SessionStatData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): SessionStatDataOutput {
        data = typeof data === 'object' ? data : {};
        let result = new SessionStatDataOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.sessionStat && this.sessionStat.constructor === Array) {
            data["sessionStat"] = [];
            for (let item of this.sessionStat)
                data["sessionStat"].push(item.toJSON());
        }
        return data;
    }
}

export interface ISessionStatDataOutput {
    sessionStat: SessionStatData[] | undefined;
}

export class DateToStringOutput implements IDateToStringOutput {
    dateString!: string | undefined;

    constructor(data?: IDateToStringOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.dateString = data["dateString"];
        }
    }

    static fromJS(data: any): DateToStringOutput {
        data = typeof data === 'object' ? data : {};
        let result = new DateToStringOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["dateString"] = this.dateString;
        return data;
    }
}

export interface IDateToStringOutput {
    dateString: string | undefined;
}

export class NameValueOfString implements INameValueOfString {
    name!: string | undefined;
    value!: string | undefined;

    constructor(data?: INameValueOfString) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): NameValueOfString {
        data = typeof data === 'object' ? data : {};
        let result = new NameValueOfString();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["value"] = this.value;
        return data;
    }
}

export interface INameValueOfString {
    name: string | undefined;
    value: string | undefined;
}

export class StringOutput implements IStringOutput {
    output!: string | undefined;

    constructor(data?: IStringOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.output = data["output"];
        }
    }

    static fromJS(data: any): StringOutput {
        data = typeof data === 'object' ? data : {};
        let result = new StringOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["output"] = this.output;
        return data;
    }
}

export interface IStringOutput {
    output: string | undefined;
}

export class PagedResultDtoOfGetDiscForView implements IPagedResultDtoOfGetDiscForView {
    totalCount!: number | undefined;
    items!: GetDiscForView[] | undefined;

    constructor(data?: IPagedResultDtoOfGetDiscForView) {
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
                    this.items.push(GetDiscForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetDiscForView {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetDiscForView();
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
}

export interface IPagedResultDtoOfGetDiscForView {
    totalCount: number | undefined;
    items: GetDiscForView[] | undefined;
}

export class GetDiscForView implements IGetDiscForView {
    disc!: DiscDto | undefined;
    plateName!: string | undefined;

    constructor(data?: IGetDiscForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.disc = data["disc"] ? DiscDto.fromJS(data["disc"]) : <any>undefined;
            this.plateName = data["plateName"];
        }
    }

    static fromJS(data: any): GetDiscForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetDiscForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["disc"] = this.disc ? this.disc.toJSON() : <any>undefined;
        data["plateName"] = this.plateName;
        return data;
    }
}

export interface IGetDiscForView {
    disc: DiscDto | undefined;
    plateName: string | undefined;
}

export class DiscDto implements IDiscDto {
    uid!: string | undefined;
    code!: string | undefined;
    plateId!: string | undefined;
    id!: string | undefined;

    constructor(data?: IDiscDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.uid = data["uid"];
            this.code = data["code"];
            this.plateId = data["plateId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): DiscDto {
        data = typeof data === 'object' ? data : {};
        let result = new DiscDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["uid"] = this.uid;
        data["code"] = this.code;
        data["plateId"] = this.plateId;
        data["id"] = this.id;
        return data;
    }
}

export interface IDiscDto {
    uid: string | undefined;
    code: string | undefined;
    plateId: string | undefined;
    id: string | undefined;
}

export class GetDiscForEditOutput implements IGetDiscForEditOutput {
    disc!: CreateOrEditDiscDto | undefined;
    plateName!: string | undefined;

    constructor(data?: IGetDiscForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.disc = data["disc"] ? CreateOrEditDiscDto.fromJS(data["disc"]) : <any>undefined;
            this.plateName = data["plateName"];
        }
    }

    static fromJS(data: any): GetDiscForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetDiscForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["disc"] = this.disc ? this.disc.toJSON() : <any>undefined;
        data["plateName"] = this.plateName;
        return data;
    }
}

export interface IGetDiscForEditOutput {
    disc: CreateOrEditDiscDto | undefined;
    plateName: string | undefined;
}

export class CreateOrEditDiscDto implements ICreateOrEditDiscDto {
    uid!: string;
    code!: string;
    plateId!: string | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditDiscDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.uid = data["uid"];
            this.code = data["code"];
            this.plateId = data["plateId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditDiscDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditDiscDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["uid"] = this.uid;
        data["code"] = this.code;
        data["plateId"] = this.plateId;
        data["id"] = this.id;
        return data;
    }
}

export interface ICreateOrEditDiscDto {
    uid: string;
    code: string;
    plateId: string | undefined;
    id: string | undefined;
}

export class PagedResultDtoOfPlateLookupTableDto implements IPagedResultDtoOfPlateLookupTableDto {
    totalCount!: number | undefined;
    items!: PlateLookupTableDto[] | undefined;

    constructor(data?: IPagedResultDtoOfPlateLookupTableDto) {
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
                    this.items.push(PlateLookupTableDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfPlateLookupTableDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfPlateLookupTableDto();
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
}

export interface IPagedResultDtoOfPlateLookupTableDto {
    totalCount: number | undefined;
    items: PlateLookupTableDto[] | undefined;
}

export class PlateLookupTableDto implements IPlateLookupTableDto {
    id!: string | undefined;
    displayName!: string | undefined;
    code!: string | undefined;

    constructor(data?: IPlateLookupTableDto) {
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
            this.displayName = data["displayName"];
            this.code = data["code"];
        }
    }

    static fromJS(data: any): PlateLookupTableDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateLookupTableDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["displayName"] = this.displayName;
        data["code"] = this.code;
        return data;
    }
}

export interface IPlateLookupTableDto {
    id: string | undefined;
    displayName: string | undefined;
    code: string | undefined;
}

export class ListResultDtoOfEditionListDto implements IListResultDtoOfEditionListDto {
    items!: EditionListDto[] | undefined;

    constructor(data?: IListResultDtoOfEditionListDto) {
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
                    this.items.push(EditionListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfEditionListDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfEditionListDto();
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

export interface IListResultDtoOfEditionListDto {
    items: EditionListDto[] | undefined;
}

export class EditionListDto implements IEditionListDto {
    name!: string | undefined;
    displayName!: string | undefined;
    creationTime!: moment.Moment | undefined;
    id!: number | undefined;

    constructor(data?: IEditionListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): EditionListDto {
        data = typeof data === 'object' ? data : {};
        let result = new EditionListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface IEditionListDto {
    name: string | undefined;
    displayName: string | undefined;
    creationTime: moment.Moment | undefined;
    id: number | undefined;
}

export class GetEditionEditOutput implements IGetEditionEditOutput {
    edition!: EditionEditDto | undefined;
    featureValues!: NameValueDto[] | undefined;
    features!: FlatFeatureDto[] | undefined;

    constructor(data?: IGetEditionEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.edition = data["edition"] ? EditionEditDto.fromJS(data["edition"]) : <any>undefined;
            if (data["featureValues"] && data["featureValues"].constructor === Array) {
                this.featureValues = [];
                for (let item of data["featureValues"])
                    this.featureValues.push(NameValueDto.fromJS(item));
            }
            if (data["features"] && data["features"].constructor === Array) {
                this.features = [];
                for (let item of data["features"])
                    this.features.push(FlatFeatureDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetEditionEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetEditionEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["edition"] = this.edition ? this.edition.toJSON() : <any>undefined;
        if (this.featureValues && this.featureValues.constructor === Array) {
            data["featureValues"] = [];
            for (let item of this.featureValues)
                data["featureValues"].push(item.toJSON());
        }
        if (this.features && this.features.constructor === Array) {
            data["features"] = [];
            for (let item of this.features)
                data["features"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetEditionEditOutput {
    edition: EditionEditDto | undefined;
    featureValues: NameValueDto[] | undefined;
    features: FlatFeatureDto[] | undefined;
}

export class EditionEditDto implements IEditionEditDto {
    id!: number | undefined;
    displayName!: string;
    monthlyPrice!: number | undefined;
    annualPrice!: number | undefined;
    trialDayCount!: number | undefined;
    waitingDayAfterExpire!: number | undefined;
    expiringEditionId!: number | undefined;

    constructor(data?: IEditionEditDto) {
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
            this.displayName = data["displayName"];
            this.monthlyPrice = data["monthlyPrice"];
            this.annualPrice = data["annualPrice"];
            this.trialDayCount = data["trialDayCount"];
            this.waitingDayAfterExpire = data["waitingDayAfterExpire"];
            this.expiringEditionId = data["expiringEditionId"];
        }
    }

    static fromJS(data: any): EditionEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new EditionEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["displayName"] = this.displayName;
        data["monthlyPrice"] = this.monthlyPrice;
        data["annualPrice"] = this.annualPrice;
        data["trialDayCount"] = this.trialDayCount;
        data["waitingDayAfterExpire"] = this.waitingDayAfterExpire;
        data["expiringEditionId"] = this.expiringEditionId;
        return data;
    }
}

export interface IEditionEditDto {
    id: number | undefined;
    displayName: string;
    monthlyPrice: number | undefined;
    annualPrice: number | undefined;
    trialDayCount: number | undefined;
    waitingDayAfterExpire: number | undefined;
    expiringEditionId: number | undefined;
}

export class FlatFeatureDto implements IFlatFeatureDto {
    parentName!: string | undefined;
    name!: string | undefined;
    displayName!: string | undefined;
    description!: string | undefined;
    defaultValue!: string | undefined;
    inputType!: FeatureInputTypeDto | undefined;

    constructor(data?: IFlatFeatureDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.parentName = data["parentName"];
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.description = data["description"];
            this.defaultValue = data["defaultValue"];
            this.inputType = data["inputType"] ? FeatureInputTypeDto.fromJS(data["inputType"]) : <any>undefined;
        }
    }

    static fromJS(data: any): FlatFeatureDto {
        data = typeof data === 'object' ? data : {};
        let result = new FlatFeatureDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["parentName"] = this.parentName;
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["description"] = this.description;
        data["defaultValue"] = this.defaultValue;
        data["inputType"] = this.inputType ? this.inputType.toJSON() : <any>undefined;
        return data;
    }
}

export interface IFlatFeatureDto {
    parentName: string | undefined;
    name: string | undefined;
    displayName: string | undefined;
    description: string | undefined;
    defaultValue: string | undefined;
    inputType: FeatureInputTypeDto | undefined;
}

export class FeatureInputTypeDto implements IFeatureInputTypeDto {
    name!: string | undefined;
    attributes!: { [key: string]: any; } | undefined;
    validator!: IValueValidator | undefined;
    itemSource!: LocalizableComboboxItemSourceDto | undefined;

    constructor(data?: IFeatureInputTypeDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            if (data["attributes"]) {
                this.attributes = {};
                for (let key in data["attributes"]) {
                    if (data["attributes"].hasOwnProperty(key))
                        this.attributes[key] = data["attributes"][key];
                }
            }
            this.validator = data["validator"] ? IValueValidator.fromJS(data["validator"]) : <any>undefined;
            this.itemSource = data["itemSource"] ? LocalizableComboboxItemSourceDto.fromJS(data["itemSource"]) : <any>undefined;
        }
    }

    static fromJS(data: any): FeatureInputTypeDto {
        data = typeof data === 'object' ? data : {};
        let result = new FeatureInputTypeDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        if (this.attributes) {
            data["attributes"] = {};
            for (let key in this.attributes) {
                if (this.attributes.hasOwnProperty(key))
                    data["attributes"][key] = this.attributes[key];
            }
        }
        data["validator"] = this.validator ? this.validator.toJSON() : <any>undefined;
        data["itemSource"] = this.itemSource ? this.itemSource.toJSON() : <any>undefined;
        return data;
    }
}

export interface IFeatureInputTypeDto {
    name: string | undefined;
    attributes: { [key: string]: any; } | undefined;
    validator: IValueValidator | undefined;
    itemSource: LocalizableComboboxItemSourceDto | undefined;
}

export class IValueValidator implements IIValueValidator {
    name!: string | undefined;
    attributes!: { [key: string]: any; } | undefined;

    constructor(data?: IIValueValidator) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            if (data["attributes"]) {
                this.attributes = {};
                for (let key in data["attributes"]) {
                    if (data["attributes"].hasOwnProperty(key))
                        this.attributes[key] = data["attributes"][key];
                }
            }
        }
    }

    static fromJS(data: any): IValueValidator {
        data = typeof data === 'object' ? data : {};
        let result = new IValueValidator();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        if (this.attributes) {
            data["attributes"] = {};
            for (let key in this.attributes) {
                if (this.attributes.hasOwnProperty(key))
                    data["attributes"][key] = this.attributes[key];
            }
        }
        return data;
    }
}

export interface IIValueValidator {
    name: string | undefined;
    attributes: { [key: string]: any; } | undefined;
}

export class LocalizableComboboxItemSourceDto implements ILocalizableComboboxItemSourceDto {
    items!: LocalizableComboboxItemDto[] | undefined;

    constructor(data?: ILocalizableComboboxItemSourceDto) {
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
                    this.items.push(LocalizableComboboxItemDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): LocalizableComboboxItemSourceDto {
        data = typeof data === 'object' ? data : {};
        let result = new LocalizableComboboxItemSourceDto();
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

export interface ILocalizableComboboxItemSourceDto {
    items: LocalizableComboboxItemDto[] | undefined;
}

export class LocalizableComboboxItemDto implements ILocalizableComboboxItemDto {
    value!: string | undefined;
    displayText!: string | undefined;

    constructor(data?: ILocalizableComboboxItemDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.value = data["value"];
            this.displayText = data["displayText"];
        }
    }

    static fromJS(data: any): LocalizableComboboxItemDto {
        data = typeof data === 'object' ? data : {};
        let result = new LocalizableComboboxItemDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["value"] = this.value;
        data["displayText"] = this.displayText;
        return data;
    }
}

export interface ILocalizableComboboxItemDto {
    value: string | undefined;
    displayText: string | undefined;
}

export class CreateOrUpdateEditionDto implements ICreateOrUpdateEditionDto {
    edition!: EditionEditDto;
    featureValues!: NameValueDto[];

    constructor(data?: ICreateOrUpdateEditionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.edition = new EditionEditDto();
            this.featureValues = [];
        }
    }

    init(data?: any) {
        if (data) {
            this.edition = data["edition"] ? EditionEditDto.fromJS(data["edition"]) : new EditionEditDto();
            if (data["featureValues"] && data["featureValues"].constructor === Array) {
                this.featureValues = [];
                for (let item of data["featureValues"])
                    this.featureValues.push(NameValueDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): CreateOrUpdateEditionDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrUpdateEditionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["edition"] = this.edition ? this.edition.toJSON() : <any>undefined;
        if (this.featureValues && this.featureValues.constructor === Array) {
            data["featureValues"] = [];
            for (let item of this.featureValues)
                data["featureValues"].push(item.toJSON());
        }
        return data;
    }
}

export interface ICreateOrUpdateEditionDto {
    edition: EditionEditDto;
    featureValues: NameValueDto[];
}

export class CreateFriendshipRequestInput implements ICreateFriendshipRequestInput {
    userId!: number | undefined;
    tenantId!: number | undefined;

    constructor(data?: ICreateFriendshipRequestInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.tenantId = data["tenantId"];
        }
    }

    static fromJS(data: any): CreateFriendshipRequestInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateFriendshipRequestInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["tenantId"] = this.tenantId;
        return data;
    }
}

export interface ICreateFriendshipRequestInput {
    userId: number | undefined;
    tenantId: number | undefined;
}

export class CreateFriendshipRequestByUserNameInput implements ICreateFriendshipRequestByUserNameInput {
    tenancyName!: string;
    userName!: string | undefined;

    constructor(data?: ICreateFriendshipRequestByUserNameInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.userName = data["userName"];
        }
    }

    static fromJS(data: any): CreateFriendshipRequestByUserNameInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateFriendshipRequestByUserNameInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["userName"] = this.userName;
        return data;
    }
}

export interface ICreateFriendshipRequestByUserNameInput {
    tenancyName: string;
    userName: string | undefined;
}

export class BlockUserInput implements IBlockUserInput {
    userId!: number | undefined;
    tenantId!: number | undefined;

    constructor(data?: IBlockUserInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.tenantId = data["tenantId"];
        }
    }

    static fromJS(data: any): BlockUserInput {
        data = typeof data === 'object' ? data : {};
        let result = new BlockUserInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["tenantId"] = this.tenantId;
        return data;
    }
}

export interface IBlockUserInput {
    userId: number | undefined;
    tenantId: number | undefined;
}

export class UnblockUserInput implements IUnblockUserInput {
    userId!: number | undefined;
    tenantId!: number | undefined;

    constructor(data?: IUnblockUserInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.tenantId = data["tenantId"];
        }
    }

    static fromJS(data: any): UnblockUserInput {
        data = typeof data === 'object' ? data : {};
        let result = new UnblockUserInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["tenantId"] = this.tenantId;
        return data;
    }
}

export interface IUnblockUserInput {
    userId: number | undefined;
    tenantId: number | undefined;
}

export class AcceptFriendshipRequestInput implements IAcceptFriendshipRequestInput {
    userId!: number | undefined;
    tenantId!: number | undefined;

    constructor(data?: IAcceptFriendshipRequestInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.tenantId = data["tenantId"];
        }
    }

    static fromJS(data: any): AcceptFriendshipRequestInput {
        data = typeof data === 'object' ? data : {};
        let result = new AcceptFriendshipRequestInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["tenantId"] = this.tenantId;
        return data;
    }
}

export interface IAcceptFriendshipRequestInput {
    userId: number | undefined;
    tenantId: number | undefined;
}

export class HostDashboardData implements IHostDashboardData {
    newTenantsCount!: number | undefined;
    newSubscriptionAmount!: number | undefined;
    dashboardPlaceholder1!: number | undefined;
    dashboardPlaceholder2!: number | undefined;
    incomeStatistics!: IncomeStastistic[] | undefined;
    editionStatistics!: TenantEdition[] | undefined;
    expiringTenants!: ExpiringTenant[] | undefined;
    recentTenants!: RecentTenant[] | undefined;
    maxExpiringTenantsShownCount!: number | undefined;
    maxRecentTenantsShownCount!: number | undefined;
    subscriptionEndAlertDayCount!: number | undefined;
    recentTenantsDayCount!: number | undefined;
    subscriptionEndDateStart!: moment.Moment | undefined;
    subscriptionEndDateEnd!: moment.Moment | undefined;
    tenantCreationStartDate!: moment.Moment | undefined;

    constructor(data?: IHostDashboardData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.newTenantsCount = data["newTenantsCount"];
            this.newSubscriptionAmount = data["newSubscriptionAmount"];
            this.dashboardPlaceholder1 = data["dashboardPlaceholder1"];
            this.dashboardPlaceholder2 = data["dashboardPlaceholder2"];
            if (data["incomeStatistics"] && data["incomeStatistics"].constructor === Array) {
                this.incomeStatistics = [];
                for (let item of data["incomeStatistics"])
                    this.incomeStatistics.push(IncomeStastistic.fromJS(item));
            }
            if (data["editionStatistics"] && data["editionStatistics"].constructor === Array) {
                this.editionStatistics = [];
                for (let item of data["editionStatistics"])
                    this.editionStatistics.push(TenantEdition.fromJS(item));
            }
            if (data["expiringTenants"] && data["expiringTenants"].constructor === Array) {
                this.expiringTenants = [];
                for (let item of data["expiringTenants"])
                    this.expiringTenants.push(ExpiringTenant.fromJS(item));
            }
            if (data["recentTenants"] && data["recentTenants"].constructor === Array) {
                this.recentTenants = [];
                for (let item of data["recentTenants"])
                    this.recentTenants.push(RecentTenant.fromJS(item));
            }
            this.maxExpiringTenantsShownCount = data["maxExpiringTenantsShownCount"];
            this.maxRecentTenantsShownCount = data["maxRecentTenantsShownCount"];
            this.subscriptionEndAlertDayCount = data["subscriptionEndAlertDayCount"];
            this.recentTenantsDayCount = data["recentTenantsDayCount"];
            this.subscriptionEndDateStart = data["subscriptionEndDateStart"] ? moment(data["subscriptionEndDateStart"].toString()) : <any>undefined;
            this.subscriptionEndDateEnd = data["subscriptionEndDateEnd"] ? moment(data["subscriptionEndDateEnd"].toString()) : <any>undefined;
            this.tenantCreationStartDate = data["tenantCreationStartDate"] ? moment(data["tenantCreationStartDate"].toString()) : <any>undefined;
        }
    }

    static fromJS(data: any): HostDashboardData {
        data = typeof data === 'object' ? data : {};
        let result = new HostDashboardData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["newTenantsCount"] = this.newTenantsCount;
        data["newSubscriptionAmount"] = this.newSubscriptionAmount;
        data["dashboardPlaceholder1"] = this.dashboardPlaceholder1;
        data["dashboardPlaceholder2"] = this.dashboardPlaceholder2;
        if (this.incomeStatistics && this.incomeStatistics.constructor === Array) {
            data["incomeStatistics"] = [];
            for (let item of this.incomeStatistics)
                data["incomeStatistics"].push(item.toJSON());
        }
        if (this.editionStatistics && this.editionStatistics.constructor === Array) {
            data["editionStatistics"] = [];
            for (let item of this.editionStatistics)
                data["editionStatistics"].push(item.toJSON());
        }
        if (this.expiringTenants && this.expiringTenants.constructor === Array) {
            data["expiringTenants"] = [];
            for (let item of this.expiringTenants)
                data["expiringTenants"].push(item.toJSON());
        }
        if (this.recentTenants && this.recentTenants.constructor === Array) {
            data["recentTenants"] = [];
            for (let item of this.recentTenants)
                data["recentTenants"].push(item.toJSON());
        }
        data["maxExpiringTenantsShownCount"] = this.maxExpiringTenantsShownCount;
        data["maxRecentTenantsShownCount"] = this.maxRecentTenantsShownCount;
        data["subscriptionEndAlertDayCount"] = this.subscriptionEndAlertDayCount;
        data["recentTenantsDayCount"] = this.recentTenantsDayCount;
        data["subscriptionEndDateStart"] = this.subscriptionEndDateStart ? this.subscriptionEndDateStart.toISOString() : <any>undefined;
        data["subscriptionEndDateEnd"] = this.subscriptionEndDateEnd ? this.subscriptionEndDateEnd.toISOString() : <any>undefined;
        data["tenantCreationStartDate"] = this.tenantCreationStartDate ? this.tenantCreationStartDate.toISOString() : <any>undefined;
        return data;
    }
}

export interface IHostDashboardData {
    newTenantsCount: number | undefined;
    newSubscriptionAmount: number | undefined;
    dashboardPlaceholder1: number | undefined;
    dashboardPlaceholder2: number | undefined;
    incomeStatistics: IncomeStastistic[] | undefined;
    editionStatistics: TenantEdition[] | undefined;
    expiringTenants: ExpiringTenant[] | undefined;
    recentTenants: RecentTenant[] | undefined;
    maxExpiringTenantsShownCount: number | undefined;
    maxRecentTenantsShownCount: number | undefined;
    subscriptionEndAlertDayCount: number | undefined;
    recentTenantsDayCount: number | undefined;
    subscriptionEndDateStart: moment.Moment | undefined;
    subscriptionEndDateEnd: moment.Moment | undefined;
    tenantCreationStartDate: moment.Moment | undefined;
}

export class IncomeStastistic implements IIncomeStastistic {
    label!: string | undefined;
    date!: moment.Moment | undefined;
    amount!: number | undefined;

    constructor(data?: IIncomeStastistic) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.label = data["label"];
            this.date = data["date"] ? moment(data["date"].toString()) : <any>undefined;
            this.amount = data["amount"];
        }
    }

    static fromJS(data: any): IncomeStastistic {
        data = typeof data === 'object' ? data : {};
        let result = new IncomeStastistic();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["label"] = this.label;
        data["date"] = this.date ? this.date.toISOString() : <any>undefined;
        data["amount"] = this.amount;
        return data;
    }
}

export interface IIncomeStastistic {
    label: string | undefined;
    date: moment.Moment | undefined;
    amount: number | undefined;
}

export class TenantEdition implements ITenantEdition {
    label!: string | undefined;
    value!: number | undefined;

    constructor(data?: ITenantEdition) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.label = data["label"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): TenantEdition {
        data = typeof data === 'object' ? data : {};
        let result = new TenantEdition();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["label"] = this.label;
        data["value"] = this.value;
        return data;
    }
}

export interface ITenantEdition {
    label: string | undefined;
    value: number | undefined;
}

export class ExpiringTenant implements IExpiringTenant {
    tenantName!: string | undefined;
    remainingDayCount!: number | undefined;

    constructor(data?: IExpiringTenant) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantName = data["tenantName"];
            this.remainingDayCount = data["remainingDayCount"];
        }
    }

    static fromJS(data: any): ExpiringTenant {
        data = typeof data === 'object' ? data : {};
        let result = new ExpiringTenant();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantName"] = this.tenantName;
        data["remainingDayCount"] = this.remainingDayCount;
        return data;
    }
}

export interface IExpiringTenant {
    tenantName: string | undefined;
    remainingDayCount: number | undefined;
}

export class RecentTenant implements IRecentTenant {
    id!: number | undefined;
    name!: string | undefined;
    creationTime!: moment.Moment | undefined;

    constructor(data?: IRecentTenant) {
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
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
        }
    }

    static fromJS(data: any): RecentTenant {
        data = typeof data === 'object' ? data : {};
        let result = new RecentTenant();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        return data;
    }
}

export interface IRecentTenant {
    id: number | undefined;
    name: string | undefined;
    creationTime: moment.Moment | undefined;
}

export class GetIncomeStatisticsDataOutput implements IGetIncomeStatisticsDataOutput {
    incomeStatistics!: IncomeStastistic[] | undefined;

    constructor(data?: IGetIncomeStatisticsDataOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["incomeStatistics"] && data["incomeStatistics"].constructor === Array) {
                this.incomeStatistics = [];
                for (let item of data["incomeStatistics"])
                    this.incomeStatistics.push(IncomeStastistic.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetIncomeStatisticsDataOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetIncomeStatisticsDataOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.incomeStatistics && this.incomeStatistics.constructor === Array) {
            data["incomeStatistics"] = [];
            for (let item of this.incomeStatistics)
                data["incomeStatistics"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetIncomeStatisticsDataOutput {
    incomeStatistics: IncomeStastistic[] | undefined;
}

export class GetEditionTenantStatisticsOutput implements IGetEditionTenantStatisticsOutput {
    editionStatistics!: TenantEdition[] | undefined;

    constructor(data?: IGetEditionTenantStatisticsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["editionStatistics"] && data["editionStatistics"].constructor === Array) {
                this.editionStatistics = [];
                for (let item of data["editionStatistics"])
                    this.editionStatistics.push(TenantEdition.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetEditionTenantStatisticsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetEditionTenantStatisticsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.editionStatistics && this.editionStatistics.constructor === Array) {
            data["editionStatistics"] = [];
            for (let item of this.editionStatistics)
                data["editionStatistics"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetEditionTenantStatisticsOutput {
    editionStatistics: TenantEdition[] | undefined;
}

export class HostSettingsEditDto implements IHostSettingsEditDto {
    general!: GeneralSettingsEditDto;
    userManagement!: HostUserManagementSettingsEditDto;
    email!: EmailSettingsEditDto;
    tenantManagement!: TenantManagementSettingsEditDto;
    security!: SecuritySettingsEditDto;
    billing!: HostBillingSettingsEditDto | undefined;

    constructor(data?: IHostSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.general = new GeneralSettingsEditDto();
            this.userManagement = new HostUserManagementSettingsEditDto();
            this.email = new EmailSettingsEditDto();
            this.tenantManagement = new TenantManagementSettingsEditDto();
            this.security = new SecuritySettingsEditDto();
        }
    }

    init(data?: any) {
        if (data) {
            this.general = data["general"] ? GeneralSettingsEditDto.fromJS(data["general"]) : new GeneralSettingsEditDto();
            this.userManagement = data["userManagement"] ? HostUserManagementSettingsEditDto.fromJS(data["userManagement"]) : new HostUserManagementSettingsEditDto();
            this.email = data["email"] ? EmailSettingsEditDto.fromJS(data["email"]) : new EmailSettingsEditDto();
            this.tenantManagement = data["tenantManagement"] ? TenantManagementSettingsEditDto.fromJS(data["tenantManagement"]) : new TenantManagementSettingsEditDto();
            this.security = data["security"] ? SecuritySettingsEditDto.fromJS(data["security"]) : new SecuritySettingsEditDto();
            this.billing = data["billing"] ? HostBillingSettingsEditDto.fromJS(data["billing"]) : <any>undefined;
        }
    }

    static fromJS(data: any): HostSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new HostSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["general"] = this.general ? this.general.toJSON() : <any>undefined;
        data["userManagement"] = this.userManagement ? this.userManagement.toJSON() : <any>undefined;
        data["email"] = this.email ? this.email.toJSON() : <any>undefined;
        data["tenantManagement"] = this.tenantManagement ? this.tenantManagement.toJSON() : <any>undefined;
        data["security"] = this.security ? this.security.toJSON() : <any>undefined;
        data["billing"] = this.billing ? this.billing.toJSON() : <any>undefined;
        return data;
    }
}

export interface IHostSettingsEditDto {
    general: GeneralSettingsEditDto;
    userManagement: HostUserManagementSettingsEditDto;
    email: EmailSettingsEditDto;
    tenantManagement: TenantManagementSettingsEditDto;
    security: SecuritySettingsEditDto;
    billing: HostBillingSettingsEditDto | undefined;
}

export class GeneralSettingsEditDto implements IGeneralSettingsEditDto {
    timezone!: string | undefined;
    timezoneForComparison!: string | undefined;

    constructor(data?: IGeneralSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.timezone = data["timezone"];
            this.timezoneForComparison = data["timezoneForComparison"];
        }
    }

    static fromJS(data: any): GeneralSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new GeneralSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["timezone"] = this.timezone;
        data["timezoneForComparison"] = this.timezoneForComparison;
        return data;
    }
}

export interface IGeneralSettingsEditDto {
    timezone: string | undefined;
    timezoneForComparison: string | undefined;
}

export class HostUserManagementSettingsEditDto implements IHostUserManagementSettingsEditDto {
    isEmailConfirmationRequiredForLogin!: boolean | undefined;
    smsVerificationEnabled!: boolean | undefined;

    constructor(data?: IHostUserManagementSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.isEmailConfirmationRequiredForLogin = data["isEmailConfirmationRequiredForLogin"];
            this.smsVerificationEnabled = data["smsVerificationEnabled"];
        }
    }

    static fromJS(data: any): HostUserManagementSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new HostUserManagementSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isEmailConfirmationRequiredForLogin"] = this.isEmailConfirmationRequiredForLogin;
        data["smsVerificationEnabled"] = this.smsVerificationEnabled;
        return data;
    }
}

export interface IHostUserManagementSettingsEditDto {
    isEmailConfirmationRequiredForLogin: boolean | undefined;
    smsVerificationEnabled: boolean | undefined;
}

export class EmailSettingsEditDto implements IEmailSettingsEditDto {
    defaultFromAddress!: string | undefined;
    defaultFromDisplayName!: string | undefined;
    smtpHost!: string | undefined;
    smtpPort!: number | undefined;
    smtpUserName!: string | undefined;
    smtpPassword!: string | undefined;
    smtpDomain!: string | undefined;
    smtpEnableSsl!: boolean | undefined;
    smtpUseDefaultCredentials!: boolean | undefined;

    constructor(data?: IEmailSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.defaultFromAddress = data["defaultFromAddress"];
            this.defaultFromDisplayName = data["defaultFromDisplayName"];
            this.smtpHost = data["smtpHost"];
            this.smtpPort = data["smtpPort"];
            this.smtpUserName = data["smtpUserName"];
            this.smtpPassword = data["smtpPassword"];
            this.smtpDomain = data["smtpDomain"];
            this.smtpEnableSsl = data["smtpEnableSsl"];
            this.smtpUseDefaultCredentials = data["smtpUseDefaultCredentials"];
        }
    }

    static fromJS(data: any): EmailSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new EmailSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["defaultFromAddress"] = this.defaultFromAddress;
        data["defaultFromDisplayName"] = this.defaultFromDisplayName;
        data["smtpHost"] = this.smtpHost;
        data["smtpPort"] = this.smtpPort;
        data["smtpUserName"] = this.smtpUserName;
        data["smtpPassword"] = this.smtpPassword;
        data["smtpDomain"] = this.smtpDomain;
        data["smtpEnableSsl"] = this.smtpEnableSsl;
        data["smtpUseDefaultCredentials"] = this.smtpUseDefaultCredentials;
        return data;
    }
}

export interface IEmailSettingsEditDto {
    defaultFromAddress: string | undefined;
    defaultFromDisplayName: string | undefined;
    smtpHost: string | undefined;
    smtpPort: number | undefined;
    smtpUserName: string | undefined;
    smtpPassword: string | undefined;
    smtpDomain: string | undefined;
    smtpEnableSsl: boolean | undefined;
    smtpUseDefaultCredentials: boolean | undefined;
}

export class TenantManagementSettingsEditDto implements ITenantManagementSettingsEditDto {
    allowSelfRegistration!: boolean | undefined;
    isNewRegisteredTenantActiveByDefault!: boolean | undefined;
    useCaptchaOnRegistration!: boolean | undefined;
    defaultEditionId!: number | undefined;

    constructor(data?: ITenantManagementSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.allowSelfRegistration = data["allowSelfRegistration"];
            this.isNewRegisteredTenantActiveByDefault = data["isNewRegisteredTenantActiveByDefault"];
            this.useCaptchaOnRegistration = data["useCaptchaOnRegistration"];
            this.defaultEditionId = data["defaultEditionId"];
        }
    }

    static fromJS(data: any): TenantManagementSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantManagementSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["allowSelfRegistration"] = this.allowSelfRegistration;
        data["isNewRegisteredTenantActiveByDefault"] = this.isNewRegisteredTenantActiveByDefault;
        data["useCaptchaOnRegistration"] = this.useCaptchaOnRegistration;
        data["defaultEditionId"] = this.defaultEditionId;
        return data;
    }
}

export interface ITenantManagementSettingsEditDto {
    allowSelfRegistration: boolean | undefined;
    isNewRegisteredTenantActiveByDefault: boolean | undefined;
    useCaptchaOnRegistration: boolean | undefined;
    defaultEditionId: number | undefined;
}

export class SecuritySettingsEditDto implements ISecuritySettingsEditDto {
    useDefaultPasswordComplexitySettings!: boolean | undefined;
    passwordComplexity!: PasswordComplexitySetting | undefined;
    defaultPasswordComplexity!: PasswordComplexitySetting | undefined;
    userLockOut!: UserLockOutSettingsEditDto | undefined;
    twoFactorLogin!: TwoFactorLoginSettingsEditDto | undefined;

    constructor(data?: ISecuritySettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.useDefaultPasswordComplexitySettings = data["useDefaultPasswordComplexitySettings"];
            this.passwordComplexity = data["passwordComplexity"] ? PasswordComplexitySetting.fromJS(data["passwordComplexity"]) : <any>undefined;
            this.defaultPasswordComplexity = data["defaultPasswordComplexity"] ? PasswordComplexitySetting.fromJS(data["defaultPasswordComplexity"]) : <any>undefined;
            this.userLockOut = data["userLockOut"] ? UserLockOutSettingsEditDto.fromJS(data["userLockOut"]) : <any>undefined;
            this.twoFactorLogin = data["twoFactorLogin"] ? TwoFactorLoginSettingsEditDto.fromJS(data["twoFactorLogin"]) : <any>undefined;
        }
    }

    static fromJS(data: any): SecuritySettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new SecuritySettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["useDefaultPasswordComplexitySettings"] = this.useDefaultPasswordComplexitySettings;
        data["passwordComplexity"] = this.passwordComplexity ? this.passwordComplexity.toJSON() : <any>undefined;
        data["defaultPasswordComplexity"] = this.defaultPasswordComplexity ? this.defaultPasswordComplexity.toJSON() : <any>undefined;
        data["userLockOut"] = this.userLockOut ? this.userLockOut.toJSON() : <any>undefined;
        data["twoFactorLogin"] = this.twoFactorLogin ? this.twoFactorLogin.toJSON() : <any>undefined;
        return data;
    }
}

export interface ISecuritySettingsEditDto {
    useDefaultPasswordComplexitySettings: boolean | undefined;
    passwordComplexity: PasswordComplexitySetting | undefined;
    defaultPasswordComplexity: PasswordComplexitySetting | undefined;
    userLockOut: UserLockOutSettingsEditDto | undefined;
    twoFactorLogin: TwoFactorLoginSettingsEditDto | undefined;
}

export class HostBillingSettingsEditDto implements IHostBillingSettingsEditDto {
    legalName!: string | undefined;
    address!: string | undefined;

    constructor(data?: IHostBillingSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.legalName = data["legalName"];
            this.address = data["address"];
        }
    }

    static fromJS(data: any): HostBillingSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new HostBillingSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["legalName"] = this.legalName;
        data["address"] = this.address;
        return data;
    }
}

export interface IHostBillingSettingsEditDto {
    legalName: string | undefined;
    address: string | undefined;
}

export class PasswordComplexitySetting implements IPasswordComplexitySetting {
    requireDigit!: boolean | undefined;
    requireLowercase!: boolean | undefined;
    requireNonAlphanumeric!: boolean | undefined;
    requireUppercase!: boolean | undefined;
    requiredLength!: number | undefined;

    constructor(data?: IPasswordComplexitySetting) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.requireDigit = data["requireDigit"];
            this.requireLowercase = data["requireLowercase"];
            this.requireNonAlphanumeric = data["requireNonAlphanumeric"];
            this.requireUppercase = data["requireUppercase"];
            this.requiredLength = data["requiredLength"];
        }
    }

    static fromJS(data: any): PasswordComplexitySetting {
        data = typeof data === 'object' ? data : {};
        let result = new PasswordComplexitySetting();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["requireDigit"] = this.requireDigit;
        data["requireLowercase"] = this.requireLowercase;
        data["requireNonAlphanumeric"] = this.requireNonAlphanumeric;
        data["requireUppercase"] = this.requireUppercase;
        data["requiredLength"] = this.requiredLength;
        return data;
    }
}

export interface IPasswordComplexitySetting {
    requireDigit: boolean | undefined;
    requireLowercase: boolean | undefined;
    requireNonAlphanumeric: boolean | undefined;
    requireUppercase: boolean | undefined;
    requiredLength: number | undefined;
}

export class UserLockOutSettingsEditDto implements IUserLockOutSettingsEditDto {
    isEnabled!: boolean | undefined;
    maxFailedAccessAttemptsBeforeLockout!: number | undefined;
    defaultAccountLockoutSeconds!: number | undefined;

    constructor(data?: IUserLockOutSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.isEnabled = data["isEnabled"];
            this.maxFailedAccessAttemptsBeforeLockout = data["maxFailedAccessAttemptsBeforeLockout"];
            this.defaultAccountLockoutSeconds = data["defaultAccountLockoutSeconds"];
        }
    }

    static fromJS(data: any): UserLockOutSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserLockOutSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isEnabled"] = this.isEnabled;
        data["maxFailedAccessAttemptsBeforeLockout"] = this.maxFailedAccessAttemptsBeforeLockout;
        data["defaultAccountLockoutSeconds"] = this.defaultAccountLockoutSeconds;
        return data;
    }
}

export interface IUserLockOutSettingsEditDto {
    isEnabled: boolean | undefined;
    maxFailedAccessAttemptsBeforeLockout: number | undefined;
    defaultAccountLockoutSeconds: number | undefined;
}

export class TwoFactorLoginSettingsEditDto implements ITwoFactorLoginSettingsEditDto {
    isEnabledForApplication!: boolean | undefined;
    isEnabled!: boolean | undefined;
    isEmailProviderEnabled!: boolean | undefined;
    isSmsProviderEnabled!: boolean | undefined;
    isRememberBrowserEnabled!: boolean | undefined;
    isGoogleAuthenticatorEnabled!: boolean | undefined;

    constructor(data?: ITwoFactorLoginSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.isEnabledForApplication = data["isEnabledForApplication"];
            this.isEnabled = data["isEnabled"];
            this.isEmailProviderEnabled = data["isEmailProviderEnabled"];
            this.isSmsProviderEnabled = data["isSmsProviderEnabled"];
            this.isRememberBrowserEnabled = data["isRememberBrowserEnabled"];
            this.isGoogleAuthenticatorEnabled = data["isGoogleAuthenticatorEnabled"];
        }
    }

    static fromJS(data: any): TwoFactorLoginSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new TwoFactorLoginSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isEnabledForApplication"] = this.isEnabledForApplication;
        data["isEnabled"] = this.isEnabled;
        data["isEmailProviderEnabled"] = this.isEmailProviderEnabled;
        data["isSmsProviderEnabled"] = this.isSmsProviderEnabled;
        data["isRememberBrowserEnabled"] = this.isRememberBrowserEnabled;
        data["isGoogleAuthenticatorEnabled"] = this.isGoogleAuthenticatorEnabled;
        return data;
    }
}

export interface ITwoFactorLoginSettingsEditDto {
    isEnabledForApplication: boolean | undefined;
    isEnabled: boolean | undefined;
    isEmailProviderEnabled: boolean | undefined;
    isSmsProviderEnabled: boolean | undefined;
    isRememberBrowserEnabled: boolean | undefined;
    isGoogleAuthenticatorEnabled: boolean | undefined;
}

export class SendTestEmailInput implements ISendTestEmailInput {
    emailAddress!: string;

    constructor(data?: ISendTestEmailInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.emailAddress = data["emailAddress"];
        }
    }

    static fromJS(data: any): SendTestEmailInput {
        data = typeof data === 'object' ? data : {};
        let result = new SendTestEmailInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["emailAddress"] = this.emailAddress;
        return data;
    }
}

export interface ISendTestEmailInput {
    emailAddress: string;
}

export class InstallDto implements IInstallDto {
    connectionString!: string;
    adminPassword!: string;
    webSiteUrl!: string;
    serverUrl!: string | undefined;
    defaultLanguage!: string;
    smtpSettings!: EmailSettingsEditDto | undefined;
    billInfo!: HostBillingSettingsEditDto | undefined;

    constructor(data?: IInstallDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.connectionString = data["connectionString"];
            this.adminPassword = data["adminPassword"];
            this.webSiteUrl = data["webSiteUrl"];
            this.serverUrl = data["serverUrl"];
            this.defaultLanguage = data["defaultLanguage"];
            this.smtpSettings = data["smtpSettings"] ? EmailSettingsEditDto.fromJS(data["smtpSettings"]) : <any>undefined;
            this.billInfo = data["billInfo"] ? HostBillingSettingsEditDto.fromJS(data["billInfo"]) : <any>undefined;
        }
    }

    static fromJS(data: any): InstallDto {
        data = typeof data === 'object' ? data : {};
        let result = new InstallDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["connectionString"] = this.connectionString;
        data["adminPassword"] = this.adminPassword;
        data["webSiteUrl"] = this.webSiteUrl;
        data["serverUrl"] = this.serverUrl;
        data["defaultLanguage"] = this.defaultLanguage;
        data["smtpSettings"] = this.smtpSettings ? this.smtpSettings.toJSON() : <any>undefined;
        data["billInfo"] = this.billInfo ? this.billInfo.toJSON() : <any>undefined;
        return data;
    }
}

export interface IInstallDto {
    connectionString: string;
    adminPassword: string;
    webSiteUrl: string;
    serverUrl: string | undefined;
    defaultLanguage: string;
    smtpSettings: EmailSettingsEditDto | undefined;
    billInfo: HostBillingSettingsEditDto | undefined;
}

export class AppSettingsJsonDto implements IAppSettingsJsonDto {
    webSiteUrl!: string | undefined;
    serverSiteUrl!: string | undefined;
    languages!: NameValue[] | undefined;

    constructor(data?: IAppSettingsJsonDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.webSiteUrl = data["webSiteUrl"];
            this.serverSiteUrl = data["serverSiteUrl"];
            if (data["languages"] && data["languages"].constructor === Array) {
                this.languages = [];
                for (let item of data["languages"])
                    this.languages.push(NameValue.fromJS(item));
            }
        }
    }

    static fromJS(data: any): AppSettingsJsonDto {
        data = typeof data === 'object' ? data : {};
        let result = new AppSettingsJsonDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["webSiteUrl"] = this.webSiteUrl;
        data["serverSiteUrl"] = this.serverSiteUrl;
        if (this.languages && this.languages.constructor === Array) {
            data["languages"] = [];
            for (let item of this.languages)
                data["languages"].push(item.toJSON());
        }
        return data;
    }
}

export interface IAppSettingsJsonDto {
    webSiteUrl: string | undefined;
    serverSiteUrl: string | undefined;
    languages: NameValue[] | undefined;
}

export class NameValue implements INameValue {
    name!: string | undefined;
    value!: string | undefined;

    constructor(data?: INameValue) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): NameValue {
        data = typeof data === 'object' ? data : {};
        let result = new NameValue();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["value"] = this.value;
        return data;
    }
}

export interface INameValue {
    name: string | undefined;
    value: string | undefined;
}

export class CheckDatabaseOutput implements ICheckDatabaseOutput {
    isDatabaseExist!: boolean | undefined;

    constructor(data?: ICheckDatabaseOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.isDatabaseExist = data["isDatabaseExist"];
        }
    }

    static fromJS(data: any): CheckDatabaseOutput {
        data = typeof data === 'object' ? data : {};
        let result = new CheckDatabaseOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isDatabaseExist"] = this.isDatabaseExist;
        return data;
    }
}

export interface ICheckDatabaseOutput {
    isDatabaseExist: boolean | undefined;
}

export class InvoiceDto implements IInvoiceDto {
    amount!: number | undefined;
    editionDisplayName!: string | undefined;
    invoiceNo!: string | undefined;
    invoiceDate!: moment.Moment | undefined;
    tenantLegalName!: string | undefined;
    tenantAddress!: string[] | undefined;
    tenantTaxNo!: string | undefined;
    hostLegalName!: string | undefined;
    hostAddress!: string[] | undefined;

    constructor(data?: IInvoiceDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.amount = data["amount"];
            this.editionDisplayName = data["editionDisplayName"];
            this.invoiceNo = data["invoiceNo"];
            this.invoiceDate = data["invoiceDate"] ? moment(data["invoiceDate"].toString()) : <any>undefined;
            this.tenantLegalName = data["tenantLegalName"];
            if (data["tenantAddress"] && data["tenantAddress"].constructor === Array) {
                this.tenantAddress = [];
                for (let item of data["tenantAddress"])
                    this.tenantAddress.push(item);
            }
            this.tenantTaxNo = data["tenantTaxNo"];
            this.hostLegalName = data["hostLegalName"];
            if (data["hostAddress"] && data["hostAddress"].constructor === Array) {
                this.hostAddress = [];
                for (let item of data["hostAddress"])
                    this.hostAddress.push(item);
            }
        }
    }

    static fromJS(data: any): InvoiceDto {
        data = typeof data === 'object' ? data : {};
        let result = new InvoiceDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["amount"] = this.amount;
        data["editionDisplayName"] = this.editionDisplayName;
        data["invoiceNo"] = this.invoiceNo;
        data["invoiceDate"] = this.invoiceDate ? this.invoiceDate.toISOString() : <any>undefined;
        data["tenantLegalName"] = this.tenantLegalName;
        if (this.tenantAddress && this.tenantAddress.constructor === Array) {
            data["tenantAddress"] = [];
            for (let item of this.tenantAddress)
                data["tenantAddress"].push(item);
        }
        data["tenantTaxNo"] = this.tenantTaxNo;
        data["hostLegalName"] = this.hostLegalName;
        if (this.hostAddress && this.hostAddress.constructor === Array) {
            data["hostAddress"] = [];
            for (let item of this.hostAddress)
                data["hostAddress"].push(item);
        }
        return data;
    }
}

export interface IInvoiceDto {
    amount: number | undefined;
    editionDisplayName: string | undefined;
    invoiceNo: string | undefined;
    invoiceDate: moment.Moment | undefined;
    tenantLegalName: string | undefined;
    tenantAddress: string[] | undefined;
    tenantTaxNo: string | undefined;
    hostLegalName: string | undefined;
    hostAddress: string[] | undefined;
}

export class CreateInvoiceDto implements ICreateInvoiceDto {
    subscriptionPaymentId!: number | undefined;

    constructor(data?: ICreateInvoiceDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.subscriptionPaymentId = data["subscriptionPaymentId"];
        }
    }

    static fromJS(data: any): CreateInvoiceDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateInvoiceDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["subscriptionPaymentId"] = this.subscriptionPaymentId;
        return data;
    }
}

export interface ICreateInvoiceDto {
    subscriptionPaymentId: number | undefined;
}

export class GetLanguagesOutput implements IGetLanguagesOutput {
    defaultLanguageName!: string | undefined;
    items!: ApplicationLanguageListDto[] | undefined;

    constructor(data?: IGetLanguagesOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.defaultLanguageName = data["defaultLanguageName"];
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(ApplicationLanguageListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetLanguagesOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetLanguagesOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["defaultLanguageName"] = this.defaultLanguageName;
        if (this.items && this.items.constructor === Array) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetLanguagesOutput {
    defaultLanguageName: string | undefined;
    items: ApplicationLanguageListDto[] | undefined;
}

export class ApplicationLanguageListDto implements IApplicationLanguageListDto {
    tenantId!: number | undefined;
    name!: string | undefined;
    displayName!: string | undefined;
    icon!: string | undefined;
    isDisabled!: boolean | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IApplicationLanguageListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.icon = data["icon"];
            this.isDisabled = data["isDisabled"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ApplicationLanguageListDto {
        data = typeof data === 'object' ? data : {};
        let result = new ApplicationLanguageListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["icon"] = this.icon;
        data["isDisabled"] = this.isDisabled;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IApplicationLanguageListDto {
    tenantId: number | undefined;
    name: string | undefined;
    displayName: string | undefined;
    icon: string | undefined;
    isDisabled: boolean | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class GetLanguageForEditOutput implements IGetLanguageForEditOutput {
    language!: ApplicationLanguageEditDto | undefined;
    languageNames!: ComboboxItemDto[] | undefined;
    flags!: ComboboxItemDto[] | undefined;

    constructor(data?: IGetLanguageForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.language = data["language"] ? ApplicationLanguageEditDto.fromJS(data["language"]) : <any>undefined;
            if (data["languageNames"] && data["languageNames"].constructor === Array) {
                this.languageNames = [];
                for (let item of data["languageNames"])
                    this.languageNames.push(ComboboxItemDto.fromJS(item));
            }
            if (data["flags"] && data["flags"].constructor === Array) {
                this.flags = [];
                for (let item of data["flags"])
                    this.flags.push(ComboboxItemDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetLanguageForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetLanguageForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["language"] = this.language ? this.language.toJSON() : <any>undefined;
        if (this.languageNames && this.languageNames.constructor === Array) {
            data["languageNames"] = [];
            for (let item of this.languageNames)
                data["languageNames"].push(item.toJSON());
        }
        if (this.flags && this.flags.constructor === Array) {
            data["flags"] = [];
            for (let item of this.flags)
                data["flags"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetLanguageForEditOutput {
    language: ApplicationLanguageEditDto | undefined;
    languageNames: ComboboxItemDto[] | undefined;
    flags: ComboboxItemDto[] | undefined;
}

export class ApplicationLanguageEditDto implements IApplicationLanguageEditDto {
    id!: number | undefined;
    name!: string;
    icon!: string | undefined;
    isEnabled!: boolean | undefined;

    constructor(data?: IApplicationLanguageEditDto) {
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
            this.icon = data["icon"];
            this.isEnabled = data["isEnabled"];
        }
    }

    static fromJS(data: any): ApplicationLanguageEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new ApplicationLanguageEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["icon"] = this.icon;
        data["isEnabled"] = this.isEnabled;
        return data;
    }
}

export interface IApplicationLanguageEditDto {
    id: number | undefined;
    name: string;
    icon: string | undefined;
    isEnabled: boolean | undefined;
}

export class ComboboxItemDto implements IComboboxItemDto {
    value!: string | undefined;
    displayText!: string | undefined;
    isSelected!: boolean | undefined;

    constructor(data?: IComboboxItemDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.value = data["value"];
            this.displayText = data["displayText"];
            this.isSelected = data["isSelected"];
        }
    }

    static fromJS(data: any): ComboboxItemDto {
        data = typeof data === 'object' ? data : {};
        let result = new ComboboxItemDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["value"] = this.value;
        data["displayText"] = this.displayText;
        data["isSelected"] = this.isSelected;
        return data;
    }
}

export interface IComboboxItemDto {
    value: string | undefined;
    displayText: string | undefined;
    isSelected: boolean | undefined;
}

export class CreateOrUpdateLanguageInput implements ICreateOrUpdateLanguageInput {
    language!: ApplicationLanguageEditDto;

    constructor(data?: ICreateOrUpdateLanguageInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.language = new ApplicationLanguageEditDto();
        }
    }

    init(data?: any) {
        if (data) {
            this.language = data["language"] ? ApplicationLanguageEditDto.fromJS(data["language"]) : new ApplicationLanguageEditDto();
        }
    }

    static fromJS(data: any): CreateOrUpdateLanguageInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrUpdateLanguageInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["language"] = this.language ? this.language.toJSON() : <any>undefined;
        return data;
    }
}

export interface ICreateOrUpdateLanguageInput {
    language: ApplicationLanguageEditDto;
}

export class SetDefaultLanguageInput implements ISetDefaultLanguageInput {
    name!: string;

    constructor(data?: ISetDefaultLanguageInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
        }
    }

    static fromJS(data: any): SetDefaultLanguageInput {
        data = typeof data === 'object' ? data : {};
        let result = new SetDefaultLanguageInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        return data;
    }
}

export interface ISetDefaultLanguageInput {
    name: string;
}

export class PagedResultDtoOfLanguageTextListDto implements IPagedResultDtoOfLanguageTextListDto {
    totalCount!: number | undefined;
    items!: LanguageTextListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfLanguageTextListDto) {
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
                    this.items.push(LanguageTextListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfLanguageTextListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfLanguageTextListDto();
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
}

export interface IPagedResultDtoOfLanguageTextListDto {
    totalCount: number | undefined;
    items: LanguageTextListDto[] | undefined;
}

export class LanguageTextListDto implements ILanguageTextListDto {
    key!: string | undefined;
    baseValue!: string | undefined;
    targetValue!: string | undefined;

    constructor(data?: ILanguageTextListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.key = data["key"];
            this.baseValue = data["baseValue"];
            this.targetValue = data["targetValue"];
        }
    }

    static fromJS(data: any): LanguageTextListDto {
        data = typeof data === 'object' ? data : {};
        let result = new LanguageTextListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["key"] = this.key;
        data["baseValue"] = this.baseValue;
        data["targetValue"] = this.targetValue;
        return data;
    }
}

export interface ILanguageTextListDto {
    key: string | undefined;
    baseValue: string | undefined;
    targetValue: string | undefined;
}

export class UpdateLanguageTextInput implements IUpdateLanguageTextInput {
    languageName!: string;
    sourceName!: string;
    key!: string;
    value!: string;

    constructor(data?: IUpdateLanguageTextInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.languageName = data["languageName"];
            this.sourceName = data["sourceName"];
            this.key = data["key"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): UpdateLanguageTextInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateLanguageTextInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["languageName"] = this.languageName;
        data["sourceName"] = this.sourceName;
        data["key"] = this.key;
        data["value"] = this.value;
        return data;
    }
}

export interface IUpdateLanguageTextInput {
    languageName: string;
    sourceName: string;
    key: string;
    value: string;
}

export class Machine implements IMachine {
    tenantId!: number | undefined;
    id!: string | undefined;
    name!: string | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;

    constructor(data?: IMachine) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.id = data["id"];
            this.name = data["name"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
        }
    }

    static fromJS(data: any): Machine {
        data = typeof data === 'object' ? data : {};
        let result = new Machine();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["id"] = this.id;
        data["name"] = this.name;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        return data;
    }
}

export interface IMachine {
    tenantId: number | undefined;
    id: string | undefined;
    name: string | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
}

export class GetNotificationsOutput implements IGetNotificationsOutput {
    unreadCount!: number | undefined;
    totalCount!: number | undefined;
    items!: UserNotification[] | undefined;

    constructor(data?: IGetNotificationsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.unreadCount = data["unreadCount"];
            this.totalCount = data["totalCount"];
            if (data["items"] && data["items"].constructor === Array) {
                this.items = [];
                for (let item of data["items"])
                    this.items.push(UserNotification.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetNotificationsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetNotificationsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["unreadCount"] = this.unreadCount;
        data["totalCount"] = this.totalCount;
        if (this.items && this.items.constructor === Array) {
            data["items"] = [];
            for (let item of this.items)
                data["items"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetNotificationsOutput {
    unreadCount: number | undefined;
    totalCount: number | undefined;
    items: UserNotification[] | undefined;
}

export class UserNotification implements IUserNotification {
    tenantId!: number | undefined;
    userId!: number | undefined;
    state!: UserNotificationState | undefined;
    notification!: TenantNotification | undefined;
    id!: string | undefined;

    constructor(data?: IUserNotification) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.userId = data["userId"];
            this.state = data["state"];
            this.notification = data["notification"] ? TenantNotification.fromJS(data["notification"]) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): UserNotification {
        data = typeof data === 'object' ? data : {};
        let result = new UserNotification();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["userId"] = this.userId;
        data["state"] = this.state;
        data["notification"] = this.notification ? this.notification.toJSON() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface IUserNotification {
    tenantId: number | undefined;
    userId: number | undefined;
    state: UserNotificationState | undefined;
    notification: TenantNotification | undefined;
    id: string | undefined;
}

export class TenantNotification implements ITenantNotification {
    tenantId!: number | undefined;
    notificationName!: string | undefined;
    data!: NotificationData | undefined;
    entityType!: string | undefined;
    entityTypeName!: string | undefined;
    entityId!: any | undefined;
    severity!: TenantNotificationSeverity | undefined;
    creationTime!: moment.Moment | undefined;
    id!: string | undefined;

    constructor(data?: ITenantNotification) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.notificationName = data["notificationName"];
            this.data = data["data"] ? NotificationData.fromJS(data["data"]) : <any>undefined;
            this.entityType = data["entityType"];
            this.entityTypeName = data["entityTypeName"];
            this.entityId = data["entityId"];
            this.severity = data["severity"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): TenantNotification {
        data = typeof data === 'object' ? data : {};
        let result = new TenantNotification();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["notificationName"] = this.notificationName;
        data["data"] = this.data ? this.data.toJSON() : <any>undefined;
        data["entityType"] = this.entityType;
        data["entityTypeName"] = this.entityTypeName;
        data["entityId"] = this.entityId;
        data["severity"] = this.severity;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface ITenantNotification {
    tenantId: number | undefined;
    notificationName: string | undefined;
    data: NotificationData | undefined;
    entityType: string | undefined;
    entityTypeName: string | undefined;
    entityId: any | undefined;
    severity: TenantNotificationSeverity | undefined;
    creationTime: moment.Moment | undefined;
    id: string | undefined;
}

export class NotificationData implements INotificationData {
    type!: string | undefined;
    properties!: { [key: string]: any; } | undefined;

    constructor(data?: INotificationData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.type = data["type"];
            if (data["properties"]) {
                this.properties = {};
                for (let key in data["properties"]) {
                    if (data["properties"].hasOwnProperty(key))
                        this.properties[key] = data["properties"][key];
                }
            }
        }
    }

    static fromJS(data: any): NotificationData {
        data = typeof data === 'object' ? data : {};
        let result = new NotificationData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["type"] = this.type;
        if (this.properties) {
            data["properties"] = {};
            for (let key in this.properties) {
                if (this.properties.hasOwnProperty(key))
                    data["properties"][key] = this.properties[key];
            }
        }
        return data;
    }
}

export interface INotificationData {
    type: string | undefined;
    properties: { [key: string]: any; } | undefined;
}

export class EntityDtoOfGuid implements IEntityDtoOfGuid {
    id!: string | undefined;

    constructor(data?: IEntityDtoOfGuid) {
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
        }
    }

    static fromJS(data: any): EntityDtoOfGuid {
        data = typeof data === 'object' ? data : {};
        let result = new EntityDtoOfGuid();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        return data;
    }
}

export interface IEntityDtoOfGuid {
    id: string | undefined;
}

export class GetNotificationSettingsOutput implements IGetNotificationSettingsOutput {
    receiveNotifications!: boolean | undefined;
    notifications!: NotificationSubscriptionWithDisplayNameDto[] | undefined;

    constructor(data?: IGetNotificationSettingsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.receiveNotifications = data["receiveNotifications"];
            if (data["notifications"] && data["notifications"].constructor === Array) {
                this.notifications = [];
                for (let item of data["notifications"])
                    this.notifications.push(NotificationSubscriptionWithDisplayNameDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetNotificationSettingsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetNotificationSettingsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["receiveNotifications"] = this.receiveNotifications;
        if (this.notifications && this.notifications.constructor === Array) {
            data["notifications"] = [];
            for (let item of this.notifications)
                data["notifications"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetNotificationSettingsOutput {
    receiveNotifications: boolean | undefined;
    notifications: NotificationSubscriptionWithDisplayNameDto[] | undefined;
}

export class NotificationSubscriptionWithDisplayNameDto implements INotificationSubscriptionWithDisplayNameDto {
    displayName!: string | undefined;
    description!: string | undefined;
    name!: string;
    isSubscribed!: boolean | undefined;

    constructor(data?: INotificationSubscriptionWithDisplayNameDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.displayName = data["displayName"];
            this.description = data["description"];
            this.name = data["name"];
            this.isSubscribed = data["isSubscribed"];
        }
    }

    static fromJS(data: any): NotificationSubscriptionWithDisplayNameDto {
        data = typeof data === 'object' ? data : {};
        let result = new NotificationSubscriptionWithDisplayNameDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["displayName"] = this.displayName;
        data["description"] = this.description;
        data["name"] = this.name;
        data["isSubscribed"] = this.isSubscribed;
        return data;
    }
}

export interface INotificationSubscriptionWithDisplayNameDto {
    displayName: string | undefined;
    description: string | undefined;
    name: string;
    isSubscribed: boolean | undefined;
}

export class UpdateNotificationSettingsInput implements IUpdateNotificationSettingsInput {
    receiveNotifications!: boolean | undefined;
    notifications!: NotificationSubscriptionDto[] | undefined;

    constructor(data?: IUpdateNotificationSettingsInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.receiveNotifications = data["receiveNotifications"];
            if (data["notifications"] && data["notifications"].constructor === Array) {
                this.notifications = [];
                for (let item of data["notifications"])
                    this.notifications.push(NotificationSubscriptionDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): UpdateNotificationSettingsInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateNotificationSettingsInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["receiveNotifications"] = this.receiveNotifications;
        if (this.notifications && this.notifications.constructor === Array) {
            data["notifications"] = [];
            for (let item of this.notifications)
                data["notifications"].push(item.toJSON());
        }
        return data;
    }
}

export interface IUpdateNotificationSettingsInput {
    receiveNotifications: boolean | undefined;
    notifications: NotificationSubscriptionDto[] | undefined;
}

export class NotificationSubscriptionDto implements INotificationSubscriptionDto {
    name!: string;
    isSubscribed!: boolean | undefined;

    constructor(data?: INotificationSubscriptionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.isSubscribed = data["isSubscribed"];
        }
    }

    static fromJS(data: any): NotificationSubscriptionDto {
        data = typeof data === 'object' ? data : {};
        let result = new NotificationSubscriptionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["isSubscribed"] = this.isSubscribed;
        return data;
    }
}

export interface INotificationSubscriptionDto {
    name: string;
    isSubscribed: boolean | undefined;
}

export class ListResultDtoOfOrganizationUnitDto implements IListResultDtoOfOrganizationUnitDto {
    items!: OrganizationUnitDto[] | undefined;

    constructor(data?: IListResultDtoOfOrganizationUnitDto) {
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
                    this.items.push(OrganizationUnitDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfOrganizationUnitDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfOrganizationUnitDto();
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

export interface IListResultDtoOfOrganizationUnitDto {
    items: OrganizationUnitDto[] | undefined;
}

export class OrganizationUnitDto implements IOrganizationUnitDto {
    parentId!: number | undefined;
    code!: string | undefined;
    displayName!: string | undefined;
    memberCount!: number | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IOrganizationUnitDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.parentId = data["parentId"];
            this.code = data["code"];
            this.displayName = data["displayName"];
            this.memberCount = data["memberCount"];
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): OrganizationUnitDto {
        data = typeof data === 'object' ? data : {};
        let result = new OrganizationUnitDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["parentId"] = this.parentId;
        data["code"] = this.code;
        data["displayName"] = this.displayName;
        data["memberCount"] = this.memberCount;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IOrganizationUnitDto {
    parentId: number | undefined;
    code: string | undefined;
    displayName: string | undefined;
    memberCount: number | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class PagedResultDtoOfOrganizationUnitUserListDto implements IPagedResultDtoOfOrganizationUnitUserListDto {
    totalCount!: number | undefined;
    items!: OrganizationUnitUserListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfOrganizationUnitUserListDto) {
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
                    this.items.push(OrganizationUnitUserListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfOrganizationUnitUserListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfOrganizationUnitUserListDto();
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
}

export interface IPagedResultDtoOfOrganizationUnitUserListDto {
    totalCount: number | undefined;
    items: OrganizationUnitUserListDto[] | undefined;
}

export class OrganizationUnitUserListDto implements IOrganizationUnitUserListDto {
    name!: string | undefined;
    surname!: string | undefined;
    userName!: string | undefined;
    emailAddress!: string | undefined;
    profilePictureId!: string | undefined;
    addedTime!: moment.Moment | undefined;
    id!: number | undefined;

    constructor(data?: IOrganizationUnitUserListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.surname = data["surname"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.profilePictureId = data["profilePictureId"];
            this.addedTime = data["addedTime"] ? moment(data["addedTime"].toString()) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): OrganizationUnitUserListDto {
        data = typeof data === 'object' ? data : {};
        let result = new OrganizationUnitUserListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["profilePictureId"] = this.profilePictureId;
        data["addedTime"] = this.addedTime ? this.addedTime.toISOString() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface IOrganizationUnitUserListDto {
    name: string | undefined;
    surname: string | undefined;
    userName: string | undefined;
    emailAddress: string | undefined;
    profilePictureId: string | undefined;
    addedTime: moment.Moment | undefined;
    id: number | undefined;
}

export class CreateOrganizationUnitInput implements ICreateOrganizationUnitInput {
    parentId!: number | undefined;
    displayName!: string;

    constructor(data?: ICreateOrganizationUnitInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.parentId = data["parentId"];
            this.displayName = data["displayName"];
        }
    }

    static fromJS(data: any): CreateOrganizationUnitInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrganizationUnitInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["parentId"] = this.parentId;
        data["displayName"] = this.displayName;
        return data;
    }
}

export interface ICreateOrganizationUnitInput {
    parentId: number | undefined;
    displayName: string;
}

export class UpdateOrganizationUnitInput implements IUpdateOrganizationUnitInput {
    id!: number | undefined;
    displayName!: string;

    constructor(data?: IUpdateOrganizationUnitInput) {
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
            this.displayName = data["displayName"];
        }
    }

    static fromJS(data: any): UpdateOrganizationUnitInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateOrganizationUnitInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["displayName"] = this.displayName;
        return data;
    }
}

export interface IUpdateOrganizationUnitInput {
    id: number | undefined;
    displayName: string;
}

export class MoveOrganizationUnitInput implements IMoveOrganizationUnitInput {
    id!: number | undefined;
    newParentId!: number | undefined;

    constructor(data?: IMoveOrganizationUnitInput) {
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
            this.newParentId = data["newParentId"];
        }
    }

    static fromJS(data: any): MoveOrganizationUnitInput {
        data = typeof data === 'object' ? data : {};
        let result = new MoveOrganizationUnitInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["newParentId"] = this.newParentId;
        return data;
    }
}

export interface IMoveOrganizationUnitInput {
    id: number | undefined;
    newParentId: number | undefined;
}

export class UsersToOrganizationUnitInput implements IUsersToOrganizationUnitInput {
    userIds!: number[] | undefined;
    organizationUnitId!: number | undefined;

    constructor(data?: IUsersToOrganizationUnitInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["userIds"] && data["userIds"].constructor === Array) {
                this.userIds = [];
                for (let item of data["userIds"])
                    this.userIds.push(item);
            }
            this.organizationUnitId = data["organizationUnitId"];
        }
    }

    static fromJS(data: any): UsersToOrganizationUnitInput {
        data = typeof data === 'object' ? data : {};
        let result = new UsersToOrganizationUnitInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.userIds && this.userIds.constructor === Array) {
            data["userIds"] = [];
            for (let item of this.userIds)
                data["userIds"].push(item);
        }
        data["organizationUnitId"] = this.organizationUnitId;
        return data;
    }
}

export interface IUsersToOrganizationUnitInput {
    userIds: number[] | undefined;
    organizationUnitId: number | undefined;
}

export class FindOrganizationUnitUsersInput implements IFindOrganizationUnitUsersInput {
    organizationUnitId!: number | undefined;
    maxResultCount!: number | undefined;
    skipCount!: number | undefined;
    filter!: string | undefined;

    constructor(data?: IFindOrganizationUnitUsersInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.organizationUnitId = data["organizationUnitId"];
            this.maxResultCount = data["maxResultCount"];
            this.skipCount = data["skipCount"];
            this.filter = data["filter"];
        }
    }

    static fromJS(data: any): FindOrganizationUnitUsersInput {
        data = typeof data === 'object' ? data : {};
        let result = new FindOrganizationUnitUsersInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["organizationUnitId"] = this.organizationUnitId;
        data["maxResultCount"] = this.maxResultCount;
        data["skipCount"] = this.skipCount;
        data["filter"] = this.filter;
        return data;
    }
}

export interface IFindOrganizationUnitUsersInput {
    organizationUnitId: number | undefined;
    maxResultCount: number | undefined;
    skipCount: number | undefined;
    filter: string | undefined;
}

export class PaymentInfoDto implements IPaymentInfoDto {
    edition!: EditionSelectDto | undefined;
    additionalPrice!: number | undefined;

    constructor(data?: IPaymentInfoDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.edition = data["edition"] ? EditionSelectDto.fromJS(data["edition"]) : <any>undefined;
            this.additionalPrice = data["additionalPrice"];
        }
    }

    static fromJS(data: any): PaymentInfoDto {
        data = typeof data === 'object' ? data : {};
        let result = new PaymentInfoDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["edition"] = this.edition ? this.edition.toJSON() : <any>undefined;
        data["additionalPrice"] = this.additionalPrice;
        return data;
    }
}

export interface IPaymentInfoDto {
    edition: EditionSelectDto | undefined;
    additionalPrice: number | undefined;
}

export class EditionSelectDto implements IEditionSelectDto {
    id!: number | undefined;
    name!: string | undefined;
    displayName!: string | undefined;
    expiringEditionId!: number | undefined;
    monthlyPrice!: number | undefined;
    annualPrice!: number | undefined;
    trialDayCount!: number | undefined;
    waitingDayAfterExpire!: number | undefined;
    isFree!: boolean | undefined;
    additionalData!: AdditionalData | undefined;

    constructor(data?: IEditionSelectDto) {
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
            this.displayName = data["displayName"];
            this.expiringEditionId = data["expiringEditionId"];
            this.monthlyPrice = data["monthlyPrice"];
            this.annualPrice = data["annualPrice"];
            this.trialDayCount = data["trialDayCount"];
            this.waitingDayAfterExpire = data["waitingDayAfterExpire"];
            this.isFree = data["isFree"];
            this.additionalData = data["additionalData"] ? AdditionalData.fromJS(data["additionalData"]) : <any>undefined;
        }
    }

    static fromJS(data: any): EditionSelectDto {
        data = typeof data === 'object' ? data : {};
        let result = new EditionSelectDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["expiringEditionId"] = this.expiringEditionId;
        data["monthlyPrice"] = this.monthlyPrice;
        data["annualPrice"] = this.annualPrice;
        data["trialDayCount"] = this.trialDayCount;
        data["waitingDayAfterExpire"] = this.waitingDayAfterExpire;
        data["isFree"] = this.isFree;
        data["additionalData"] = this.additionalData ? this.additionalData.toJSON() : <any>undefined;
        return data;
    }
}

export interface IEditionSelectDto {
    id: number | undefined;
    name: string | undefined;
    displayName: string | undefined;
    expiringEditionId: number | undefined;
    monthlyPrice: number | undefined;
    annualPrice: number | undefined;
    trialDayCount: number | undefined;
    waitingDayAfterExpire: number | undefined;
    isFree: boolean | undefined;
    additionalData: AdditionalData | undefined;
}

export class CreatePaymentDto implements ICreatePaymentDto {
    editionId!: number | undefined;
    editionPaymentType!: CreatePaymentDtoEditionPaymentType | undefined;
    paymentPeriodType!: CreatePaymentDtoPaymentPeriodType | undefined;
    subscriptionPaymentGatewayType!: CreatePaymentDtoSubscriptionPaymentGatewayType | undefined;

    constructor(data?: ICreatePaymentDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.editionId = data["editionId"];
            this.editionPaymentType = data["editionPaymentType"];
            this.paymentPeriodType = data["paymentPeriodType"];
            this.subscriptionPaymentGatewayType = data["subscriptionPaymentGatewayType"];
        }
    }

    static fromJS(data: any): CreatePaymentDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreatePaymentDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["editionId"] = this.editionId;
        data["editionPaymentType"] = this.editionPaymentType;
        data["paymentPeriodType"] = this.paymentPeriodType;
        data["subscriptionPaymentGatewayType"] = this.subscriptionPaymentGatewayType;
        return data;
    }
}

export interface ICreatePaymentDto {
    editionId: number | undefined;
    editionPaymentType: CreatePaymentDtoEditionPaymentType | undefined;
    paymentPeriodType: CreatePaymentDtoPaymentPeriodType | undefined;
    subscriptionPaymentGatewayType: CreatePaymentDtoSubscriptionPaymentGatewayType | undefined;
}

export class CancelPaymentDto implements ICancelPaymentDto {
    paymentId!: string | undefined;
    gateway!: CancelPaymentDtoGateway | undefined;

    constructor(data?: ICancelPaymentDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.paymentId = data["paymentId"];
            this.gateway = data["gateway"];
        }
    }

    static fromJS(data: any): CancelPaymentDto {
        data = typeof data === 'object' ? data : {};
        let result = new CancelPaymentDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["paymentId"] = this.paymentId;
        data["gateway"] = this.gateway;
        return data;
    }
}

export interface ICancelPaymentDto {
    paymentId: string | undefined;
    gateway: CancelPaymentDtoGateway | undefined;
}

export class ExecutePaymentDto implements IExecutePaymentDto {
    gateway!: ExecutePaymentDtoGateway | undefined;
    editionPaymentType!: ExecutePaymentDtoEditionPaymentType | undefined;
    editionId!: number | undefined;
    paymentPeriodType!: ExecutePaymentDtoPaymentPeriodType | undefined;
    additionalData!: { [key: string]: string; } | undefined;

    constructor(data?: IExecutePaymentDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.gateway = data["gateway"];
            this.editionPaymentType = data["editionPaymentType"];
            this.editionId = data["editionId"];
            this.paymentPeriodType = data["paymentPeriodType"];
            if (data["additionalData"]) {
                this.additionalData = {};
                for (let key in data["additionalData"]) {
                    if (data["additionalData"].hasOwnProperty(key))
                        this.additionalData[key] = data["additionalData"][key];
                }
            }
        }
    }

    static fromJS(data: any): ExecutePaymentDto {
        data = typeof data === 'object' ? data : {};
        let result = new ExecutePaymentDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["gateway"] = this.gateway;
        data["editionPaymentType"] = this.editionPaymentType;
        data["editionId"] = this.editionId;
        data["paymentPeriodType"] = this.paymentPeriodType;
        if (this.additionalData) {
            data["additionalData"] = {};
            for (let key in this.additionalData) {
                if (this.additionalData.hasOwnProperty(key))
                    data["additionalData"][key] = this.additionalData[key];
            }
        }
        return data;
    }
}

export interface IExecutePaymentDto {
    gateway: ExecutePaymentDtoGateway | undefined;
    editionPaymentType: ExecutePaymentDtoEditionPaymentType | undefined;
    editionId: number | undefined;
    paymentPeriodType: ExecutePaymentDtoPaymentPeriodType | undefined;
    additionalData: { [key: string]: string; } | undefined;
}

export class PagedResultDtoOfSubscriptionPaymentListDto implements IPagedResultDtoOfSubscriptionPaymentListDto {
    totalCount!: number | undefined;
    items!: SubscriptionPaymentListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfSubscriptionPaymentListDto) {
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
                    this.items.push(SubscriptionPaymentListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfSubscriptionPaymentListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfSubscriptionPaymentListDto();
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
}

export interface IPagedResultDtoOfSubscriptionPaymentListDto {
    totalCount: number | undefined;
    items: SubscriptionPaymentListDto[] | undefined;
}

export class SubscriptionPaymentListDto implements ISubscriptionPaymentListDto {
    gateway!: string | undefined;
    amount!: number | undefined;
    editionId!: number | undefined;
    dayCount!: number | undefined;
    paymentPeriodType!: string | undefined;
    paymentId!: string | undefined;
    payerId!: string | undefined;
    status!: string | undefined;
    editionDisplayName!: string | undefined;
    tenantId!: number | undefined;
    invoiceNo!: string | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: ISubscriptionPaymentListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.gateway = data["gateway"];
            this.amount = data["amount"];
            this.editionId = data["editionId"];
            this.dayCount = data["dayCount"];
            this.paymentPeriodType = data["paymentPeriodType"];
            this.paymentId = data["paymentId"];
            this.payerId = data["payerId"];
            this.status = data["status"];
            this.editionDisplayName = data["editionDisplayName"];
            this.tenantId = data["tenantId"];
            this.invoiceNo = data["invoiceNo"];
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): SubscriptionPaymentListDto {
        data = typeof data === 'object' ? data : {};
        let result = new SubscriptionPaymentListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["gateway"] = this.gateway;
        data["amount"] = this.amount;
        data["editionId"] = this.editionId;
        data["dayCount"] = this.dayCount;
        data["paymentPeriodType"] = this.paymentPeriodType;
        data["paymentId"] = this.paymentId;
        data["payerId"] = this.payerId;
        data["status"] = this.status;
        data["editionDisplayName"] = this.editionDisplayName;
        data["tenantId"] = this.tenantId;
        data["invoiceNo"] = this.invoiceNo;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface ISubscriptionPaymentListDto {
    gateway: string | undefined;
    amount: number | undefined;
    editionId: number | undefined;
    dayCount: number | undefined;
    paymentPeriodType: string | undefined;
    paymentId: string | undefined;
    payerId: string | undefined;
    status: string | undefined;
    editionDisplayName: string | undefined;
    tenantId: number | undefined;
    invoiceNo: string | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class ListResultDtoOfFlatPermissionWithLevelDto implements IListResultDtoOfFlatPermissionWithLevelDto {
    items!: FlatPermissionWithLevelDto[] | undefined;

    constructor(data?: IListResultDtoOfFlatPermissionWithLevelDto) {
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
                    this.items.push(FlatPermissionWithLevelDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfFlatPermissionWithLevelDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfFlatPermissionWithLevelDto();
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

export interface IListResultDtoOfFlatPermissionWithLevelDto {
    items: FlatPermissionWithLevelDto[] | undefined;
}

export class FlatPermissionWithLevelDto implements IFlatPermissionWithLevelDto {
    level!: number | undefined;
    parentName!: string | undefined;
    name!: string | undefined;
    displayName!: string | undefined;
    description!: string | undefined;
    isGrantedByDefault!: boolean | undefined;

    constructor(data?: IFlatPermissionWithLevelDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.level = data["level"];
            this.parentName = data["parentName"];
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.description = data["description"];
            this.isGrantedByDefault = data["isGrantedByDefault"];
        }
    }

    static fromJS(data: any): FlatPermissionWithLevelDto {
        data = typeof data === 'object' ? data : {};
        let result = new FlatPermissionWithLevelDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["level"] = this.level;
        data["parentName"] = this.parentName;
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["description"] = this.description;
        data["isGrantedByDefault"] = this.isGrantedByDefault;
        return data;
    }
}

export interface IFlatPermissionWithLevelDto {
    level: number | undefined;
    parentName: string | undefined;
    name: string | undefined;
    displayName: string | undefined;
    description: string | undefined;
    isGrantedByDefault: boolean | undefined;
}

export class PagedResultDtoOfGetPlateCategoryForView implements IPagedResultDtoOfGetPlateCategoryForView {
    totalCount!: number | undefined;
    items!: GetPlateCategoryForView[] | undefined;

    constructor(data?: IPagedResultDtoOfGetPlateCategoryForView) {
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
                    this.items.push(GetPlateCategoryForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetPlateCategoryForView {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetPlateCategoryForView();
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
}

export interface IPagedResultDtoOfGetPlateCategoryForView {
    totalCount: number | undefined;
    items: GetPlateCategoryForView[] | undefined;
}

export class GetPlateCategoryForView implements IGetPlateCategoryForView {
    plateCategory!: PlateCategoryDto | undefined;

    constructor(data?: IGetPlateCategoryForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plateCategory = data["plateCategory"] ? PlateCategoryDto.fromJS(data["plateCategory"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetPlateCategoryForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateCategoryForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plateCategory"] = this.plateCategory ? this.plateCategory.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetPlateCategoryForView {
    plateCategory: PlateCategoryDto | undefined;
}

export class PlateCategoryDto implements IPlateCategoryDto {
    name!: string | undefined;
    desc!: string | undefined;
    plates!: PlateDto[] | undefined;
    id!: number | undefined;

    constructor(data?: IPlateCategoryDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.desc = data["desc"];
            if (data["plates"] && data["plates"].constructor === Array) {
                this.plates = [];
                for (let item of data["plates"])
                    this.plates.push(PlateDto.fromJS(item));
            }
            this.id = data["id"];
        }
    }

    static fromJS(data: any): PlateCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["desc"] = this.desc;
        if (this.plates && this.plates.constructor === Array) {
            data["plates"] = [];
            for (let item of this.plates)
                data["plates"].push(item.toJSON());
        }
        data["id"] = this.id;
        return data;
    }
}

export interface IPlateCategoryDto {
    name: string | undefined;
    desc: string | undefined;
    plates: PlateDto[] | undefined;
    id: number | undefined;
}

export class PlateDto implements IPlateDto {
    name!: string | undefined;
    imageUrl!: string | undefined;
    desc!: string | undefined;
    code!: string | undefined;
    avaiable!: number | undefined;
    color!: string | undefined;
    plateCategoryId!: number | undefined;
    discs!: DiscDto[] | undefined;
    id!: string | undefined;

    constructor(data?: IPlateDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.code = data["code"];
            this.avaiable = data["avaiable"];
            this.color = data["color"];
            this.plateCategoryId = data["plateCategoryId"];
            if (data["discs"] && data["discs"].constructor === Array) {
                this.discs = [];
                for (let item of data["discs"])
                    this.discs.push(DiscDto.fromJS(item));
            }
            this.id = data["id"];
        }
    }

    static fromJS(data: any): PlateDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["code"] = this.code;
        data["avaiable"] = this.avaiable;
        data["color"] = this.color;
        data["plateCategoryId"] = this.plateCategoryId;
        if (this.discs && this.discs.constructor === Array) {
            data["discs"] = [];
            for (let item of this.discs)
                data["discs"].push(item.toJSON());
        }
        data["id"] = this.id;
        return data;
    }
}

export interface IPlateDto {
    name: string | undefined;
    imageUrl: string | undefined;
    desc: string | undefined;
    code: string | undefined;
    avaiable: number | undefined;
    color: string | undefined;
    plateCategoryId: number | undefined;
    discs: DiscDto[] | undefined;
    id: string | undefined;
}

export class GetPlateCategoryForEditOutput implements IGetPlateCategoryForEditOutput {
    plateCategory!: CreateOrEditPlateCategoryDto | undefined;

    constructor(data?: IGetPlateCategoryForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plateCategory = data["plateCategory"] ? CreateOrEditPlateCategoryDto.fromJS(data["plateCategory"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetPlateCategoryForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateCategoryForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plateCategory"] = this.plateCategory ? this.plateCategory.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetPlateCategoryForEditOutput {
    plateCategory: CreateOrEditPlateCategoryDto | undefined;
}

export class CreateOrEditPlateCategoryDto implements ICreateOrEditPlateCategoryDto {
    name!: string;
    desc!: string | undefined;
    id!: number | undefined;

    constructor(data?: ICreateOrEditPlateCategoryDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.desc = data["desc"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditPlateCategoryDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditPlateCategoryDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["desc"] = this.desc;
        data["id"] = this.id;
        return data;
    }
}

export interface ICreateOrEditPlateCategoryDto {
    name: string;
    desc: string | undefined;
    id: number | undefined;
}

export class PagedResultDtoOfGetPlateForView implements IPagedResultDtoOfGetPlateForView {
    totalCount!: number | undefined;
    items!: GetPlateForView[] | undefined;

    constructor(data?: IPagedResultDtoOfGetPlateForView) {
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
                    this.items.push(GetPlateForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetPlateForView {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetPlateForView();
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
}

export interface IPagedResultDtoOfGetPlateForView {
    totalCount: number | undefined;
    items: GetPlateForView[] | undefined;
}

export class GetPlateForView implements IGetPlateForView {
    plate!: PlateDto | undefined;
    plateCategoryName!: string | undefined;

    constructor(data?: IGetPlateForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plate = data["plate"] ? PlateDto.fromJS(data["plate"]) : <any>undefined;
            this.plateCategoryName = data["plateCategoryName"];
        }
    }

    static fromJS(data: any): GetPlateForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plate"] = this.plate ? this.plate.toJSON() : <any>undefined;
        data["plateCategoryName"] = this.plateCategoryName;
        return data;
    }
}

export interface IGetPlateForView {
    plate: PlateDto | undefined;
    plateCategoryName: string | undefined;
}

export class GetPlateForEditOutput implements IGetPlateForEditOutput {
    plate!: CreateOrEditPlateDto | undefined;
    plateCategoryName!: string | undefined;

    constructor(data?: IGetPlateForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plate = data["plate"] ? CreateOrEditPlateDto.fromJS(data["plate"]) : <any>undefined;
            this.plateCategoryName = data["plateCategoryName"];
        }
    }

    static fromJS(data: any): GetPlateForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetPlateForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plate"] = this.plate ? this.plate.toJSON() : <any>undefined;
        data["plateCategoryName"] = this.plateCategoryName;
        return data;
    }
}

export interface IGetPlateForEditOutput {
    plate: CreateOrEditPlateDto | undefined;
    plateCategoryName: string | undefined;
}

export class CreateOrEditPlateDto implements ICreateOrEditPlateDto {
    name!: string | undefined;
    imageUrl!: string | undefined;
    desc!: string | undefined;
    code!: string;
    avaiable!: number | undefined;
    color!: string | undefined;
    plateCategoryId!: number | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditPlateDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.code = data["code"];
            this.avaiable = data["avaiable"];
            this.color = data["color"];
            this.plateCategoryId = data["plateCategoryId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditPlateDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditPlateDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["code"] = this.code;
        data["avaiable"] = this.avaiable;
        data["color"] = this.color;
        data["plateCategoryId"] = this.plateCategoryId;
        data["id"] = this.id;
        return data;
    }
}

export interface ICreateOrEditPlateDto {
    name: string | undefined;
    imageUrl: string | undefined;
    desc: string | undefined;
    code: string;
    avaiable: number | undefined;
    color: string | undefined;
    plateCategoryId: number | undefined;
    id: string | undefined;
}

export class PlateMessage implements IPlateMessage {
    message!: string | undefined;

    constructor(data?: IPlateMessage) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.message = data["message"];
        }
    }

    static fromJS(data: any): PlateMessage {
        data = typeof data === 'object' ? data : {};
        let result = new PlateMessage();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["message"] = this.message;
        return data;
    }
}

export interface IPlateMessage {
    message: string | undefined;
}

export class PagedResultDtoOfPlateCategoryLookupTableDto implements IPagedResultDtoOfPlateCategoryLookupTableDto {
    totalCount!: number | undefined;
    items!: PlateCategoryLookupTableDto[] | undefined;

    constructor(data?: IPagedResultDtoOfPlateCategoryLookupTableDto) {
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
                    this.items.push(PlateCategoryLookupTableDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfPlateCategoryLookupTableDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfPlateCategoryLookupTableDto();
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
}

export interface IPagedResultDtoOfPlateCategoryLookupTableDto {
    totalCount: number | undefined;
    items: PlateCategoryLookupTableDto[] | undefined;
}

export class PlateCategoryLookupTableDto implements IPlateCategoryLookupTableDto {
    id!: number | undefined;
    displayName!: string | undefined;

    constructor(data?: IPlateCategoryLookupTableDto) {
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
            this.displayName = data["displayName"];
        }
    }

    static fromJS(data: any): PlateCategoryLookupTableDto {
        data = typeof data === 'object' ? data : {};
        let result = new PlateCategoryLookupTableDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["displayName"] = this.displayName;
        return data;
    }
}

export interface IPlateCategoryLookupTableDto {
    id: number | undefined;
    displayName: string | undefined;
}

export class ImportResult implements IImportResult {
    errorList!: string | undefined;
    errorCount!: number | undefined;
    successCount!: number | undefined;

    constructor(data?: IImportResult) {
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
        return data;
    }
}

export interface IImportResult {
    errorList: string | undefined;
    errorCount: number | undefined;
    successCount: number | undefined;
}

export class PagedResultDtoOfGetProductForView implements IPagedResultDtoOfGetProductForView {
    totalCount!: number | undefined;
    items!: GetProductForView[] | undefined;

    constructor(data?: IPagedResultDtoOfGetProductForView) {
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
                    this.items.push(GetProductForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetProductForView {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetProductForView();
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
}

export interface IPagedResultDtoOfGetProductForView {
    totalCount: number | undefined;
    items: GetProductForView[] | undefined;
}

export class GetProductForView implements IGetProductForView {
    product!: ProductDto | undefined;
    categoryName!: string | undefined;

    constructor(data?: IGetProductForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.product = data["product"] ? ProductDto.fromJS(data["product"]) : <any>undefined;
            this.categoryName = data["categoryName"];
        }
    }

    static fromJS(data: any): GetProductForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetProductForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        data["categoryName"] = this.categoryName;
        return data;
    }
}

export interface IGetProductForView {
    product: ProductDto | undefined;
    categoryName: string | undefined;
}

export class GetProductForEditOutput implements IGetProductForEditOutput {
    product!: CreateOrEditProductDto | undefined;
    categoryName!: string | undefined;

    constructor(data?: IGetProductForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.product = data["product"] ? CreateOrEditProductDto.fromJS(data["product"]) : <any>undefined;
            this.categoryName = data["categoryName"];
        }
    }

    static fromJS(data: any): GetProductForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetProductForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        data["categoryName"] = this.categoryName;
        return data;
    }
}

export interface IGetProductForEditOutput {
    product: CreateOrEditProductDto | undefined;
    categoryName: string | undefined;
}

export class CreateOrEditProductDto implements ICreateOrEditProductDto {
    name!: string | undefined;
    imageUrl!: string | undefined;
    desc!: string | undefined;
    categoryId!: string | undefined;
    plateId!: string | undefined;
    price!: number | undefined;
    contractorPrice!: number | undefined;
    barcode!: string | undefined;
    sku!: string | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditProductDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.categoryId = data["categoryId"];
            this.plateId = data["plateId"];
            this.price = data["price"];
            this.contractorPrice = data["contractorPrice"];
            this.barcode = data["barcode"];
            this.sku = data["sku"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditProductDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditProductDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["categoryId"] = this.categoryId;
        data["plateId"] = this.plateId;
        data["price"] = this.price;
        data["contractorPrice"] = this.contractorPrice;
        data["barcode"] = this.barcode;
        data["sku"] = this.sku;
        data["id"] = this.id;
        return data;
    }
}

export interface ICreateOrEditProductDto {
    name: string | undefined;
    imageUrl: string | undefined;
    desc: string | undefined;
    categoryId: string | undefined;
    plateId: string | undefined;
    price: number | undefined;
    contractorPrice: number | undefined;
    barcode: string | undefined;
    sku: string | undefined;
    id: string | undefined;
}

export class ProductMessage implements IProductMessage {
    message!: string | undefined;

    constructor(data?: IProductMessage) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.message = data["message"];
        }
    }

    static fromJS(data: any): ProductMessage {
        data = typeof data === 'object' ? data : {};
        let result = new ProductMessage();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["message"] = this.message;
        return data;
    }
}

export interface IProductMessage {
    message: string | undefined;
}

export class ListResultDtoOfPosProductMenuOutput implements IListResultDtoOfPosProductMenuOutput {
    items!: PosProductMenuOutput[] | undefined;

    constructor(data?: IListResultDtoOfPosProductMenuOutput) {
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
                    this.items.push(PosProductMenuOutput.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfPosProductMenuOutput {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfPosProductMenuOutput();
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

export interface IListResultDtoOfPosProductMenuOutput {
    items: PosProductMenuOutput[] | undefined;
}

export class PosProductMenuOutput implements IPosProductMenuOutput {
    productId!: string | undefined;
    plateCode!: string | undefined;
    productName!: string | undefined;
    price!: number | undefined;

    constructor(data?: IPosProductMenuOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.productId = data["productId"];
            this.plateCode = data["plateCode"];
            this.productName = data["productName"];
            this.price = data["price"];
        }
    }

    static fromJS(data: any): PosProductMenuOutput {
        data = typeof data === 'object' ? data : {};
        let result = new PosProductMenuOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["productId"] = this.productId;
        data["plateCode"] = this.plateCode;
        data["productName"] = this.productName;
        data["price"] = this.price;
        return data;
    }
}

export interface IPosProductMenuOutput {
    productId: string | undefined;
    plateCode: string | undefined;
    productName: string | undefined;
    price: number | undefined;
}

export class PagedResultDtoOfProductMenuDto implements IPagedResultDtoOfProductMenuDto {
    totalCount!: number | undefined;
    items!: ProductMenuDto[] | undefined;

    constructor(data?: IPagedResultDtoOfProductMenuDto) {
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
                    this.items.push(ProductMenuDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfProductMenuDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfProductMenuDto();
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
}

export interface IPagedResultDtoOfProductMenuDto {
    totalCount: number | undefined;
    items: ProductMenuDto[] | undefined;
}

export class ProductMenuDto implements IProductMenuDto {
    id!: string | undefined;
    plate!: Plate | undefined;
    plateCode!: string | undefined;
    product!: ProductDto | undefined;
    productId!: string | undefined;
    categoryName!: string | undefined;
    price!: number | undefined;
    selectedDate!: moment.Moment | undefined;
    priceStrategyId!: string | undefined;
    priceStrategy!: number | undefined;
    session!: Session | undefined;
    sessionName!: string | undefined;
    displayOrder!: number | undefined;

    constructor(data?: IProductMenuDto) {
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
            this.plate = data["plate"] ? Plate.fromJS(data["plate"]) : <any>undefined;
            this.plateCode = data["plateCode"];
            this.product = data["product"] ? ProductDto.fromJS(data["product"]) : <any>undefined;
            this.productId = data["productId"];
            this.categoryName = data["categoryName"];
            this.price = data["price"];
            this.selectedDate = data["selectedDate"] ? moment(data["selectedDate"].toString()) : <any>undefined;
            this.priceStrategyId = data["priceStrategyId"];
            this.priceStrategy = data["priceStrategy"];
            this.session = data["session"] ? Session.fromJS(data["session"]) : <any>undefined;
            this.sessionName = data["sessionName"];
            this.displayOrder = data["displayOrder"];
        }
    }

    static fromJS(data: any): ProductMenuDto {
        data = typeof data === 'object' ? data : {};
        let result = new ProductMenuDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["plate"] = this.plate ? this.plate.toJSON() : <any>undefined;
        data["plateCode"] = this.plateCode;
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        data["productId"] = this.productId;
        data["categoryName"] = this.categoryName;
        data["price"] = this.price;
        data["selectedDate"] = this.selectedDate ? this.selectedDate.toISOString() : <any>undefined;
        data["priceStrategyId"] = this.priceStrategyId;
        data["priceStrategy"] = this.priceStrategy;
        data["session"] = this.session ? this.session.toJSON() : <any>undefined;
        data["sessionName"] = this.sessionName;
        data["displayOrder"] = this.displayOrder;
        return data;
    }
}

export interface IProductMenuDto {
    id: string | undefined;
    plate: Plate | undefined;
    plateCode: string | undefined;
    product: ProductDto | undefined;
    productId: string | undefined;
    categoryName: string | undefined;
    price: number | undefined;
    selectedDate: moment.Moment | undefined;
    priceStrategyId: string | undefined;
    priceStrategy: number | undefined;
    session: Session | undefined;
    sessionName: string | undefined;
    displayOrder: number | undefined;
}

export class Plate implements IPlate {
    tenantId!: number | undefined;
    name!: string | undefined;
    imageUrl!: string | undefined;
    desc!: string | undefined;
    code!: string;
    avaiable!: number | undefined;
    color!: string | undefined;
    plateCategoryId!: number | undefined;
    plateCategory!: PlateCategory | undefined;
    discs!: Disc[] | undefined;
    type!: string | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IPlate) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.name = data["name"];
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.code = data["code"];
            this.avaiable = data["avaiable"];
            this.color = data["color"];
            this.plateCategoryId = data["plateCategoryId"];
            this.plateCategory = data["plateCategory"] ? PlateCategory.fromJS(data["plateCategory"]) : <any>undefined;
            if (data["discs"] && data["discs"].constructor === Array) {
                this.discs = [];
                for (let item of data["discs"])
                    this.discs.push(Disc.fromJS(item));
            }
            this.type = data["type"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): Plate {
        data = typeof data === 'object' ? data : {};
        let result = new Plate();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["code"] = this.code;
        data["avaiable"] = this.avaiable;
        data["color"] = this.color;
        data["plateCategoryId"] = this.plateCategoryId;
        data["plateCategory"] = this.plateCategory ? this.plateCategory.toJSON() : <any>undefined;
        if (this.discs && this.discs.constructor === Array) {
            data["discs"] = [];
            for (let item of this.discs)
                data["discs"].push(item.toJSON());
        }
        data["type"] = this.type;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IPlate {
    tenantId: number | undefined;
    name: string | undefined;
    imageUrl: string | undefined;
    desc: string | undefined;
    code: string;
    avaiable: number | undefined;
    color: string | undefined;
    plateCategoryId: number | undefined;
    plateCategory: PlateCategory | undefined;
    discs: Disc[] | undefined;
    type: string | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class Session implements ISession {
    tenantId!: number | undefined;
    name!: string;
    fromHrs!: string | undefined;
    toHrs!: string | undefined;
    activeFlg!: boolean | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: ISession) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.name = data["name"];
            this.fromHrs = data["fromHrs"];
            this.toHrs = data["toHrs"];
            this.activeFlg = data["activeFlg"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): Session {
        data = typeof data === 'object' ? data : {};
        let result = new Session();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["name"] = this.name;
        data["fromHrs"] = this.fromHrs;
        data["toHrs"] = this.toHrs;
        data["activeFlg"] = this.activeFlg;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface ISession {
    tenantId: number | undefined;
    name: string;
    fromHrs: string | undefined;
    toHrs: string | undefined;
    activeFlg: boolean | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class PlateCategory implements IPlateCategory {
    tenantId!: number | undefined;
    name!: string;
    desc!: string | undefined;
    plates!: Plate[] | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IPlateCategory) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.name = data["name"];
            this.desc = data["desc"];
            if (data["plates"] && data["plates"].constructor === Array) {
                this.plates = [];
                for (let item of data["plates"])
                    this.plates.push(Plate.fromJS(item));
            }
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): PlateCategory {
        data = typeof data === 'object' ? data : {};
        let result = new PlateCategory();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["name"] = this.name;
        data["desc"] = this.desc;
        if (this.plates && this.plates.constructor === Array) {
            data["plates"] = [];
            for (let item of this.plates)
                data["plates"].push(item.toJSON());
        }
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IPlateCategory {
    tenantId: number | undefined;
    name: string;
    desc: string | undefined;
    plates: Plate[] | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class Disc implements IDisc {
    tenantId!: number | undefined;
    uid!: string;
    code!: string;
    plateId!: string | undefined;
    plate!: Plate | undefined;
    isSynced!: boolean | undefined;
    syncDate!: moment.Moment | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IDisc) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.uid = data["uid"];
            this.code = data["code"];
            this.plateId = data["plateId"];
            this.plate = data["plate"] ? Plate.fromJS(data["plate"]) : <any>undefined;
            this.isSynced = data["isSynced"];
            this.syncDate = data["syncDate"] ? moment(data["syncDate"].toString()) : <any>undefined;
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): Disc {
        data = typeof data === 'object' ? data : {};
        let result = new Disc();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["uid"] = this.uid;
        data["code"] = this.code;
        data["plateId"] = this.plateId;
        data["plate"] = this.plate ? this.plate.toJSON() : <any>undefined;
        data["isSynced"] = this.isSynced;
        data["syncDate"] = this.syncDate ? this.syncDate.toISOString() : <any>undefined;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IDisc {
    tenantId: number | undefined;
    uid: string;
    code: string;
    plateId: string | undefined;
    plate: Plate | undefined;
    isSynced: boolean | undefined;
    syncDate: moment.Moment | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class ReplicateInput implements IReplicateInput {
    dateFilter!: moment.Moment | undefined;
    sessionId!: string | undefined;
    days!: number | undefined;

    constructor(data?: IReplicateInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.dateFilter = data["dateFilter"] ? moment(data["dateFilter"].toString()) : <any>undefined;
            this.sessionId = data["sessionId"];
            this.days = data["days"];
        }
    }

    static fromJS(data: any): ReplicateInput {
        data = typeof data === 'object' ? data : {};
        let result = new ReplicateInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["dateFilter"] = this.dateFilter ? this.dateFilter.toISOString() : <any>undefined;
        data["sessionId"] = this.sessionId;
        data["days"] = this.days;
        return data;
    }
}

export interface IReplicateInput {
    dateFilter: moment.Moment | undefined;
    sessionId: string | undefined;
    days: number | undefined;
}

export class ProductMenusInput implements IProductMenusInput {
    nameFilter!: string | undefined;
    codeFilter!: string | undefined;
    categoryFilter!: string | undefined;
    skuFilter!: string | undefined;
    dateFilter!: moment.Moment | undefined;
    sessionFilter!: string | undefined;
    id!: string | undefined;
    price!: number | undefined;
    priceStrategyId!: string | undefined;
    priceStrategy!: number | undefined;
    productId!: string | undefined;
    sorting!: string | undefined;
    skipCount!: number | undefined;
    maxResultCount!: number | undefined;

    constructor(data?: IProductMenusInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.nameFilter = data["nameFilter"];
            this.codeFilter = data["codeFilter"];
            this.categoryFilter = data["categoryFilter"];
            this.skuFilter = data["skuFilter"];
            this.dateFilter = data["dateFilter"] ? moment(data["dateFilter"].toString()) : <any>undefined;
            this.sessionFilter = data["sessionFilter"];
            this.id = data["id"];
            this.price = data["price"];
            this.priceStrategyId = data["priceStrategyId"];
            this.priceStrategy = data["priceStrategy"];
            this.productId = data["productId"];
            this.sorting = data["sorting"];
            this.skipCount = data["skipCount"];
            this.maxResultCount = data["maxResultCount"];
        }
    }

    static fromJS(data: any): ProductMenusInput {
        data = typeof data === 'object' ? data : {};
        let result = new ProductMenusInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["nameFilter"] = this.nameFilter;
        data["codeFilter"] = this.codeFilter;
        data["categoryFilter"] = this.categoryFilter;
        data["skuFilter"] = this.skuFilter;
        data["dateFilter"] = this.dateFilter ? this.dateFilter.toISOString() : <any>undefined;
        data["sessionFilter"] = this.sessionFilter;
        data["id"] = this.id;
        data["price"] = this.price;
        data["priceStrategyId"] = this.priceStrategyId;
        data["priceStrategy"] = this.priceStrategy;
        data["productId"] = this.productId;
        data["sorting"] = this.sorting;
        data["skipCount"] = this.skipCount;
        data["maxResultCount"] = this.maxResultCount;
        return data;
    }
}

export interface IProductMenusInput {
    nameFilter: string | undefined;
    codeFilter: string | undefined;
    categoryFilter: string | undefined;
    skuFilter: string | undefined;
    dateFilter: moment.Moment | undefined;
    sessionFilter: string | undefined;
    id: string | undefined;
    price: number | undefined;
    priceStrategyId: string | undefined;
    priceStrategy: number | undefined;
    productId: string | undefined;
    sorting: string | undefined;
    skipCount: number | undefined;
    maxResultCount: number | undefined;
}

export class ImportData implements IImportData {
    productName!: string | undefined;
    barCode!: string | undefined;
    sku!: string | undefined;
    plateCode!: string | undefined;
    price!: string | undefined;
    contractorPrice!: string | undefined;
    selectedDate!: string | undefined;
    sessionName!: string | undefined;

    constructor(data?: IImportData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.productName = data["productName"];
            this.barCode = data["barCode"];
            this.sku = data["sku"];
            this.plateCode = data["plateCode"];
            this.price = data["price"];
            this.contractorPrice = data["contractorPrice"];
            this.selectedDate = data["selectedDate"];
            this.sessionName = data["sessionName"];
        }
    }

    static fromJS(data: any): ImportData {
        data = typeof data === 'object' ? data : {};
        let result = new ImportData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["productName"] = this.productName;
        data["barCode"] = this.barCode;
        data["sku"] = this.sku;
        data["plateCode"] = this.plateCode;
        data["price"] = this.price;
        data["contractorPrice"] = this.contractorPrice;
        data["selectedDate"] = this.selectedDate;
        data["sessionName"] = this.sessionName;
        return data;
    }
}

export interface IImportData {
    productName: string | undefined;
    barCode: string | undefined;
    sku: string | undefined;
    plateCode: string | undefined;
    price: string | undefined;
    contractorPrice: string | undefined;
    selectedDate: string | undefined;
    sessionName: string | undefined;
}

export class PlateMenuDayResult implements IPlateMenuDayResult {
    sessionId!: string | undefined;
    sessionName!: string | undefined;
    totalSetPrice!: number | undefined;
    totalNoPrice!: number | undefined;
    activeFlg!: boolean | undefined;

    constructor(data?: IPlateMenuDayResult) {
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
}

export interface IPlateMenuDayResult {
    sessionId: string | undefined;
    sessionName: string | undefined;
    totalSetPrice: number | undefined;
    totalNoPrice: number | undefined;
    activeFlg: boolean | undefined;
}

export class ProductMenuMonthResult implements IProductMenuMonthResult {
    day!: string | undefined;
    totalSetPrice!: number | undefined;
    totalNoPrice!: number | undefined;

    constructor(data?: IProductMenuMonthResult) {
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

    static fromJS(data: any): ProductMenuMonthResult {
        data = typeof data === 'object' ? data : {};
        let result = new ProductMenuMonthResult();
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
}

export interface IProductMenuMonthResult {
    day: string | undefined;
    totalSetPrice: number | undefined;
    totalNoPrice: number | undefined;
}

export class CurrentUserProfileEditDto implements ICurrentUserProfileEditDto {
    name!: string;
    surname!: string;
    userName!: string;
    emailAddress!: string;
    phoneNumber!: string | undefined;
    isPhoneNumberConfirmed!: boolean | undefined;
    timezone!: string | undefined;
    qrCodeSetupImageUrl!: string | undefined;
    isGoogleAuthenticatorEnabled!: boolean | undefined;

    constructor(data?: ICurrentUserProfileEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.surname = data["surname"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.phoneNumber = data["phoneNumber"];
            this.isPhoneNumberConfirmed = data["isPhoneNumberConfirmed"];
            this.timezone = data["timezone"];
            this.qrCodeSetupImageUrl = data["qrCodeSetupImageUrl"];
            this.isGoogleAuthenticatorEnabled = data["isGoogleAuthenticatorEnabled"];
        }
    }

    static fromJS(data: any): CurrentUserProfileEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new CurrentUserProfileEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["phoneNumber"] = this.phoneNumber;
        data["isPhoneNumberConfirmed"] = this.isPhoneNumberConfirmed;
        data["timezone"] = this.timezone;
        data["qrCodeSetupImageUrl"] = this.qrCodeSetupImageUrl;
        data["isGoogleAuthenticatorEnabled"] = this.isGoogleAuthenticatorEnabled;
        return data;
    }
}

export interface ICurrentUserProfileEditDto {
    name: string;
    surname: string;
    userName: string;
    emailAddress: string;
    phoneNumber: string | undefined;
    isPhoneNumberConfirmed: boolean | undefined;
    timezone: string | undefined;
    qrCodeSetupImageUrl: string | undefined;
    isGoogleAuthenticatorEnabled: boolean | undefined;
}

export class UpdateGoogleAuthenticatorKeyOutput implements IUpdateGoogleAuthenticatorKeyOutput {
    qrCodeSetupImageUrl!: string | undefined;

    constructor(data?: IUpdateGoogleAuthenticatorKeyOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.qrCodeSetupImageUrl = data["qrCodeSetupImageUrl"];
        }
    }

    static fromJS(data: any): UpdateGoogleAuthenticatorKeyOutput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateGoogleAuthenticatorKeyOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["qrCodeSetupImageUrl"] = this.qrCodeSetupImageUrl;
        return data;
    }
}

export interface IUpdateGoogleAuthenticatorKeyOutput {
    qrCodeSetupImageUrl: string | undefined;
}

export class VerifySmsCodeInputDto implements IVerifySmsCodeInputDto {
    code!: string | undefined;

    constructor(data?: IVerifySmsCodeInputDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.code = data["code"];
        }
    }

    static fromJS(data: any): VerifySmsCodeInputDto {
        data = typeof data === 'object' ? data : {};
        let result = new VerifySmsCodeInputDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["code"] = this.code;
        return data;
    }
}

export interface IVerifySmsCodeInputDto {
    code: string | undefined;
}

export class ChangePasswordInput implements IChangePasswordInput {
    currentPassword!: string;
    newPassword!: string;

    constructor(data?: IChangePasswordInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.currentPassword = data["currentPassword"];
            this.newPassword = data["newPassword"];
        }
    }

    static fromJS(data: any): ChangePasswordInput {
        data = typeof data === 'object' ? data : {};
        let result = new ChangePasswordInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["currentPassword"] = this.currentPassword;
        data["newPassword"] = this.newPassword;
        return data;
    }
}

export interface IChangePasswordInput {
    currentPassword: string;
    newPassword: string;
}

export class UpdateProfilePictureInput implements IUpdateProfilePictureInput {
    fileToken!: string;
    x!: number | undefined;
    y!: number | undefined;
    width!: number | undefined;
    height!: number | undefined;

    constructor(data?: IUpdateProfilePictureInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.fileToken = data["fileToken"];
            this.x = data["x"];
            this.y = data["y"];
            this.width = data["width"];
            this.height = data["height"];
        }
    }

    static fromJS(data: any): UpdateProfilePictureInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateProfilePictureInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["fileToken"] = this.fileToken;
        data["x"] = this.x;
        data["y"] = this.y;
        data["width"] = this.width;
        data["height"] = this.height;
        return data;
    }
}

export interface IUpdateProfilePictureInput {
    fileToken: string;
    x: number | undefined;
    y: number | undefined;
    width: number | undefined;
    height: number | undefined;
}

export class GetPasswordComplexitySettingOutput implements IGetPasswordComplexitySettingOutput {
    setting!: PasswordComplexitySetting | undefined;

    constructor(data?: IGetPasswordComplexitySettingOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.setting = data["setting"] ? PasswordComplexitySetting.fromJS(data["setting"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetPasswordComplexitySettingOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetPasswordComplexitySettingOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["setting"] = this.setting ? this.setting.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetPasswordComplexitySettingOutput {
    setting: PasswordComplexitySetting | undefined;
}

export class GetProfilePictureOutput implements IGetProfilePictureOutput {
    profilePicture!: string | undefined;

    constructor(data?: IGetProfilePictureOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.profilePicture = data["profilePicture"];
        }
    }

    static fromJS(data: any): GetProfilePictureOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetProfilePictureOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["profilePicture"] = this.profilePicture;
        return data;
    }
}

export interface IGetProfilePictureOutput {
    profilePicture: string | undefined;
}

export class ChangeUserLanguageDto implements IChangeUserLanguageDto {
    languageName!: string;

    constructor(data?: IChangeUserLanguageDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.languageName = data["languageName"];
        }
    }

    static fromJS(data: any): ChangeUserLanguageDto {
        data = typeof data === 'object' ? data : {};
        let result = new ChangeUserLanguageDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["languageName"] = this.languageName;
        return data;
    }
}

export interface IChangeUserLanguageDto {
    languageName: string;
}

export class SyncedItemDataOfDisc implements ISyncedItemDataOfDisc {
    machineId!: string | undefined;
    syncedItems!: Disc[] | undefined;

    constructor(data?: ISyncedItemDataOfDisc) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machineId = data["machineId"];
            if (data["syncedItems"] && data["syncedItems"].constructor === Array) {
                this.syncedItems = [];
                for (let item of data["syncedItems"])
                    this.syncedItems.push(Disc.fromJS(item));
            }
        }
    }

    static fromJS(data: any): SyncedItemDataOfDisc {
        data = typeof data === 'object' ? data : {};
        let result = new SyncedItemDataOfDisc();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineId"] = this.machineId;
        if (this.syncedItems && this.syncedItems.constructor === Array) {
            data["syncedItems"] = [];
            for (let item of this.syncedItems)
                data["syncedItems"].push(item.toJSON());
        }
        return data;
    }
}

export interface ISyncedItemDataOfDisc {
    machineId: string | undefined;
    syncedItems: Disc[] | undefined;
}

export class SyncedItemDataOfGuid implements ISyncedItemDataOfGuid {
    machineId!: string | undefined;
    syncedItems!: string[] | undefined;

    constructor(data?: ISyncedItemDataOfGuid) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.machineId = data["machineId"];
            if (data["syncedItems"] && data["syncedItems"].constructor === Array) {
                this.syncedItems = [];
                for (let item of data["syncedItems"])
                    this.syncedItems.push(item);
            }
        }
    }

    static fromJS(data: any): SyncedItemDataOfGuid {
        data = typeof data === 'object' ? data : {};
        let result = new SyncedItemDataOfGuid();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["machineId"] = this.machineId;
        if (this.syncedItems && this.syncedItems.constructor === Array) {
            data["syncedItems"] = [];
            for (let item of this.syncedItems)
                data["syncedItems"].push(item);
        }
        return data;
    }
}

export interface ISyncedItemDataOfGuid {
    machineId: string | undefined;
    syncedItems: string[] | undefined;
}

export class ProductMenu implements IProductMenu {
    tenantId!: number | undefined;
    productId!: string | undefined;
    product!: Product | undefined;
    price!: number | undefined;
    priceStrategies!: PriceStrategy[] | undefined;
    sessionId!: string | undefined;
    session!: Session | undefined;
    selectedDate!: moment.Moment | undefined;
    contractorPrice!: number | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IProductMenu) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.productId = data["productId"];
            this.product = data["product"] ? Product.fromJS(data["product"]) : <any>undefined;
            this.price = data["price"];
            if (data["priceStrategies"] && data["priceStrategies"].constructor === Array) {
                this.priceStrategies = [];
                for (let item of data["priceStrategies"])
                    this.priceStrategies.push(PriceStrategy.fromJS(item));
            }
            this.sessionId = data["sessionId"];
            this.session = data["session"] ? Session.fromJS(data["session"]) : <any>undefined;
            this.selectedDate = data["selectedDate"] ? moment(data["selectedDate"].toString()) : <any>undefined;
            this.contractorPrice = data["contractorPrice"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ProductMenu {
        data = typeof data === 'object' ? data : {};
        let result = new ProductMenu();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["productId"] = this.productId;
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        data["price"] = this.price;
        if (this.priceStrategies && this.priceStrategies.constructor === Array) {
            data["priceStrategies"] = [];
            for (let item of this.priceStrategies)
                data["priceStrategies"].push(item.toJSON());
        }
        data["sessionId"] = this.sessionId;
        data["session"] = this.session ? this.session.toJSON() : <any>undefined;
        data["selectedDate"] = this.selectedDate ? this.selectedDate.toISOString() : <any>undefined;
        data["contractorPrice"] = this.contractorPrice;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IProductMenu {
    tenantId: number | undefined;
    productId: string | undefined;
    product: Product | undefined;
    price: number | undefined;
    priceStrategies: PriceStrategy[] | undefined;
    sessionId: string | undefined;
    session: Session | undefined;
    selectedDate: moment.Moment | undefined;
    contractorPrice: number | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class Product implements IProduct {
    sku!: string | undefined;
    name!: string | undefined;
    status!: number | undefined;
    unit!: string | undefined;
    price!: number | undefined;
    contractorPrice!: number | undefined;
    imageUrl!: string | undefined;
    imageChecksum!: string | undefined;
    categories!: ProductCategory[] | undefined;
    tenantId!: number | undefined;
    barcode!: string | undefined;
    fromDate!: moment.Moment | undefined;
    toDate!: moment.Moment | undefined;
    shortDesc1!: string | undefined;
    shortDesc2!: string | undefined;
    shortDesc3!: string | undefined;
    desc!: string | undefined;
    categoryId!: string | undefined;
    plateId!: string | undefined;
    plate!: Plate | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IProduct) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.sku = data["sku"];
            this.name = data["name"];
            this.status = data["status"];
            this.unit = data["unit"];
            this.price = data["price"];
            this.contractorPrice = data["contractorPrice"];
            this.imageUrl = data["imageUrl"];
            this.imageChecksum = data["imageChecksum"];
            if (data["categories"] && data["categories"].constructor === Array) {
                this.categories = [];
                for (let item of data["categories"])
                    this.categories.push(ProductCategory.fromJS(item));
            }
            this.tenantId = data["tenantId"];
            this.barcode = data["barcode"];
            this.fromDate = data["fromDate"] ? moment(data["fromDate"].toString()) : <any>undefined;
            this.toDate = data["toDate"] ? moment(data["toDate"].toString()) : <any>undefined;
            this.shortDesc1 = data["shortDesc1"];
            this.shortDesc2 = data["shortDesc2"];
            this.shortDesc3 = data["shortDesc3"];
            this.desc = data["desc"];
            this.categoryId = data["categoryId"];
            this.plateId = data["plateId"];
            this.plate = data["plate"] ? Plate.fromJS(data["plate"]) : <any>undefined;
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): Product {
        data = typeof data === 'object' ? data : {};
        let result = new Product();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sku"] = this.sku;
        data["name"] = this.name;
        data["status"] = this.status;
        data["unit"] = this.unit;
        data["price"] = this.price;
        data["contractorPrice"] = this.contractorPrice;
        data["imageUrl"] = this.imageUrl;
        data["imageChecksum"] = this.imageChecksum;
        if (this.categories && this.categories.constructor === Array) {
            data["categories"] = [];
            for (let item of this.categories)
                data["categories"].push(item.toJSON());
        }
        data["tenantId"] = this.tenantId;
        data["barcode"] = this.barcode;
        data["fromDate"] = this.fromDate ? this.fromDate.toISOString() : <any>undefined;
        data["toDate"] = this.toDate ? this.toDate.toISOString() : <any>undefined;
        data["shortDesc1"] = this.shortDesc1;
        data["shortDesc2"] = this.shortDesc2;
        data["shortDesc3"] = this.shortDesc3;
        data["desc"] = this.desc;
        data["categoryId"] = this.categoryId;
        data["plateId"] = this.plateId;
        data["plate"] = this.plate ? this.plate.toJSON() : <any>undefined;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IProduct {
    sku: string | undefined;
    name: string | undefined;
    status: number | undefined;
    unit: string | undefined;
    price: number | undefined;
    contractorPrice: number | undefined;
    imageUrl: string | undefined;
    imageChecksum: string | undefined;
    categories: ProductCategory[] | undefined;
    tenantId: number | undefined;
    barcode: string | undefined;
    fromDate: moment.Moment | undefined;
    toDate: moment.Moment | undefined;
    shortDesc1: string | undefined;
    shortDesc2: string | undefined;
    shortDesc3: string | undefined;
    desc: string | undefined;
    categoryId: string | undefined;
    plateId: string | undefined;
    plate: Plate | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class PriceStrategy implements IPriceStrategy {
    tenantId!: number | undefined;
    value!: number | undefined;
    priceCodeId!: number | undefined;
    plateMenuId!: string | undefined;
    productMenuId!: string | undefined;
    priceCode!: PriceStrategyCode | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: IPriceStrategy) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.value = data["value"];
            this.priceCodeId = data["priceCodeId"];
            this.plateMenuId = data["plateMenuId"];
            this.productMenuId = data["productMenuId"];
            this.priceCode = data["priceCode"] ? PriceStrategyCode.fromJS(data["priceCode"]) : <any>undefined;
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): PriceStrategy {
        data = typeof data === 'object' ? data : {};
        let result = new PriceStrategy();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["value"] = this.value;
        data["priceCodeId"] = this.priceCodeId;
        data["plateMenuId"] = this.plateMenuId;
        data["productMenuId"] = this.productMenuId;
        data["priceCode"] = this.priceCode ? this.priceCode.toJSON() : <any>undefined;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IPriceStrategy {
    tenantId: number | undefined;
    value: number | undefined;
    priceCodeId: number | undefined;
    plateMenuId: string | undefined;
    productMenuId: string | undefined;
    priceCode: PriceStrategyCode | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class ProductCategory implements IProductCategory {
    category!: Category | undefined;
    categoryId!: string | undefined;
    id!: number | undefined;

    constructor(data?: IProductCategory) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.category = data["category"] ? Category.fromJS(data["category"]) : <any>undefined;
            this.categoryId = data["categoryId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ProductCategory {
        data = typeof data === 'object' ? data : {};
        let result = new ProductCategory();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["category"] = this.category ? this.category.toJSON() : <any>undefined;
        data["categoryId"] = this.categoryId;
        data["id"] = this.id;
        return data;
    }
}

export interface IProductCategory {
    category: Category | undefined;
    categoryId: string | undefined;
    id: number | undefined;
}

export class PriceStrategyCode implements IPriceStrategyCode {
    tenantId!: number | undefined;
    code!: string | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IPriceStrategyCode) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.code = data["code"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): PriceStrategyCode {
        data = typeof data === 'object' ? data : {};
        let result = new PriceStrategyCode();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["code"] = this.code;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IPriceStrategyCode {
    tenantId: number | undefined;
    code: string | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class Category implements ICategory {
    status!: number | undefined;
    name!: string | undefined;
    description!: string | undefined;
    imageUrl!: string | undefined;
    tenantId!: number | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: string | undefined;

    constructor(data?: ICategory) {
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
            this.name = data["name"];
            this.description = data["description"];
            this.imageUrl = data["imageUrl"];
            this.tenantId = data["tenantId"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): Category {
        data = typeof data === 'object' ? data : {};
        let result = new Category();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["status"] = this.status;
        data["name"] = this.name;
        data["description"] = this.description;
        data["imageUrl"] = this.imageUrl;
        data["tenantId"] = this.tenantId;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface ICategory {
    status: number | undefined;
    name: string | undefined;
    description: string | undefined;
    imageUrl: string | undefined;
    tenantId: number | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: string | undefined;
}

export class DetailTransaction implements IDetailTransaction {
    tranCode!: string | undefined;
    tenantId!: number | undefined;
    startTime!: moment.Moment | undefined;
    paymentTime!: moment.Moment | undefined;
    sessionId!: string | undefined;
    session!: Session | undefined;
    products!: ProductTransaction[] | undefined;
    paymentState!: DetailTransactionPaymentState | undefined;
    paymentType!: DetailTransactionPaymentType | undefined;
    status!: DetailTransactionStatus | undefined;
    amount!: number | undefined;
    cashlessDetailId!: number | undefined;
    cashlessDetail!: CashlessDetail | undefined;
    cashDetailId!: number | undefined;
    cashDetail!: CashDetail | undefined;
    isSynced!: boolean | undefined;
    syncDate!: moment.Moment | undefined;
    machineId!: string | undefined;
    machineName!: string | undefined;
    buyer!: string | undefined;
    beginTranImage!: string | undefined;
    endTranImage!: string | undefined;
    tranVideo!: string | undefined;
    beginTranImageByte!: string | undefined;
    endTranImageByte!: string | undefined;
    isDeleted!: boolean | undefined;
    deleterUserId!: number | undefined;
    deletionTime!: moment.Moment | undefined;
    lastModificationTime!: moment.Moment | undefined;
    lastModifierUserId!: number | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IDetailTransaction) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tranCode = data["tranCode"];
            this.tenantId = data["tenantId"];
            this.startTime = data["startTime"] ? moment(data["startTime"].toString()) : <any>undefined;
            this.paymentTime = data["paymentTime"] ? moment(data["paymentTime"].toString()) : <any>undefined;
            this.sessionId = data["sessionId"];
            this.session = data["session"] ? Session.fromJS(data["session"]) : <any>undefined;
            if (data["products"] && data["products"].constructor === Array) {
                this.products = [];
                for (let item of data["products"])
                    this.products.push(ProductTransaction.fromJS(item));
            }
            this.paymentState = data["paymentState"];
            this.paymentType = data["paymentType"];
            this.status = data["status"];
            this.amount = data["amount"];
            this.cashlessDetailId = data["cashlessDetailId"];
            this.cashlessDetail = data["cashlessDetail"] ? CashlessDetail.fromJS(data["cashlessDetail"]) : <any>undefined;
            this.cashDetailId = data["cashDetailId"];
            this.cashDetail = data["cashDetail"] ? CashDetail.fromJS(data["cashDetail"]) : <any>undefined;
            this.isSynced = data["isSynced"];
            this.syncDate = data["syncDate"] ? moment(data["syncDate"].toString()) : <any>undefined;
            this.machineId = data["machineId"];
            this.machineName = data["machineName"];
            this.buyer = data["buyer"];
            this.beginTranImage = data["beginTranImage"];
            this.endTranImage = data["endTranImage"];
            this.tranVideo = data["tranVideo"];
            this.beginTranImageByte = data["beginTranImageByte"];
            this.endTranImageByte = data["endTranImageByte"];
            this.isDeleted = data["isDeleted"];
            this.deleterUserId = data["deleterUserId"];
            this.deletionTime = data["deletionTime"] ? moment(data["deletionTime"].toString()) : <any>undefined;
            this.lastModificationTime = data["lastModificationTime"] ? moment(data["lastModificationTime"].toString()) : <any>undefined;
            this.lastModifierUserId = data["lastModifierUserId"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): DetailTransaction {
        data = typeof data === 'object' ? data : {};
        let result = new DetailTransaction();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tranCode"] = this.tranCode;
        data["tenantId"] = this.tenantId;
        data["startTime"] = this.startTime ? this.startTime.toISOString() : <any>undefined;
        data["paymentTime"] = this.paymentTime ? this.paymentTime.toISOString() : <any>undefined;
        data["sessionId"] = this.sessionId;
        data["session"] = this.session ? this.session.toJSON() : <any>undefined;
        if (this.products && this.products.constructor === Array) {
            data["products"] = [];
            for (let item of this.products)
                data["products"].push(item.toJSON());
        }
        data["paymentState"] = this.paymentState;
        data["paymentType"] = this.paymentType;
        data["status"] = this.status;
        data["amount"] = this.amount;
        data["cashlessDetailId"] = this.cashlessDetailId;
        data["cashlessDetail"] = this.cashlessDetail ? this.cashlessDetail.toJSON() : <any>undefined;
        data["cashDetailId"] = this.cashDetailId;
        data["cashDetail"] = this.cashDetail ? this.cashDetail.toJSON() : <any>undefined;
        data["isSynced"] = this.isSynced;
        data["syncDate"] = this.syncDate ? this.syncDate.toISOString() : <any>undefined;
        data["machineId"] = this.machineId;
        data["machineName"] = this.machineName;
        data["buyer"] = this.buyer;
        data["beginTranImage"] = this.beginTranImage;
        data["endTranImage"] = this.endTranImage;
        data["tranVideo"] = this.tranVideo;
        data["beginTranImageByte"] = this.beginTranImageByte;
        data["endTranImageByte"] = this.endTranImageByte;
        data["isDeleted"] = this.isDeleted;
        data["deleterUserId"] = this.deleterUserId;
        data["deletionTime"] = this.deletionTime ? this.deletionTime.toISOString() : <any>undefined;
        data["lastModificationTime"] = this.lastModificationTime ? this.lastModificationTime.toISOString() : <any>undefined;
        data["lastModifierUserId"] = this.lastModifierUserId;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IDetailTransaction {
    tranCode: string | undefined;
    tenantId: number | undefined;
    startTime: moment.Moment | undefined;
    paymentTime: moment.Moment | undefined;
    sessionId: string | undefined;
    session: Session | undefined;
    products: ProductTransaction[] | undefined;
    paymentState: DetailTransactionPaymentState | undefined;
    paymentType: DetailTransactionPaymentType | undefined;
    status: DetailTransactionStatus | undefined;
    amount: number | undefined;
    cashlessDetailId: number | undefined;
    cashlessDetail: CashlessDetail | undefined;
    cashDetailId: number | undefined;
    cashDetail: CashDetail | undefined;
    isSynced: boolean | undefined;
    syncDate: moment.Moment | undefined;
    machineId: string | undefined;
    machineName: string | undefined;
    buyer: string | undefined;
    beginTranImage: string | undefined;
    endTranImage: string | undefined;
    tranVideo: string | undefined;
    beginTranImageByte: string | undefined;
    endTranImageByte: string | undefined;
    isDeleted: boolean | undefined;
    deleterUserId: number | undefined;
    deletionTime: moment.Moment | undefined;
    lastModificationTime: moment.Moment | undefined;
    lastModifierUserId: number | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class ProductTransaction implements IProductTransaction {
    product!: Product | undefined;
    transaction!: DetailTransaction | undefined;
    amount!: number | undefined;
    discId!: string | undefined;
    disc!: Disc | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: IProductTransaction) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.product = data["product"] ? Product.fromJS(data["product"]) : <any>undefined;
            this.transaction = data["transaction"] ? DetailTransaction.fromJS(data["transaction"]) : <any>undefined;
            this.amount = data["amount"];
            this.discId = data["discId"];
            this.disc = data["disc"] ? Disc.fromJS(data["disc"]) : <any>undefined;
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): ProductTransaction {
        data = typeof data === 'object' ? data : {};
        let result = new ProductTransaction();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        data["transaction"] = this.transaction ? this.transaction.toJSON() : <any>undefined;
        data["amount"] = this.amount;
        data["discId"] = this.discId;
        data["disc"] = this.disc ? this.disc.toJSON() : <any>undefined;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface IProductTransaction {
    product: Product | undefined;
    transaction: DetailTransaction | undefined;
    amount: number | undefined;
    discId: string | undefined;
    disc: Disc | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class CashlessDetail implements ICashlessDetail {
    amount!: number | undefined;
    tid!: string | undefined;
    mid!: string | undefined;
    invoice!: string | undefined;
    batch!: string | undefined;
    cardLabel!: string | undefined;
    cardNumber!: string | undefined;
    rrn!: string | undefined;
    approveCode!: string | undefined;
    entryMode!: string | undefined;
    appLabel!: string | undefined;
    aid!: string | undefined;
    tc!: string | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: ICashlessDetail) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.amount = data["amount"];
            this.tid = data["tid"];
            this.mid = data["mid"];
            this.invoice = data["invoice"];
            this.batch = data["batch"];
            this.cardLabel = data["cardLabel"];
            this.cardNumber = data["cardNumber"];
            this.rrn = data["rrn"];
            this.approveCode = data["approveCode"];
            this.entryMode = data["entryMode"];
            this.appLabel = data["appLabel"];
            this.aid = data["aid"];
            this.tc = data["tc"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CashlessDetail {
        data = typeof data === 'object' ? data : {};
        let result = new CashlessDetail();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["amount"] = this.amount;
        data["tid"] = this.tid;
        data["mid"] = this.mid;
        data["invoice"] = this.invoice;
        data["batch"] = this.batch;
        data["cardLabel"] = this.cardLabel;
        data["cardNumber"] = this.cardNumber;
        data["rrn"] = this.rrn;
        data["approveCode"] = this.approveCode;
        data["entryMode"] = this.entryMode;
        data["appLabel"] = this.appLabel;
        data["aid"] = this.aid;
        data["tc"] = this.tc;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface ICashlessDetail {
    amount: number | undefined;
    tid: string | undefined;
    mid: string | undefined;
    invoice: string | undefined;
    batch: string | undefined;
    cardLabel: string | undefined;
    cardNumber: string | undefined;
    rrn: string | undefined;
    approveCode: string | undefined;
    entryMode: string | undefined;
    appLabel: string | undefined;
    aid: string | undefined;
    tc: string | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class CashDetail implements ICashDetail {
    changeIssued!: number | undefined;
    amount!: number | undefined;
    cashInfo!: string | undefined;
    creationTime!: moment.Moment | undefined;
    creatorUserId!: number | undefined;
    id!: number | undefined;

    constructor(data?: ICashDetail) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.changeIssued = data["changeIssued"];
            this.amount = data["amount"];
            this.cashInfo = data["cashInfo"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.creatorUserId = data["creatorUserId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CashDetail {
        data = typeof data === 'object' ? data : {};
        let result = new CashDetail();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["changeIssued"] = this.changeIssued;
        data["amount"] = this.amount;
        data["cashInfo"] = this.cashInfo;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["creatorUserId"] = this.creatorUserId;
        data["id"] = this.id;
        return data;
    }
}

export interface ICashDetail {
    changeIssued: number | undefined;
    amount: number | undefined;
    cashInfo: string | undefined;
    creationTime: moment.Moment | undefined;
    creatorUserId: number | undefined;
    id: number | undefined;
}

export class RestApiGenericResultOfInt64 implements IRestApiGenericResultOfInt64 {
    result!: number[] | undefined;
    targetUrl!: any | undefined;
    success!: boolean | undefined;
    error!: any | undefined;
    unAuthorizedRequest!: boolean | undefined;
    __abp!: boolean | undefined;

    constructor(data?: IRestApiGenericResultOfInt64) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["result"] && data["result"].constructor === Array) {
                this.result = [];
                for (let item of data["result"])
                    this.result.push(item);
            }
            this.targetUrl = data["targetUrl"];
            this.success = data["success"];
            this.error = data["error"];
            this.unAuthorizedRequest = data["unAuthorizedRequest"];
            this.__abp = data["__abp"];
        }
    }

    static fromJS(data: any): RestApiGenericResultOfInt64 {
        data = typeof data === 'object' ? data : {};
        let result = new RestApiGenericResultOfInt64();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.result && this.result.constructor === Array) {
            data["result"] = [];
            for (let item of this.result)
                data["result"].push(item);
        }
        data["targetUrl"] = this.targetUrl;
        data["success"] = this.success;
        data["error"] = this.error;
        data["unAuthorizedRequest"] = this.unAuthorizedRequest;
        data["__abp"] = this.__abp;
        return data;
    }
}

export interface IRestApiGenericResultOfInt64 {
    result: number[] | undefined;
    targetUrl: any | undefined;
    success: boolean | undefined;
    error: any | undefined;
    unAuthorizedRequest: boolean | undefined;
    __abp: boolean | undefined;
}

export class CreateRoleDto implements ICreateRoleDto {
    name!: string;
    displayName!: string;
    normalizedName!: string | undefined;
    description!: string | undefined;
    isStatic!: boolean | undefined;
    permissions!: string[] | undefined;

    constructor(data?: ICreateRoleDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.normalizedName = data["normalizedName"];
            this.description = data["description"];
            this.isStatic = data["isStatic"];
            if (data["permissions"] && data["permissions"].constructor === Array) {
                this.permissions = [];
                for (let item of data["permissions"])
                    this.permissions.push(item);
            }
        }
    }

    static fromJS(data: any): CreateRoleDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateRoleDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["normalizedName"] = this.normalizedName;
        data["description"] = this.description;
        data["isStatic"] = this.isStatic;
        if (this.permissions && this.permissions.constructor === Array) {
            data["permissions"] = [];
            for (let item of this.permissions)
                data["permissions"].push(item);
        }
        return data;
    }
}

export interface ICreateRoleDto {
    name: string;
    displayName: string;
    normalizedName: string | undefined;
    description: string | undefined;
    isStatic: boolean | undefined;
    permissions: string[] | undefined;
}

export class RoleDto implements IRoleDto {
    name!: string;
    displayName!: string;
    normalizedName!: string | undefined;
    description!: string | undefined;
    isStatic!: boolean | undefined;
    permissions!: string[] | undefined;
    id!: number | undefined;

    constructor(data?: IRoleDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.normalizedName = data["normalizedName"];
            this.description = data["description"];
            this.isStatic = data["isStatic"];
            if (data["permissions"] && data["permissions"].constructor === Array) {
                this.permissions = [];
                for (let item of data["permissions"])
                    this.permissions.push(item);
            }
            this.id = data["id"];
        }
    }

    static fromJS(data: any): RoleDto {
        data = typeof data === 'object' ? data : {};
        let result = new RoleDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["normalizedName"] = this.normalizedName;
        data["description"] = this.description;
        data["isStatic"] = this.isStatic;
        if (this.permissions && this.permissions.constructor === Array) {
            data["permissions"] = [];
            for (let item of this.permissions)
                data["permissions"].push(item);
        }
        data["id"] = this.id;
        return data;
    }
}

export interface IRoleDto {
    name: string;
    displayName: string;
    normalizedName: string | undefined;
    description: string | undefined;
    isStatic: boolean | undefined;
    permissions: string[] | undefined;
    id: number | undefined;
}

export class ListResultDtoOfPermissionDto implements IListResultDtoOfPermissionDto {
    items!: PermissionDto[] | undefined;

    constructor(data?: IListResultDtoOfPermissionDto) {
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
                    this.items.push(PermissionDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfPermissionDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfPermissionDto();
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

export interface IListResultDtoOfPermissionDto {
    items: PermissionDto[] | undefined;
}

export class PermissionDto implements IPermissionDto {
    name!: string | undefined;
    displayName!: string | undefined;
    description!: string | undefined;
    id!: number | undefined;

    constructor(data?: IPermissionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.description = data["description"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): PermissionDto {
        data = typeof data === 'object' ? data : {};
        let result = new PermissionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["description"] = this.description;
        data["id"] = this.id;
        return data;
    }
}

export interface IPermissionDto {
    name: string | undefined;
    displayName: string | undefined;
    description: string | undefined;
    id: number | undefined;
}

export class PagedResultDtoOfRoleDto implements IPagedResultDtoOfRoleDto {
    totalCount!: number | undefined;
    items!: RoleDto[] | undefined;

    constructor(data?: IPagedResultDtoOfRoleDto) {
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
                    this.items.push(RoleDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfRoleDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfRoleDto();
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
}

export interface IPagedResultDtoOfRoleDto {
    totalCount: number | undefined;
    items: RoleDto[] | undefined;
}

export class ListResultDtoOfRoleListDto implements IListResultDtoOfRoleListDto {
    items!: RoleListDto[] | undefined;

    constructor(data?: IListResultDtoOfRoleListDto) {
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
                    this.items.push(RoleListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfRoleListDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfRoleListDto();
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

export interface IListResultDtoOfRoleListDto {
    items: RoleListDto[] | undefined;
}

export class RoleListDto implements IRoleListDto {
    name!: string | undefined;
    displayName!: string | undefined;
    isStatic!: boolean | undefined;
    isDefault!: boolean | undefined;
    creationTime!: moment.Moment | undefined;
    id!: number | undefined;

    constructor(data?: IRoleListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.isStatic = data["isStatic"];
            this.isDefault = data["isDefault"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): RoleListDto {
        data = typeof data === 'object' ? data : {};
        let result = new RoleListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["isStatic"] = this.isStatic;
        data["isDefault"] = this.isDefault;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface IRoleListDto {
    name: string | undefined;
    displayName: string | undefined;
    isStatic: boolean | undefined;
    isDefault: boolean | undefined;
    creationTime: moment.Moment | undefined;
    id: number | undefined;
}

export class GetRoleForEditOutput implements IGetRoleForEditOutput {
    role!: RoleEditDto | undefined;
    permissions!: FlatPermissionDto[] | undefined;
    grantedPermissionNames!: string[] | undefined;

    constructor(data?: IGetRoleForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.role = data["role"] ? RoleEditDto.fromJS(data["role"]) : <any>undefined;
            if (data["permissions"] && data["permissions"].constructor === Array) {
                this.permissions = [];
                for (let item of data["permissions"])
                    this.permissions.push(FlatPermissionDto.fromJS(item));
            }
            if (data["grantedPermissionNames"] && data["grantedPermissionNames"].constructor === Array) {
                this.grantedPermissionNames = [];
                for (let item of data["grantedPermissionNames"])
                    this.grantedPermissionNames.push(item);
            }
        }
    }

    static fromJS(data: any): GetRoleForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetRoleForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["role"] = this.role ? this.role.toJSON() : <any>undefined;
        if (this.permissions && this.permissions.constructor === Array) {
            data["permissions"] = [];
            for (let item of this.permissions)
                data["permissions"].push(item.toJSON());
        }
        if (this.grantedPermissionNames && this.grantedPermissionNames.constructor === Array) {
            data["grantedPermissionNames"] = [];
            for (let item of this.grantedPermissionNames)
                data["grantedPermissionNames"].push(item);
        }
        return data;
    }
}

export interface IGetRoleForEditOutput {
    role: RoleEditDto | undefined;
    permissions: FlatPermissionDto[] | undefined;
    grantedPermissionNames: string[] | undefined;
}

export class RoleEditDto implements IRoleEditDto {
    id!: number | undefined;
    displayName!: string;
    isDefault!: boolean | undefined;

    constructor(data?: IRoleEditDto) {
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
            this.displayName = data["displayName"];
            this.isDefault = data["isDefault"];
        }
    }

    static fromJS(data: any): RoleEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new RoleEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["displayName"] = this.displayName;
        data["isDefault"] = this.isDefault;
        return data;
    }
}

export interface IRoleEditDto {
    id: number | undefined;
    displayName: string;
    isDefault: boolean | undefined;
}

export class FlatPermissionDto implements IFlatPermissionDto {
    parentName!: string | undefined;
    name!: string | undefined;
    displayName!: string | undefined;
    description!: string | undefined;
    isGrantedByDefault!: boolean | undefined;

    constructor(data?: IFlatPermissionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.parentName = data["parentName"];
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.description = data["description"];
            this.isGrantedByDefault = data["isGrantedByDefault"];
        }
    }

    static fromJS(data: any): FlatPermissionDto {
        data = typeof data === 'object' ? data : {};
        let result = new FlatPermissionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["parentName"] = this.parentName;
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["description"] = this.description;
        data["isGrantedByDefault"] = this.isGrantedByDefault;
        return data;
    }
}

export interface IFlatPermissionDto {
    parentName: string | undefined;
    name: string | undefined;
    displayName: string | undefined;
    description: string | undefined;
    isGrantedByDefault: boolean | undefined;
}

export class CreateOrUpdateRoleInput implements ICreateOrUpdateRoleInput {
    role!: RoleEditDto;
    grantedPermissionNames!: string[];

    constructor(data?: ICreateOrUpdateRoleInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.role = new RoleEditDto();
            this.grantedPermissionNames = [];
        }
    }

    init(data?: any) {
        if (data) {
            this.role = data["role"] ? RoleEditDto.fromJS(data["role"]) : new RoleEditDto();
            if (data["grantedPermissionNames"] && data["grantedPermissionNames"].constructor === Array) {
                this.grantedPermissionNames = [];
                for (let item of data["grantedPermissionNames"])
                    this.grantedPermissionNames.push(item);
            }
        }
    }

    static fromJS(data: any): CreateOrUpdateRoleInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrUpdateRoleInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["role"] = this.role ? this.role.toJSON() : <any>undefined;
        if (this.grantedPermissionNames && this.grantedPermissionNames.constructor === Array) {
            data["grantedPermissionNames"] = [];
            for (let item of this.grantedPermissionNames)
                data["grantedPermissionNames"].push(item);
        }
        return data;
    }
}

export interface ICreateOrUpdateRoleInput {
    role: RoleEditDto;
    grantedPermissionNames: string[];
}

export class GetCurrentLoginInformationsOutput implements IGetCurrentLoginInformationsOutput {
    user!: UserLoginInfoDto | undefined;
    tenant!: TenantLoginInfoDto | undefined;
    application!: ApplicationInfoDto | undefined;

    constructor(data?: IGetCurrentLoginInformationsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.user = data["user"] ? UserLoginInfoDto.fromJS(data["user"]) : <any>undefined;
            this.tenant = data["tenant"] ? TenantLoginInfoDto.fromJS(data["tenant"]) : <any>undefined;
            this.application = data["application"] ? ApplicationInfoDto.fromJS(data["application"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetCurrentLoginInformationsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetCurrentLoginInformationsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["user"] = this.user ? this.user.toJSON() : <any>undefined;
        data["tenant"] = this.tenant ? this.tenant.toJSON() : <any>undefined;
        data["application"] = this.application ? this.application.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetCurrentLoginInformationsOutput {
    user: UserLoginInfoDto | undefined;
    tenant: TenantLoginInfoDto | undefined;
    application: ApplicationInfoDto | undefined;
}

export class UserLoginInfoDto implements IUserLoginInfoDto {
    name!: string | undefined;
    surname!: string | undefined;
    userName!: string | undefined;
    emailAddress!: string | undefined;
    profilePictureId!: string | undefined;
    id!: number | undefined;

    constructor(data?: IUserLoginInfoDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.surname = data["surname"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.profilePictureId = data["profilePictureId"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): UserLoginInfoDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserLoginInfoDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["profilePictureId"] = this.profilePictureId;
        data["id"] = this.id;
        return data;
    }
}

export interface IUserLoginInfoDto {
    name: string | undefined;
    surname: string | undefined;
    userName: string | undefined;
    emailAddress: string | undefined;
    profilePictureId: string | undefined;
    id: number | undefined;
}

export class TenantLoginInfoDto implements ITenantLoginInfoDto {
    tenancyName!: string | undefined;
    name!: string | undefined;
    logoId!: string | undefined;
    logoFileType!: string | undefined;
    customCssId!: string | undefined;
    subscriptionEndDateUtc!: moment.Moment | undefined;
    isInTrialPeriod!: boolean | undefined;
    edition!: EditionInfoDto | undefined;
    creationTime!: moment.Moment | undefined;
    paymentPeriodType!: TenantLoginInfoDtoPaymentPeriodType | undefined;
    subscriptionDateString!: string | undefined;
    creationTimeString!: string | undefined;
    id!: number | undefined;

    constructor(data?: ITenantLoginInfoDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.name = data["name"];
            this.logoId = data["logoId"];
            this.logoFileType = data["logoFileType"];
            this.customCssId = data["customCssId"];
            this.subscriptionEndDateUtc = data["subscriptionEndDateUtc"] ? moment(data["subscriptionEndDateUtc"].toString()) : <any>undefined;
            this.isInTrialPeriod = data["isInTrialPeriod"];
            this.edition = data["edition"] ? EditionInfoDto.fromJS(data["edition"]) : <any>undefined;
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.paymentPeriodType = data["paymentPeriodType"];
            this.subscriptionDateString = data["subscriptionDateString"];
            this.creationTimeString = data["creationTimeString"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): TenantLoginInfoDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantLoginInfoDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["name"] = this.name;
        data["logoId"] = this.logoId;
        data["logoFileType"] = this.logoFileType;
        data["customCssId"] = this.customCssId;
        data["subscriptionEndDateUtc"] = this.subscriptionEndDateUtc ? this.subscriptionEndDateUtc.toISOString() : <any>undefined;
        data["isInTrialPeriod"] = this.isInTrialPeriod;
        data["edition"] = this.edition ? this.edition.toJSON() : <any>undefined;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["paymentPeriodType"] = this.paymentPeriodType;
        data["subscriptionDateString"] = this.subscriptionDateString;
        data["creationTimeString"] = this.creationTimeString;
        data["id"] = this.id;
        return data;
    }
}

export interface ITenantLoginInfoDto {
    tenancyName: string | undefined;
    name: string | undefined;
    logoId: string | undefined;
    logoFileType: string | undefined;
    customCssId: string | undefined;
    subscriptionEndDateUtc: moment.Moment | undefined;
    isInTrialPeriod: boolean | undefined;
    edition: EditionInfoDto | undefined;
    creationTime: moment.Moment | undefined;
    paymentPeriodType: TenantLoginInfoDtoPaymentPeriodType | undefined;
    subscriptionDateString: string | undefined;
    creationTimeString: string | undefined;
    id: number | undefined;
}

export class ApplicationInfoDto implements IApplicationInfoDto {
    version!: string | undefined;
    releaseDate!: moment.Moment | undefined;
    features!: { [key: string]: boolean; } | undefined;

    constructor(data?: IApplicationInfoDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.version = data["version"];
            this.releaseDate = data["releaseDate"] ? moment(data["releaseDate"].toString()) : <any>undefined;
            if (data["features"]) {
                this.features = {};
                for (let key in data["features"]) {
                    if (data["features"].hasOwnProperty(key))
                        this.features[key] = data["features"][key];
                }
            }
        }
    }

    static fromJS(data: any): ApplicationInfoDto {
        data = typeof data === 'object' ? data : {};
        let result = new ApplicationInfoDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["version"] = this.version;
        data["releaseDate"] = this.releaseDate ? this.releaseDate.toISOString() : <any>undefined;
        if (this.features) {
            data["features"] = {};
            for (let key in this.features) {
                if (this.features.hasOwnProperty(key))
                    data["features"][key] = this.features[key];
            }
        }
        return data;
    }
}

export interface IApplicationInfoDto {
    version: string | undefined;
    releaseDate: moment.Moment | undefined;
    features: { [key: string]: boolean; } | undefined;
}

export class EditionInfoDto implements IEditionInfoDto {
    displayName!: string | undefined;
    trialDayCount!: number | undefined;
    monthlyPrice!: number | undefined;
    annualPrice!: number | undefined;
    isHighestEdition!: boolean | undefined;
    isFree!: boolean | undefined;
    id!: number | undefined;

    constructor(data?: IEditionInfoDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.displayName = data["displayName"];
            this.trialDayCount = data["trialDayCount"];
            this.monthlyPrice = data["monthlyPrice"];
            this.annualPrice = data["annualPrice"];
            this.isHighestEdition = data["isHighestEdition"];
            this.isFree = data["isFree"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): EditionInfoDto {
        data = typeof data === 'object' ? data : {};
        let result = new EditionInfoDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["displayName"] = this.displayName;
        data["trialDayCount"] = this.trialDayCount;
        data["monthlyPrice"] = this.monthlyPrice;
        data["annualPrice"] = this.annualPrice;
        data["isHighestEdition"] = this.isHighestEdition;
        data["isFree"] = this.isFree;
        data["id"] = this.id;
        return data;
    }
}

export interface IEditionInfoDto {
    displayName: string | undefined;
    trialDayCount: number | undefined;
    monthlyPrice: number | undefined;
    annualPrice: number | undefined;
    isHighestEdition: boolean | undefined;
    isFree: boolean | undefined;
    id: number | undefined;
}

export class UpdateUserSignInTokenOutput implements IUpdateUserSignInTokenOutput {
    signInToken!: string | undefined;
    encodedUserId!: string | undefined;
    encodedTenantId!: string | undefined;

    constructor(data?: IUpdateUserSignInTokenOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.signInToken = data["signInToken"];
            this.encodedUserId = data["encodedUserId"];
            this.encodedTenantId = data["encodedTenantId"];
        }
    }

    static fromJS(data: any): UpdateUserSignInTokenOutput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateUserSignInTokenOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["signInToken"] = this.signInToken;
        data["encodedUserId"] = this.encodedUserId;
        data["encodedTenantId"] = this.encodedTenantId;
        return data;
    }
}

export interface IUpdateUserSignInTokenOutput {
    signInToken: string | undefined;
    encodedUserId: string | undefined;
    encodedTenantId: string | undefined;
}

export class PagedResultDtoOfGetSessionForView implements IPagedResultDtoOfGetSessionForView {
    totalCount!: number | undefined;
    items!: GetSessionForView[] | undefined;

    constructor(data?: IPagedResultDtoOfGetSessionForView) {
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
                    this.items.push(GetSessionForView.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfGetSessionForView {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfGetSessionForView();
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
}

export interface IPagedResultDtoOfGetSessionForView {
    totalCount: number | undefined;
    items: GetSessionForView[] | undefined;
}

export class GetSessionForView implements IGetSessionForView {
    session!: SessionDto | undefined;

    constructor(data?: IGetSessionForView) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.session = data["session"] ? SessionDto.fromJS(data["session"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetSessionForView {
        data = typeof data === 'object' ? data : {};
        let result = new GetSessionForView();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["session"] = this.session ? this.session.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetSessionForView {
    session: SessionDto | undefined;
}

export class SessionDto implements ISessionDto {
    name!: string | undefined;
    fromHrs!: string | undefined;
    toHrs!: string | undefined;
    activeFlg!: boolean | undefined;
    id!: string | undefined;

    constructor(data?: ISessionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.fromHrs = data["fromHrs"];
            this.toHrs = data["toHrs"];
            this.activeFlg = data["activeFlg"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): SessionDto {
        data = typeof data === 'object' ? data : {};
        let result = new SessionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["fromHrs"] = this.fromHrs;
        data["toHrs"] = this.toHrs;
        data["activeFlg"] = this.activeFlg;
        data["id"] = this.id;
        return data;
    }
}

export interface ISessionDto {
    name: string | undefined;
    fromHrs: string | undefined;
    toHrs: string | undefined;
    activeFlg: boolean | undefined;
    id: string | undefined;
}

export class GetSessionForEditOutput implements IGetSessionForEditOutput {
    session!: CreateOrEditSessionDto | undefined;

    constructor(data?: IGetSessionForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.session = data["session"] ? CreateOrEditSessionDto.fromJS(data["session"]) : <any>undefined;
        }
    }

    static fromJS(data: any): GetSessionForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetSessionForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["session"] = this.session ? this.session.toJSON() : <any>undefined;
        return data;
    }
}

export interface IGetSessionForEditOutput {
    session: CreateOrEditSessionDto | undefined;
}

export class CreateOrEditSessionDto implements ICreateOrEditSessionDto {
    name!: string;
    fromHrs!: string | undefined;
    toHrs!: string | undefined;
    activeFlg!: boolean | undefined;
    id!: string | undefined;

    constructor(data?: ICreateOrEditSessionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.fromHrs = data["fromHrs"];
            this.toHrs = data["toHrs"];
            this.activeFlg = data["activeFlg"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): CreateOrEditSessionDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrEditSessionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["fromHrs"] = this.fromHrs;
        data["toHrs"] = this.toHrs;
        data["activeFlg"] = this.activeFlg;
        data["id"] = this.id;
        return data;
    }
}

export interface ICreateOrEditSessionDto {
    name: string;
    fromHrs: string | undefined;
    toHrs: string | undefined;
    activeFlg: boolean | undefined;
    id: string | undefined;
}

export class ListResultDtoOfSystemConfigDto implements IListResultDtoOfSystemConfigDto {
    items!: SystemConfigDto[] | undefined;

    constructor(data?: IListResultDtoOfSystemConfigDto) {
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
                    this.items.push(SystemConfigDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfSystemConfigDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfSystemConfigDto();
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

export interface IListResultDtoOfSystemConfigDto {
    items: SystemConfigDto[] | undefined;
}

export class SystemConfigDto implements ISystemConfigDto {
    name!: string | undefined;
    value!: string | undefined;

    constructor(data?: ISystemConfigDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.value = data["value"];
        }
    }

    static fromJS(data: any): SystemConfigDto {
        data = typeof data === 'object' ? data : {};
        let result = new SystemConfigDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["value"] = this.value;
        return data;
    }
}

export interface ISystemConfigDto {
    name: string | undefined;
    value: string | undefined;
}

export class UpdateConfigInput implements IUpdateConfigInput {
    name!: string | undefined;
    value!: string | undefined;
    tenantId!: number | undefined;

    constructor(data?: IUpdateConfigInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.value = data["value"];
            this.tenantId = data["tenantId"];
        }
    }

    static fromJS(data: any): UpdateConfigInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateConfigInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["value"] = this.value;
        data["tenantId"] = this.tenantId;
        return data;
    }
}

export interface IUpdateConfigInput {
    name: string | undefined;
    value: string | undefined;
    tenantId: number | undefined;
}

export class TransactionInfo implements ITransactionInfo {
    id!: string | undefined;
    menuItems!: MenuItemInfo[] | undefined;
    amount!: number | undefined;
    plateCount!: number | undefined;
    paymentState!: TransactionInfoPaymentState | undefined;
    paymentType!: TransactionInfoPaymentType | undefined;
    customerMessage!: string | undefined;
    sessionId!: string | undefined;
    buyer!: string | undefined;
    beginTranImage!: string | undefined;
    endTranImage!: string | undefined;
    cashlessInfo!: CashlessDetail | undefined;

    constructor(data?: ITransactionInfo) {
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
            if (data["menuItems"] && data["menuItems"].constructor === Array) {
                this.menuItems = [];
                for (let item of data["menuItems"])
                    this.menuItems.push(MenuItemInfo.fromJS(item));
            }
            this.amount = data["amount"];
            this.plateCount = data["plateCount"];
            this.paymentState = data["paymentState"];
            this.paymentType = data["paymentType"];
            this.customerMessage = data["customerMessage"];
            this.sessionId = data["sessionId"];
            this.buyer = data["buyer"];
            this.beginTranImage = data["beginTranImage"];
            this.endTranImage = data["endTranImage"];
            this.cashlessInfo = data["cashlessInfo"] ? CashlessDetail.fromJS(data["cashlessInfo"]) : <any>undefined;
        }
    }

    static fromJS(data: any): TransactionInfo {
        data = typeof data === 'object' ? data : {};
        let result = new TransactionInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        if (this.menuItems && this.menuItems.constructor === Array) {
            data["menuItems"] = [];
            for (let item of this.menuItems)
                data["menuItems"].push(item.toJSON());
        }
        data["amount"] = this.amount;
        data["plateCount"] = this.plateCount;
        data["paymentState"] = this.paymentState;
        data["paymentType"] = this.paymentType;
        data["customerMessage"] = this.customerMessage;
        data["sessionId"] = this.sessionId;
        data["buyer"] = this.buyer;
        data["beginTranImage"] = this.beginTranImage;
        data["endTranImage"] = this.endTranImage;
        data["cashlessInfo"] = this.cashlessInfo ? this.cashlessInfo.toJSON() : <any>undefined;
        return data;
    }
}

export interface ITransactionInfo {
    id: string | undefined;
    menuItems: MenuItemInfo[] | undefined;
    amount: number | undefined;
    plateCount: number | undefined;
    paymentState: TransactionInfoPaymentState | undefined;
    paymentType: TransactionInfoPaymentType | undefined;
    customerMessage: string | undefined;
    sessionId: string | undefined;
    buyer: string | undefined;
    beginTranImage: string | undefined;
    endTranImage: string | undefined;
    cashlessInfo: CashlessDetail | undefined;
}

export class MenuItemInfo implements IMenuItemInfo {
    name!: string | undefined;
    imageUrl!: string | undefined;
    desc!: string | undefined;
    code!: string | undefined;
    color!: string | undefined;
    price!: number | undefined;
    priceContractor!: number | undefined;
    plateId!: string | undefined;
    productId!: string | undefined;
    productName!: string | undefined;
    plate!: PlateInfo | undefined;
    product!: Product | undefined;

    constructor(data?: IMenuItemInfo) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.imageUrl = data["imageUrl"];
            this.desc = data["desc"];
            this.code = data["code"];
            this.color = data["color"];
            this.price = data["price"];
            this.priceContractor = data["priceContractor"];
            this.plateId = data["plateId"];
            this.productId = data["productId"];
            this.productName = data["productName"];
            this.plate = data["plate"] ? PlateInfo.fromJS(data["plate"]) : <any>undefined;
            this.product = data["product"] ? Product.fromJS(data["product"]) : <any>undefined;
        }
    }

    static fromJS(data: any): MenuItemInfo {
        data = typeof data === 'object' ? data : {};
        let result = new MenuItemInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["imageUrl"] = this.imageUrl;
        data["desc"] = this.desc;
        data["code"] = this.code;
        data["color"] = this.color;
        data["price"] = this.price;
        data["priceContractor"] = this.priceContractor;
        data["plateId"] = this.plateId;
        data["productId"] = this.productId;
        data["productName"] = this.productName;
        data["plate"] = this.plate ? this.plate.toJSON() : <any>undefined;
        data["product"] = this.product ? this.product.toJSON() : <any>undefined;
        return data;
    }
}

export interface IMenuItemInfo {
    name: string | undefined;
    imageUrl: string | undefined;
    desc: string | undefined;
    code: string | undefined;
    color: string | undefined;
    price: number | undefined;
    priceContractor: number | undefined;
    plateId: string | undefined;
    productId: string | undefined;
    productName: string | undefined;
    plate: PlateInfo | undefined;
    product: Product | undefined;
}

export class PlateInfo implements IPlateInfo {
    uid!: string | undefined;
    code!: string | undefined;
    plateId!: string | undefined;

    constructor(data?: IPlateInfo) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.uid = data["uid"];
            this.code = data["code"];
            this.plateId = data["plateId"];
        }
    }

    static fromJS(data: any): PlateInfo {
        data = typeof data === 'object' ? data : {};
        let result = new PlateInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["uid"] = this.uid;
        data["code"] = this.code;
        data["plateId"] = this.plateId;
        return data;
    }
}

export interface IPlateInfo {
    uid: string | undefined;
    code: string | undefined;
    plateId: string | undefined;
}

export class SaleSessionCacheItem implements ISaleSessionCacheItem {
    sessionInfo!: SessionInfo | undefined;
    menuItems!: MenuItemInfo[] | undefined;
    plates!: PlateInfo[] | undefined;
    trays!: TrayInfo[] | undefined;

    constructor(data?: ISaleSessionCacheItem) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.sessionInfo = data["sessionInfo"] ? SessionInfo.fromJS(data["sessionInfo"]) : <any>undefined;
            if (data["menuItems"] && data["menuItems"].constructor === Array) {
                this.menuItems = [];
                for (let item of data["menuItems"])
                    this.menuItems.push(MenuItemInfo.fromJS(item));
            }
            if (data["plates"] && data["plates"].constructor === Array) {
                this.plates = [];
                for (let item of data["plates"])
                    this.plates.push(PlateInfo.fromJS(item));
            }
            if (data["trays"] && data["trays"].constructor === Array) {
                this.trays = [];
                for (let item of data["trays"])
                    this.trays.push(TrayInfo.fromJS(item));
            }
        }
    }

    static fromJS(data: any): SaleSessionCacheItem {
        data = typeof data === 'object' ? data : {};
        let result = new SaleSessionCacheItem();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["sessionInfo"] = this.sessionInfo ? this.sessionInfo.toJSON() : <any>undefined;
        if (this.menuItems && this.menuItems.constructor === Array) {
            data["menuItems"] = [];
            for (let item of this.menuItems)
                data["menuItems"].push(item.toJSON());
        }
        if (this.plates && this.plates.constructor === Array) {
            data["plates"] = [];
            for (let item of this.plates)
                data["plates"].push(item.toJSON());
        }
        if (this.trays && this.trays.constructor === Array) {
            data["trays"] = [];
            for (let item of this.trays)
                data["trays"].push(item.toJSON());
        }
        return data;
    }
}

export interface ISaleSessionCacheItem {
    sessionInfo: SessionInfo | undefined;
    menuItems: MenuItemInfo[] | undefined;
    plates: PlateInfo[] | undefined;
    trays: TrayInfo[] | undefined;
}

export class SessionInfo implements ISessionInfo {
    id!: string | undefined;
    name!: string | undefined;
    fromHrs!: string | undefined;
    toHrs!: string | undefined;

    constructor(data?: ISessionInfo) {
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
            this.fromHrs = data["fromHrs"];
            this.toHrs = data["toHrs"];
        }
    }

    static fromJS(data: any): SessionInfo {
        data = typeof data === 'object' ? data : {};
        let result = new SessionInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["fromHrs"] = this.fromHrs;
        data["toHrs"] = this.toHrs;
        return data;
    }
}

export interface ISessionInfo {
    id: string | undefined;
    name: string | undefined;
    fromHrs: string | undefined;
    toHrs: string | undefined;
}

export class TrayInfo implements ITrayInfo {
    name!: string | undefined;
    code!: string | undefined;

    constructor(data?: ITrayInfo) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.code = data["code"];
        }
    }

    static fromJS(data: any): TrayInfo {
        data = typeof data === 'object' ? data : {};
        let result = new TrayInfo();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["code"] = this.code;
        return data;
    }
}

export interface ITrayInfo {
    name: string | undefined;
    code: string | undefined;
}

export class PlateReadingInput implements IPlateReadingInput {
    uid!: string | undefined;
    uType!: string | undefined;
    uData!: string | undefined;

    constructor(data?: IPlateReadingInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.uid = data["uid"];
            this.uType = data["uType"];
            this.uData = data["uData"];
        }
    }

    static fromJS(data: any): PlateReadingInput {
        data = typeof data === 'object' ? data : {};
        let result = new PlateReadingInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["uid"] = this.uid;
        data["uType"] = this.uType;
        data["uData"] = this.uData;
        return data;
    }
}

export interface IPlateReadingInput {
    uid: string | undefined;
    uType: string | undefined;
    uData: string | undefined;
}

export class PagedResultDtoOfTenantListDto implements IPagedResultDtoOfTenantListDto {
    totalCount!: number | undefined;
    items!: TenantListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfTenantListDto) {
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
                    this.items.push(TenantListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfTenantListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfTenantListDto();
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
}

export interface IPagedResultDtoOfTenantListDto {
    totalCount: number | undefined;
    items: TenantListDto[] | undefined;
}

export class TenantListDto implements ITenantListDto {
    tenancyName!: string | undefined;
    name!: string | undefined;
    editionDisplayName!: string | undefined;
    connectionString!: string | undefined;
    isActive!: boolean | undefined;
    creationTime!: moment.Moment | undefined;
    subscriptionEndDateUtc!: moment.Moment | undefined;
    editionId!: number | undefined;
    isInTrialPeriod!: boolean | undefined;
    id!: number | undefined;

    constructor(data?: ITenantListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.name = data["name"];
            this.editionDisplayName = data["editionDisplayName"];
            this.connectionString = data["connectionString"];
            this.isActive = data["isActive"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.subscriptionEndDateUtc = data["subscriptionEndDateUtc"] ? moment(data["subscriptionEndDateUtc"].toString()) : <any>undefined;
            this.editionId = data["editionId"];
            this.isInTrialPeriod = data["isInTrialPeriod"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): TenantListDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["name"] = this.name;
        data["editionDisplayName"] = this.editionDisplayName;
        data["connectionString"] = this.connectionString;
        data["isActive"] = this.isActive;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["subscriptionEndDateUtc"] = this.subscriptionEndDateUtc ? this.subscriptionEndDateUtc.toISOString() : <any>undefined;
        data["editionId"] = this.editionId;
        data["isInTrialPeriod"] = this.isInTrialPeriod;
        data["id"] = this.id;
        return data;
    }
}

export interface ITenantListDto {
    tenancyName: string | undefined;
    name: string | undefined;
    editionDisplayName: string | undefined;
    connectionString: string | undefined;
    isActive: boolean | undefined;
    creationTime: moment.Moment | undefined;
    subscriptionEndDateUtc: moment.Moment | undefined;
    editionId: number | undefined;
    isInTrialPeriod: boolean | undefined;
    id: number | undefined;
}

export class CreateTenantInput implements ICreateTenantInput {
    tenancyName!: string;
    name!: string;
    adminEmailAddress!: string;
    adminPassword!: string | undefined;
    connectionString!: string | undefined;
    shouldChangePasswordOnNextLogin!: boolean | undefined;
    sendActivationEmail!: boolean | undefined;
    editionId!: number | undefined;
    isActive!: boolean | undefined;
    subscriptionEndDateUtc!: moment.Moment | undefined;
    isInTrialPeriod!: boolean | undefined;

    constructor(data?: ICreateTenantInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.name = data["name"];
            this.adminEmailAddress = data["adminEmailAddress"];
            this.adminPassword = data["adminPassword"];
            this.connectionString = data["connectionString"];
            this.shouldChangePasswordOnNextLogin = data["shouldChangePasswordOnNextLogin"];
            this.sendActivationEmail = data["sendActivationEmail"];
            this.editionId = data["editionId"];
            this.isActive = data["isActive"];
            this.subscriptionEndDateUtc = data["subscriptionEndDateUtc"] ? moment(data["subscriptionEndDateUtc"].toString()) : <any>undefined;
            this.isInTrialPeriod = data["isInTrialPeriod"];
        }
    }

    static fromJS(data: any): CreateTenantInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateTenantInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["name"] = this.name;
        data["adminEmailAddress"] = this.adminEmailAddress;
        data["adminPassword"] = this.adminPassword;
        data["connectionString"] = this.connectionString;
        data["shouldChangePasswordOnNextLogin"] = this.shouldChangePasswordOnNextLogin;
        data["sendActivationEmail"] = this.sendActivationEmail;
        data["editionId"] = this.editionId;
        data["isActive"] = this.isActive;
        data["subscriptionEndDateUtc"] = this.subscriptionEndDateUtc ? this.subscriptionEndDateUtc.toISOString() : <any>undefined;
        data["isInTrialPeriod"] = this.isInTrialPeriod;
        return data;
    }
}

export interface ICreateTenantInput {
    tenancyName: string;
    name: string;
    adminEmailAddress: string;
    adminPassword: string | undefined;
    connectionString: string | undefined;
    shouldChangePasswordOnNextLogin: boolean | undefined;
    sendActivationEmail: boolean | undefined;
    editionId: number | undefined;
    isActive: boolean | undefined;
    subscriptionEndDateUtc: moment.Moment | undefined;
    isInTrialPeriod: boolean | undefined;
}

export class TenantEditDto implements ITenantEditDto {
    tenancyName!: string;
    name!: string;
    connectionString!: string | undefined;
    editionId!: number | undefined;
    isActive!: boolean | undefined;
    subscriptionEndDateUtc!: moment.Moment | undefined;
    isInTrialPeriod!: boolean | undefined;
    id!: number | undefined;

    constructor(data?: ITenantEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.name = data["name"];
            this.connectionString = data["connectionString"];
            this.editionId = data["editionId"];
            this.isActive = data["isActive"];
            this.subscriptionEndDateUtc = data["subscriptionEndDateUtc"] ? moment(data["subscriptionEndDateUtc"].toString()) : <any>undefined;
            this.isInTrialPeriod = data["isInTrialPeriod"];
            this.id = data["id"];
        }
    }

    static fromJS(data: any): TenantEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["name"] = this.name;
        data["connectionString"] = this.connectionString;
        data["editionId"] = this.editionId;
        data["isActive"] = this.isActive;
        data["subscriptionEndDateUtc"] = this.subscriptionEndDateUtc ? this.subscriptionEndDateUtc.toISOString() : <any>undefined;
        data["isInTrialPeriod"] = this.isInTrialPeriod;
        data["id"] = this.id;
        return data;
    }
}

export interface ITenantEditDto {
    tenancyName: string;
    name: string;
    connectionString: string | undefined;
    editionId: number | undefined;
    isActive: boolean | undefined;
    subscriptionEndDateUtc: moment.Moment | undefined;
    isInTrialPeriod: boolean | undefined;
    id: number | undefined;
}

export class GetTenantFeaturesEditOutput implements IGetTenantFeaturesEditOutput {
    featureValues!: NameValueDto[] | undefined;
    features!: FlatFeatureDto[] | undefined;

    constructor(data?: IGetTenantFeaturesEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["featureValues"] && data["featureValues"].constructor === Array) {
                this.featureValues = [];
                for (let item of data["featureValues"])
                    this.featureValues.push(NameValueDto.fromJS(item));
            }
            if (data["features"] && data["features"].constructor === Array) {
                this.features = [];
                for (let item of data["features"])
                    this.features.push(FlatFeatureDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetTenantFeaturesEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetTenantFeaturesEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.featureValues && this.featureValues.constructor === Array) {
            data["featureValues"] = [];
            for (let item of this.featureValues)
                data["featureValues"].push(item.toJSON());
        }
        if (this.features && this.features.constructor === Array) {
            data["features"] = [];
            for (let item of this.features)
                data["features"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetTenantFeaturesEditOutput {
    featureValues: NameValueDto[] | undefined;
    features: FlatFeatureDto[] | undefined;
}

export class UpdateTenantFeaturesInput implements IUpdateTenantFeaturesInput {
    id!: number | undefined;
    featureValues!: NameValueDto[];

    constructor(data?: IUpdateTenantFeaturesInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.featureValues = [];
        }
    }

    init(data?: any) {
        if (data) {
            this.id = data["id"];
            if (data["featureValues"] && data["featureValues"].constructor === Array) {
                this.featureValues = [];
                for (let item of data["featureValues"])
                    this.featureValues.push(NameValueDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): UpdateTenantFeaturesInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateTenantFeaturesInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        if (this.featureValues && this.featureValues.constructor === Array) {
            data["featureValues"] = [];
            for (let item of this.featureValues)
                data["featureValues"].push(item.toJSON());
        }
        return data;
    }
}

export interface IUpdateTenantFeaturesInput {
    id: number | undefined;
    featureValues: NameValueDto[];
}

export class EntityDto implements IEntityDto {
    id!: number | undefined;

    constructor(data?: IEntityDto) {
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
        }
    }

    static fromJS(data: any): EntityDto {
        data = typeof data === 'object' ? data : {};
        let result = new EntityDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        return data;
    }
}

export interface IEntityDto {
    id: number | undefined;
}

export class GetMemberActivityOutput implements IGetMemberActivityOutput {
    memberActivities!: MemberActivity[] | undefined;

    constructor(data?: IGetMemberActivityOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["memberActivities"] && data["memberActivities"].constructor === Array) {
                this.memberActivities = [];
                for (let item of data["memberActivities"])
                    this.memberActivities.push(MemberActivity.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetMemberActivityOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetMemberActivityOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.memberActivities && this.memberActivities.constructor === Array) {
            data["memberActivities"] = [];
            for (let item of this.memberActivities)
                data["memberActivities"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetMemberActivityOutput {
    memberActivities: MemberActivity[] | undefined;
}

export class MemberActivity implements IMemberActivity {
    name!: string | undefined;
    earnings!: string | undefined;
    cases!: number | undefined;
    closed!: number | undefined;
    rate!: string | undefined;

    constructor(data?: IMemberActivity) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.earnings = data["earnings"];
            this.cases = data["cases"];
            this.closed = data["closed"];
            this.rate = data["rate"];
        }
    }

    static fromJS(data: any): MemberActivity {
        data = typeof data === 'object' ? data : {};
        let result = new MemberActivity();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["earnings"] = this.earnings;
        data["cases"] = this.cases;
        data["closed"] = this.closed;
        data["rate"] = this.rate;
        return data;
    }
}

export interface IMemberActivity {
    name: string | undefined;
    earnings: string | undefined;
    cases: number | undefined;
    closed: number | undefined;
    rate: string | undefined;
}

export class GetDashboardDataOutput implements IGetDashboardDataOutput {
    totalProfit!: number | undefined;
    newFeedbacks!: number | undefined;
    newOrders!: number | undefined;
    newUsers!: number | undefined;
    salesSummary!: SalesSummaryData[] | undefined;
    totalSales!: number | undefined;
    revenue!: number | undefined;
    expenses!: number | undefined;
    growth!: number | undefined;
    transactionPercent!: number | undefined;
    newVisitPercent!: number | undefined;
    bouncePercent!: number | undefined;
    dailySales!: number[] | undefined;
    profitShares!: number[] | undefined;

    constructor(data?: IGetDashboardDataOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.totalProfit = data["totalProfit"];
            this.newFeedbacks = data["newFeedbacks"];
            this.newOrders = data["newOrders"];
            this.newUsers = data["newUsers"];
            if (data["salesSummary"] && data["salesSummary"].constructor === Array) {
                this.salesSummary = [];
                for (let item of data["salesSummary"])
                    this.salesSummary.push(SalesSummaryData.fromJS(item));
            }
            this.totalSales = data["totalSales"];
            this.revenue = data["revenue"];
            this.expenses = data["expenses"];
            this.growth = data["growth"];
            this.transactionPercent = data["transactionPercent"];
            this.newVisitPercent = data["newVisitPercent"];
            this.bouncePercent = data["bouncePercent"];
            if (data["dailySales"] && data["dailySales"].constructor === Array) {
                this.dailySales = [];
                for (let item of data["dailySales"])
                    this.dailySales.push(item);
            }
            if (data["profitShares"] && data["profitShares"].constructor === Array) {
                this.profitShares = [];
                for (let item of data["profitShares"])
                    this.profitShares.push(item);
            }
        }
    }

    static fromJS(data: any): GetDashboardDataOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetDashboardDataOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["totalProfit"] = this.totalProfit;
        data["newFeedbacks"] = this.newFeedbacks;
        data["newOrders"] = this.newOrders;
        data["newUsers"] = this.newUsers;
        if (this.salesSummary && this.salesSummary.constructor === Array) {
            data["salesSummary"] = [];
            for (let item of this.salesSummary)
                data["salesSummary"].push(item.toJSON());
        }
        data["totalSales"] = this.totalSales;
        data["revenue"] = this.revenue;
        data["expenses"] = this.expenses;
        data["growth"] = this.growth;
        data["transactionPercent"] = this.transactionPercent;
        data["newVisitPercent"] = this.newVisitPercent;
        data["bouncePercent"] = this.bouncePercent;
        if (this.dailySales && this.dailySales.constructor === Array) {
            data["dailySales"] = [];
            for (let item of this.dailySales)
                data["dailySales"].push(item);
        }
        if (this.profitShares && this.profitShares.constructor === Array) {
            data["profitShares"] = [];
            for (let item of this.profitShares)
                data["profitShares"].push(item);
        }
        return data;
    }
}

export interface IGetDashboardDataOutput {
    totalProfit: number | undefined;
    newFeedbacks: number | undefined;
    newOrders: number | undefined;
    newUsers: number | undefined;
    salesSummary: SalesSummaryData[] | undefined;
    totalSales: number | undefined;
    revenue: number | undefined;
    expenses: number | undefined;
    growth: number | undefined;
    transactionPercent: number | undefined;
    newVisitPercent: number | undefined;
    bouncePercent: number | undefined;
    dailySales: number[] | undefined;
    profitShares: number[] | undefined;
}

export class SalesSummaryData implements ISalesSummaryData {
    period!: string | undefined;
    sales!: number | undefined;
    profit!: number | undefined;

    constructor(data?: ISalesSummaryData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.period = data["period"];
            this.sales = data["sales"];
            this.profit = data["profit"];
        }
    }

    static fromJS(data: any): SalesSummaryData {
        data = typeof data === 'object' ? data : {};
        let result = new SalesSummaryData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["period"] = this.period;
        data["sales"] = this.sales;
        data["profit"] = this.profit;
        return data;
    }
}

export interface ISalesSummaryData {
    period: string | undefined;
    sales: number | undefined;
    profit: number | undefined;
}

export class GetSalesSummaryOutput implements IGetSalesSummaryOutput {
    salesSummary!: SalesSummaryData[] | undefined;

    constructor(data?: IGetSalesSummaryOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["salesSummary"] && data["salesSummary"].constructor === Array) {
                this.salesSummary = [];
                for (let item of data["salesSummary"])
                    this.salesSummary.push(SalesSummaryData.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetSalesSummaryOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetSalesSummaryOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.salesSummary && this.salesSummary.constructor === Array) {
            data["salesSummary"] = [];
            for (let item of this.salesSummary)
                data["salesSummary"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetSalesSummaryOutput {
    salesSummary: SalesSummaryData[] | undefined;
}

export class GetRegionalStatsOutput implements IGetRegionalStatsOutput {
    stats!: RegionalStatCountry[] | undefined;

    constructor(data?: IGetRegionalStatsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["stats"] && data["stats"].constructor === Array) {
                this.stats = [];
                for (let item of data["stats"])
                    this.stats.push(RegionalStatCountry.fromJS(item));
            }
        }
    }

    static fromJS(data: any): GetRegionalStatsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetRegionalStatsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.stats && this.stats.constructor === Array) {
            data["stats"] = [];
            for (let item of this.stats)
                data["stats"].push(item.toJSON());
        }
        return data;
    }
}

export interface IGetRegionalStatsOutput {
    stats: RegionalStatCountry[] | undefined;
}

export class RegionalStatCountry implements IRegionalStatCountry {
    countryName!: string | undefined;
    sales!: number | undefined;
    change!: number[] | undefined;
    averagePrice!: number | undefined;
    totalPrice!: number | undefined;

    constructor(data?: IRegionalStatCountry) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.countryName = data["countryName"];
            this.sales = data["sales"];
            if (data["change"] && data["change"].constructor === Array) {
                this.change = [];
                for (let item of data["change"])
                    this.change.push(item);
            }
            this.averagePrice = data["averagePrice"];
            this.totalPrice = data["totalPrice"];
        }
    }

    static fromJS(data: any): RegionalStatCountry {
        data = typeof data === 'object' ? data : {};
        let result = new RegionalStatCountry();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["countryName"] = this.countryName;
        data["sales"] = this.sales;
        if (this.change && this.change.constructor === Array) {
            data["change"] = [];
            for (let item of this.change)
                data["change"].push(item);
        }
        data["averagePrice"] = this.averagePrice;
        data["totalPrice"] = this.totalPrice;
        return data;
    }
}

export interface IRegionalStatCountry {
    countryName: string | undefined;
    sales: number | undefined;
    change: number[] | undefined;
    averagePrice: number | undefined;
    totalPrice: number | undefined;
}

export class GetGeneralStatsOutput implements IGetGeneralStatsOutput {
    transactionPercent!: number | undefined;
    newVisitPercent!: number | undefined;
    bouncePercent!: number | undefined;

    constructor(data?: IGetGeneralStatsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.transactionPercent = data["transactionPercent"];
            this.newVisitPercent = data["newVisitPercent"];
            this.bouncePercent = data["bouncePercent"];
        }
    }

    static fromJS(data: any): GetGeneralStatsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetGeneralStatsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["transactionPercent"] = this.transactionPercent;
        data["newVisitPercent"] = this.newVisitPercent;
        data["bouncePercent"] = this.bouncePercent;
        return data;
    }
}

export interface IGetGeneralStatsOutput {
    transactionPercent: number | undefined;
    newVisitPercent: number | undefined;
    bouncePercent: number | undefined;
}

export class RegisterTenantInput implements IRegisterTenantInput {
    tenancyName!: string;
    name!: string;
    adminEmailAddress!: string;
    adminPassword!: string | undefined;
    captchaResponse!: string | undefined;
    subscriptionStartType!: RegisterTenantInputSubscriptionStartType | undefined;
    gateway!: RegisterTenantInputGateway | undefined;
    editionId!: number | undefined;
    paymentId!: string | undefined;

    constructor(data?: IRegisterTenantInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.name = data["name"];
            this.adminEmailAddress = data["adminEmailAddress"];
            this.adminPassword = data["adminPassword"];
            this.captchaResponse = data["captchaResponse"];
            this.subscriptionStartType = data["subscriptionStartType"];
            this.gateway = data["gateway"];
            this.editionId = data["editionId"];
            this.paymentId = data["paymentId"];
        }
    }

    static fromJS(data: any): RegisterTenantInput {
        data = typeof data === 'object' ? data : {};
        let result = new RegisterTenantInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["name"] = this.name;
        data["adminEmailAddress"] = this.adminEmailAddress;
        data["adminPassword"] = this.adminPassword;
        data["captchaResponse"] = this.captchaResponse;
        data["subscriptionStartType"] = this.subscriptionStartType;
        data["gateway"] = this.gateway;
        data["editionId"] = this.editionId;
        data["paymentId"] = this.paymentId;
        return data;
    }
}

export interface IRegisterTenantInput {
    tenancyName: string;
    name: string;
    adminEmailAddress: string;
    adminPassword: string | undefined;
    captchaResponse: string | undefined;
    subscriptionStartType: RegisterTenantInputSubscriptionStartType | undefined;
    gateway: RegisterTenantInputGateway | undefined;
    editionId: number | undefined;
    paymentId: string | undefined;
}

export class RegisterTenantOutput implements IRegisterTenantOutput {
    tenantId!: number | undefined;
    tenancyName!: string | undefined;
    name!: string | undefined;
    userName!: string | undefined;
    emailAddress!: string | undefined;
    isTenantActive!: boolean | undefined;
    isActive!: boolean | undefined;
    isEmailConfirmationRequired!: boolean | undefined;

    constructor(data?: IRegisterTenantOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.tenancyName = data["tenancyName"];
            this.name = data["name"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.isTenantActive = data["isTenantActive"];
            this.isActive = data["isActive"];
            this.isEmailConfirmationRequired = data["isEmailConfirmationRequired"];
        }
    }

    static fromJS(data: any): RegisterTenantOutput {
        data = typeof data === 'object' ? data : {};
        let result = new RegisterTenantOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["tenancyName"] = this.tenancyName;
        data["name"] = this.name;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["isTenantActive"] = this.isTenantActive;
        data["isActive"] = this.isActive;
        data["isEmailConfirmationRequired"] = this.isEmailConfirmationRequired;
        return data;
    }
}

export interface IRegisterTenantOutput {
    tenantId: number | undefined;
    tenancyName: string | undefined;
    name: string | undefined;
    userName: string | undefined;
    emailAddress: string | undefined;
    isTenantActive: boolean | undefined;
    isActive: boolean | undefined;
    isEmailConfirmationRequired: boolean | undefined;
}

export class EditionsSelectOutput implements IEditionsSelectOutput {
    allFeatures!: FlatFeatureSelectDto[] | undefined;
    editionsWithFeatures!: EditionWithFeaturesDto[] | undefined;
    tenantEditionId!: number | undefined;

    constructor(data?: IEditionsSelectOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["allFeatures"] && data["allFeatures"].constructor === Array) {
                this.allFeatures = [];
                for (let item of data["allFeatures"])
                    this.allFeatures.push(FlatFeatureSelectDto.fromJS(item));
            }
            if (data["editionsWithFeatures"] && data["editionsWithFeatures"].constructor === Array) {
                this.editionsWithFeatures = [];
                for (let item of data["editionsWithFeatures"])
                    this.editionsWithFeatures.push(EditionWithFeaturesDto.fromJS(item));
            }
            this.tenantEditionId = data["tenantEditionId"];
        }
    }

    static fromJS(data: any): EditionsSelectOutput {
        data = typeof data === 'object' ? data : {};
        let result = new EditionsSelectOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.allFeatures && this.allFeatures.constructor === Array) {
            data["allFeatures"] = [];
            for (let item of this.allFeatures)
                data["allFeatures"].push(item.toJSON());
        }
        if (this.editionsWithFeatures && this.editionsWithFeatures.constructor === Array) {
            data["editionsWithFeatures"] = [];
            for (let item of this.editionsWithFeatures)
                data["editionsWithFeatures"].push(item.toJSON());
        }
        data["tenantEditionId"] = this.tenantEditionId;
        return data;
    }
}

export interface IEditionsSelectOutput {
    allFeatures: FlatFeatureSelectDto[] | undefined;
    editionsWithFeatures: EditionWithFeaturesDto[] | undefined;
    tenantEditionId: number | undefined;
}

export class FlatFeatureSelectDto implements IFlatFeatureSelectDto {
    parentName!: string | undefined;
    name!: string | undefined;
    displayName!: string | undefined;
    description!: string | undefined;
    defaultValue!: string | undefined;
    inputType!: IInputType | undefined;
    textHtmlColor!: string | undefined;

    constructor(data?: IFlatFeatureSelectDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.parentName = data["parentName"];
            this.name = data["name"];
            this.displayName = data["displayName"];
            this.description = data["description"];
            this.defaultValue = data["defaultValue"];
            this.inputType = data["inputType"] ? IInputType.fromJS(data["inputType"]) : <any>undefined;
            this.textHtmlColor = data["textHtmlColor"];
        }
    }

    static fromJS(data: any): FlatFeatureSelectDto {
        data = typeof data === 'object' ? data : {};
        let result = new FlatFeatureSelectDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["parentName"] = this.parentName;
        data["name"] = this.name;
        data["displayName"] = this.displayName;
        data["description"] = this.description;
        data["defaultValue"] = this.defaultValue;
        data["inputType"] = this.inputType ? this.inputType.toJSON() : <any>undefined;
        data["textHtmlColor"] = this.textHtmlColor;
        return data;
    }
}

export interface IFlatFeatureSelectDto {
    parentName: string | undefined;
    name: string | undefined;
    displayName: string | undefined;
    description: string | undefined;
    defaultValue: string | undefined;
    inputType: IInputType | undefined;
    textHtmlColor: string | undefined;
}

export class EditionWithFeaturesDto implements IEditionWithFeaturesDto {
    edition!: EditionSelectDto | undefined;
    featureValues!: NameValueDto[] | undefined;

    constructor(data?: IEditionWithFeaturesDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.edition = data["edition"] ? EditionSelectDto.fromJS(data["edition"]) : <any>undefined;
            if (data["featureValues"] && data["featureValues"].constructor === Array) {
                this.featureValues = [];
                for (let item of data["featureValues"])
                    this.featureValues.push(NameValueDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): EditionWithFeaturesDto {
        data = typeof data === 'object' ? data : {};
        let result = new EditionWithFeaturesDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["edition"] = this.edition ? this.edition.toJSON() : <any>undefined;
        if (this.featureValues && this.featureValues.constructor === Array) {
            data["featureValues"] = [];
            for (let item of this.featureValues)
                data["featureValues"].push(item.toJSON());
        }
        return data;
    }
}

export interface IEditionWithFeaturesDto {
    edition: EditionSelectDto | undefined;
    featureValues: NameValueDto[] | undefined;
}

export class IInputType implements IIInputType {
    name!: string | undefined;
    attributes!: { [key: string]: any; } | undefined;
    validator!: IValueValidator | undefined;

    constructor(data?: IIInputType) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            if (data["attributes"]) {
                this.attributes = {};
                for (let key in data["attributes"]) {
                    if (data["attributes"].hasOwnProperty(key))
                        this.attributes[key] = data["attributes"][key];
                }
            }
            this.validator = data["validator"] ? IValueValidator.fromJS(data["validator"]) : <any>undefined;
        }
    }

    static fromJS(data: any): IInputType {
        data = typeof data === 'object' ? data : {};
        let result = new IInputType();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        if (this.attributes) {
            data["attributes"] = {};
            for (let key in this.attributes) {
                if (this.attributes.hasOwnProperty(key))
                    data["attributes"][key] = this.attributes[key];
            }
        }
        data["validator"] = this.validator ? this.validator.toJSON() : <any>undefined;
        return data;
    }
}

export interface IIInputType {
    name: string | undefined;
    attributes: { [key: string]: any; } | undefined;
    validator: IValueValidator | undefined;
}

export class TenantSettingsEditDto implements ITenantSettingsEditDto {
    general!: GeneralSettingsEditDto | undefined;
    userManagement!: TenantUserManagementSettingsEditDto;
    email!: EmailSettingsEditDto | undefined;
    ldap!: LdapSettingsEditDto | undefined;
    security!: SecuritySettingsEditDto;
    billing!: TenantBillingSettingsEditDto | undefined;

    constructor(data?: ITenantSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.userManagement = new TenantUserManagementSettingsEditDto();
            this.security = new SecuritySettingsEditDto();
        }
    }

    init(data?: any) {
        if (data) {
            this.general = data["general"] ? GeneralSettingsEditDto.fromJS(data["general"]) : <any>undefined;
            this.userManagement = data["userManagement"] ? TenantUserManagementSettingsEditDto.fromJS(data["userManagement"]) : new TenantUserManagementSettingsEditDto();
            this.email = data["email"] ? EmailSettingsEditDto.fromJS(data["email"]) : <any>undefined;
            this.ldap = data["ldap"] ? LdapSettingsEditDto.fromJS(data["ldap"]) : <any>undefined;
            this.security = data["security"] ? SecuritySettingsEditDto.fromJS(data["security"]) : new SecuritySettingsEditDto();
            this.billing = data["billing"] ? TenantBillingSettingsEditDto.fromJS(data["billing"]) : <any>undefined;
        }
    }

    static fromJS(data: any): TenantSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["general"] = this.general ? this.general.toJSON() : <any>undefined;
        data["userManagement"] = this.userManagement ? this.userManagement.toJSON() : <any>undefined;
        data["email"] = this.email ? this.email.toJSON() : <any>undefined;
        data["ldap"] = this.ldap ? this.ldap.toJSON() : <any>undefined;
        data["security"] = this.security ? this.security.toJSON() : <any>undefined;
        data["billing"] = this.billing ? this.billing.toJSON() : <any>undefined;
        return data;
    }
}

export interface ITenantSettingsEditDto {
    general: GeneralSettingsEditDto | undefined;
    userManagement: TenantUserManagementSettingsEditDto;
    email: EmailSettingsEditDto | undefined;
    ldap: LdapSettingsEditDto | undefined;
    security: SecuritySettingsEditDto;
    billing: TenantBillingSettingsEditDto | undefined;
}

export class TenantUserManagementSettingsEditDto implements ITenantUserManagementSettingsEditDto {
    allowSelfRegistration!: boolean | undefined;
    isNewRegisteredUserActiveByDefault!: boolean | undefined;
    isEmailConfirmationRequiredForLogin!: boolean | undefined;
    useCaptchaOnRegistration!: boolean | undefined;

    constructor(data?: ITenantUserManagementSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.allowSelfRegistration = data["allowSelfRegistration"];
            this.isNewRegisteredUserActiveByDefault = data["isNewRegisteredUserActiveByDefault"];
            this.isEmailConfirmationRequiredForLogin = data["isEmailConfirmationRequiredForLogin"];
            this.useCaptchaOnRegistration = data["useCaptchaOnRegistration"];
        }
    }

    static fromJS(data: any): TenantUserManagementSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantUserManagementSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["allowSelfRegistration"] = this.allowSelfRegistration;
        data["isNewRegisteredUserActiveByDefault"] = this.isNewRegisteredUserActiveByDefault;
        data["isEmailConfirmationRequiredForLogin"] = this.isEmailConfirmationRequiredForLogin;
        data["useCaptchaOnRegistration"] = this.useCaptchaOnRegistration;
        return data;
    }
}

export interface ITenantUserManagementSettingsEditDto {
    allowSelfRegistration: boolean | undefined;
    isNewRegisteredUserActiveByDefault: boolean | undefined;
    isEmailConfirmationRequiredForLogin: boolean | undefined;
    useCaptchaOnRegistration: boolean | undefined;
}

export class LdapSettingsEditDto implements ILdapSettingsEditDto {
    isModuleEnabled!: boolean | undefined;
    isEnabled!: boolean | undefined;
    domain!: string | undefined;
    userName!: string | undefined;
    password!: string | undefined;

    constructor(data?: ILdapSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.isModuleEnabled = data["isModuleEnabled"];
            this.isEnabled = data["isEnabled"];
            this.domain = data["domain"];
            this.userName = data["userName"];
            this.password = data["password"];
        }
    }

    static fromJS(data: any): LdapSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new LdapSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["isModuleEnabled"] = this.isModuleEnabled;
        data["isEnabled"] = this.isEnabled;
        data["domain"] = this.domain;
        data["userName"] = this.userName;
        data["password"] = this.password;
        return data;
    }
}

export interface ILdapSettingsEditDto {
    isModuleEnabled: boolean | undefined;
    isEnabled: boolean | undefined;
    domain: string | undefined;
    userName: string | undefined;
    password: string | undefined;
}

export class TenantBillingSettingsEditDto implements ITenantBillingSettingsEditDto {
    legalName!: string | undefined;
    address!: string | undefined;
    taxVatNo!: string | undefined;

    constructor(data?: ITenantBillingSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.legalName = data["legalName"];
            this.address = data["address"];
            this.taxVatNo = data["taxVatNo"];
        }
    }

    static fromJS(data: any): TenantBillingSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new TenantBillingSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["legalName"] = this.legalName;
        data["address"] = this.address;
        data["taxVatNo"] = this.taxVatNo;
        return data;
    }
}

export interface ITenantBillingSettingsEditDto {
    legalName: string | undefined;
    address: string | undefined;
    taxVatNo: string | undefined;
}

export class ListResultDtoOfNameValueDto implements IListResultDtoOfNameValueDto {
    items!: NameValueDto[] | undefined;

    constructor(data?: IListResultDtoOfNameValueDto) {
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
                    this.items.push(NameValueDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfNameValueDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfNameValueDto();
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

export interface IListResultDtoOfNameValueDto {
    items: NameValueDto[] | undefined;
}

export class AuthenticateModel implements IAuthenticateModel {
    userNameOrEmailAddress!: string;
    password!: string;
    twoFactorVerificationCode!: string | undefined;
    rememberClient!: boolean | undefined;
    twoFactorRememberClientToken!: string | undefined;
    singleSignIn!: boolean | undefined;
    returnUrl!: string | undefined;

    constructor(data?: IAuthenticateModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userNameOrEmailAddress = data["userNameOrEmailAddress"];
            this.password = data["password"];
            this.twoFactorVerificationCode = data["twoFactorVerificationCode"];
            this.rememberClient = data["rememberClient"];
            this.twoFactorRememberClientToken = data["twoFactorRememberClientToken"];
            this.singleSignIn = data["singleSignIn"];
            this.returnUrl = data["returnUrl"];
        }
    }

    static fromJS(data: any): AuthenticateModel {
        data = typeof data === 'object' ? data : {};
        let result = new AuthenticateModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userNameOrEmailAddress"] = this.userNameOrEmailAddress;
        data["password"] = this.password;
        data["twoFactorVerificationCode"] = this.twoFactorVerificationCode;
        data["rememberClient"] = this.rememberClient;
        data["twoFactorRememberClientToken"] = this.twoFactorRememberClientToken;
        data["singleSignIn"] = this.singleSignIn;
        data["returnUrl"] = this.returnUrl;
        return data;
    }
}

export interface IAuthenticateModel {
    userNameOrEmailAddress: string;
    password: string;
    twoFactorVerificationCode: string | undefined;
    rememberClient: boolean | undefined;
    twoFactorRememberClientToken: string | undefined;
    singleSignIn: boolean | undefined;
    returnUrl: string | undefined;
}

export class AuthenticateResultModel implements IAuthenticateResultModel {
    accessToken!: string | undefined;
    encryptedAccessToken!: string | undefined;
    expireInSeconds!: number | undefined;
    shouldResetPassword!: boolean | undefined;
    passwordResetCode!: string | undefined;
    userId!: number | undefined;
    requiresTwoFactorVerification!: boolean | undefined;
    twoFactorAuthProviders!: string[] | undefined;
    twoFactorRememberClientToken!: string | undefined;
    returnUrl!: string | undefined;

    constructor(data?: IAuthenticateResultModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.accessToken = data["accessToken"];
            this.encryptedAccessToken = data["encryptedAccessToken"];
            this.expireInSeconds = data["expireInSeconds"];
            this.shouldResetPassword = data["shouldResetPassword"];
            this.passwordResetCode = data["passwordResetCode"];
            this.userId = data["userId"];
            this.requiresTwoFactorVerification = data["requiresTwoFactorVerification"];
            if (data["twoFactorAuthProviders"] && data["twoFactorAuthProviders"].constructor === Array) {
                this.twoFactorAuthProviders = [];
                for (let item of data["twoFactorAuthProviders"])
                    this.twoFactorAuthProviders.push(item);
            }
            this.twoFactorRememberClientToken = data["twoFactorRememberClientToken"];
            this.returnUrl = data["returnUrl"];
        }
    }

    static fromJS(data: any): AuthenticateResultModel {
        data = typeof data === 'object' ? data : {};
        let result = new AuthenticateResultModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["accessToken"] = this.accessToken;
        data["encryptedAccessToken"] = this.encryptedAccessToken;
        data["expireInSeconds"] = this.expireInSeconds;
        data["shouldResetPassword"] = this.shouldResetPassword;
        data["passwordResetCode"] = this.passwordResetCode;
        data["userId"] = this.userId;
        data["requiresTwoFactorVerification"] = this.requiresTwoFactorVerification;
        if (this.twoFactorAuthProviders && this.twoFactorAuthProviders.constructor === Array) {
            data["twoFactorAuthProviders"] = [];
            for (let item of this.twoFactorAuthProviders)
                data["twoFactorAuthProviders"].push(item);
        }
        data["twoFactorRememberClientToken"] = this.twoFactorRememberClientToken;
        data["returnUrl"] = this.returnUrl;
        return data;
    }
}

export interface IAuthenticateResultModel {
    accessToken: string | undefined;
    encryptedAccessToken: string | undefined;
    expireInSeconds: number | undefined;
    shouldResetPassword: boolean | undefined;
    passwordResetCode: string | undefined;
    userId: number | undefined;
    requiresTwoFactorVerification: boolean | undefined;
    twoFactorAuthProviders: string[] | undefined;
    twoFactorRememberClientToken: string | undefined;
    returnUrl: string | undefined;
}

export class SendTwoFactorAuthCodeModel implements ISendTwoFactorAuthCodeModel {
    userId!: number | undefined;
    provider!: string;

    constructor(data?: ISendTwoFactorAuthCodeModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userId = data["userId"];
            this.provider = data["provider"];
        }
    }

    static fromJS(data: any): SendTwoFactorAuthCodeModel {
        data = typeof data === 'object' ? data : {};
        let result = new SendTwoFactorAuthCodeModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userId"] = this.userId;
        data["provider"] = this.provider;
        return data;
    }
}

export interface ISendTwoFactorAuthCodeModel {
    userId: number | undefined;
    provider: string;
}

export class ImpersonatedAuthenticateResultModel implements IImpersonatedAuthenticateResultModel {
    accessToken!: string | undefined;
    encryptedAccessToken!: string | undefined;
    expireInSeconds!: number | undefined;

    constructor(data?: IImpersonatedAuthenticateResultModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.accessToken = data["accessToken"];
            this.encryptedAccessToken = data["encryptedAccessToken"];
            this.expireInSeconds = data["expireInSeconds"];
        }
    }

    static fromJS(data: any): ImpersonatedAuthenticateResultModel {
        data = typeof data === 'object' ? data : {};
        let result = new ImpersonatedAuthenticateResultModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["accessToken"] = this.accessToken;
        data["encryptedAccessToken"] = this.encryptedAccessToken;
        data["expireInSeconds"] = this.expireInSeconds;
        return data;
    }
}

export interface IImpersonatedAuthenticateResultModel {
    accessToken: string | undefined;
    encryptedAccessToken: string | undefined;
    expireInSeconds: number | undefined;
}

export class SwitchedAccountAuthenticateResultModel implements ISwitchedAccountAuthenticateResultModel {
    accessToken!: string | undefined;
    encryptedAccessToken!: string | undefined;
    expireInSeconds!: number | undefined;

    constructor(data?: ISwitchedAccountAuthenticateResultModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.accessToken = data["accessToken"];
            this.encryptedAccessToken = data["encryptedAccessToken"];
            this.expireInSeconds = data["expireInSeconds"];
        }
    }

    static fromJS(data: any): SwitchedAccountAuthenticateResultModel {
        data = typeof data === 'object' ? data : {};
        let result = new SwitchedAccountAuthenticateResultModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["accessToken"] = this.accessToken;
        data["encryptedAccessToken"] = this.encryptedAccessToken;
        data["expireInSeconds"] = this.expireInSeconds;
        return data;
    }
}

export interface ISwitchedAccountAuthenticateResultModel {
    accessToken: string | undefined;
    encryptedAccessToken: string | undefined;
    expireInSeconds: number | undefined;
}

export class ExternalLoginProviderInfoModel implements IExternalLoginProviderInfoModel {
    name!: string | undefined;
    clientId!: string | undefined;
    additionalParams!: { [key: string]: string; } | undefined;

    constructor(data?: IExternalLoginProviderInfoModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.clientId = data["clientId"];
            if (data["additionalParams"]) {
                this.additionalParams = {};
                for (let key in data["additionalParams"]) {
                    if (data["additionalParams"].hasOwnProperty(key))
                        this.additionalParams[key] = data["additionalParams"][key];
                }
            }
        }
    }

    static fromJS(data: any): ExternalLoginProviderInfoModel {
        data = typeof data === 'object' ? data : {};
        let result = new ExternalLoginProviderInfoModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["clientId"] = this.clientId;
        if (this.additionalParams) {
            data["additionalParams"] = {};
            for (let key in this.additionalParams) {
                if (this.additionalParams.hasOwnProperty(key))
                    data["additionalParams"][key] = this.additionalParams[key];
            }
        }
        return data;
    }
}

export interface IExternalLoginProviderInfoModel {
    name: string | undefined;
    clientId: string | undefined;
    additionalParams: { [key: string]: string; } | undefined;
}

export class ExternalAuthenticateModel implements IExternalAuthenticateModel {
    authProvider!: string;
    providerKey!: string;
    providerAccessCode!: string;
    returnUrl!: string | undefined;
    singleSignIn!: boolean | undefined;

    constructor(data?: IExternalAuthenticateModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.authProvider = data["authProvider"];
            this.providerKey = data["providerKey"];
            this.providerAccessCode = data["providerAccessCode"];
            this.returnUrl = data["returnUrl"];
            this.singleSignIn = data["singleSignIn"];
        }
    }

    static fromJS(data: any): ExternalAuthenticateModel {
        data = typeof data === 'object' ? data : {};
        let result = new ExternalAuthenticateModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["authProvider"] = this.authProvider;
        data["providerKey"] = this.providerKey;
        data["providerAccessCode"] = this.providerAccessCode;
        data["returnUrl"] = this.returnUrl;
        data["singleSignIn"] = this.singleSignIn;
        return data;
    }
}

export interface IExternalAuthenticateModel {
    authProvider: string;
    providerKey: string;
    providerAccessCode: string;
    returnUrl: string | undefined;
    singleSignIn: boolean | undefined;
}

export class ExternalAuthenticateResultModel implements IExternalAuthenticateResultModel {
    accessToken!: string | undefined;
    encryptedAccessToken!: string | undefined;
    expireInSeconds!: number | undefined;
    waitingForActivation!: boolean | undefined;
    returnUrl!: string | undefined;

    constructor(data?: IExternalAuthenticateResultModel) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.accessToken = data["accessToken"];
            this.encryptedAccessToken = data["encryptedAccessToken"];
            this.expireInSeconds = data["expireInSeconds"];
            this.waitingForActivation = data["waitingForActivation"];
            this.returnUrl = data["returnUrl"];
        }
    }

    static fromJS(data: any): ExternalAuthenticateResultModel {
        data = typeof data === 'object' ? data : {};
        let result = new ExternalAuthenticateResultModel();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["accessToken"] = this.accessToken;
        data["encryptedAccessToken"] = this.encryptedAccessToken;
        data["expireInSeconds"] = this.expireInSeconds;
        data["waitingForActivation"] = this.waitingForActivation;
        data["returnUrl"] = this.returnUrl;
        return data;
    }
}

export interface IExternalAuthenticateResultModel {
    accessToken: string | undefined;
    encryptedAccessToken: string | undefined;
    expireInSeconds: number | undefined;
    waitingForActivation: boolean | undefined;
    returnUrl: string | undefined;
}

export class PagedResultDtoOfTransactionDto implements IPagedResultDtoOfTransactionDto {
    totalCount!: number | undefined;
    items!: TransactionDto[] | undefined;

    constructor(data?: IPagedResultDtoOfTransactionDto) {
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
                    this.items.push(TransactionDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfTransactionDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfTransactionDto();
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
}

export interface IPagedResultDtoOfTransactionDto {
    totalCount: number | undefined;
    items: TransactionDto[] | undefined;
}

export class TransactionDto implements ITransactionDto {
    id!: number | undefined;
    tranCode!: string | undefined;
    buyer!: string | undefined;
    paymentTime!: moment.Moment | undefined;
    amount!: number | undefined;
    platesQuantity!: number | undefined;
    states!: string | undefined;
    dishes!: DishTransactionDto[] | undefined;
    session!: string | undefined;
    transactionId!: string | undefined;
    beginTranImage!: string | undefined;
    endTranImage!: string | undefined;
    cardLabel!: string | undefined;
    cardNumber!: string | undefined;
    approveCode!: string | undefined;

    constructor(data?: ITransactionDto) {
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
            this.tranCode = data["tranCode"];
            this.buyer = data["buyer"];
            this.paymentTime = data["paymentTime"] ? moment(data["paymentTime"].toString()) : <any>undefined;
            this.amount = data["amount"];
            this.platesQuantity = data["platesQuantity"];
            this.states = data["states"];
            if (data["dishes"] && data["dishes"].constructor === Array) {
                this.dishes = [];
                for (let item of data["dishes"])
                    this.dishes.push(DishTransactionDto.fromJS(item));
            }
            this.session = data["session"];
            this.transactionId = data["transactionId"];
            this.beginTranImage = data["beginTranImage"];
            this.endTranImage = data["endTranImage"];
            this.cardLabel = data["cardLabel"];
            this.cardNumber = data["cardNumber"];
            this.approveCode = data["approveCode"];
        }
    }

    static fromJS(data: any): TransactionDto {
        data = typeof data === 'object' ? data : {};
        let result = new TransactionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["tranCode"] = this.tranCode;
        data["buyer"] = this.buyer;
        data["paymentTime"] = this.paymentTime ? this.paymentTime.toISOString() : <any>undefined;
        data["amount"] = this.amount;
        data["platesQuantity"] = this.platesQuantity;
        data["states"] = this.states;
        if (this.dishes && this.dishes.constructor === Array) {
            data["dishes"] = [];
            for (let item of this.dishes)
                data["dishes"].push(item.toJSON());
        }
        data["session"] = this.session;
        data["transactionId"] = this.transactionId;
        data["beginTranImage"] = this.beginTranImage;
        data["endTranImage"] = this.endTranImage;
        data["cardLabel"] = this.cardLabel;
        data["cardNumber"] = this.cardNumber;
        data["approveCode"] = this.approveCode;
        return data;
    }
}

export interface ITransactionDto {
    id: number | undefined;
    tranCode: string | undefined;
    buyer: string | undefined;
    paymentTime: moment.Moment | undefined;
    amount: number | undefined;
    platesQuantity: number | undefined;
    states: string | undefined;
    dishes: DishTransactionDto[] | undefined;
    session: string | undefined;
    transactionId: string | undefined;
    beginTranImage: string | undefined;
    endTranImage: string | undefined;
    cardLabel: string | undefined;
    cardNumber: string | undefined;
    approveCode: string | undefined;
}

export class DishTransactionDto implements IDishTransactionDto {
    disc!: Disc | undefined;
    amount!: number | undefined;

    constructor(data?: IDishTransactionDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.disc = data["disc"] ? Disc.fromJS(data["disc"]) : <any>undefined;
            this.amount = data["amount"];
        }
    }

    static fromJS(data: any): DishTransactionDto {
        data = typeof data === 'object' ? data : {};
        let result = new DishTransactionDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["disc"] = this.disc ? this.disc.toJSON() : <any>undefined;
        data["amount"] = this.amount;
        return data;
    }
}

export interface IDishTransactionDto {
    disc: Disc | undefined;
    amount: number | undefined;
}

export class UiCustomizationSettingsEditDto implements IUiCustomizationSettingsEditDto {
    layout!: UiCustomizationLayoutSettingsEditDto | undefined;
    header!: UiCustomizationHeaderSettingsEditDto | undefined;
    menu!: UiCustomizationMenuSettingsEditDto | undefined;
    footer!: UiCustomizationFooterSettingsEditDto | undefined;

    constructor(data?: IUiCustomizationSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.layout = data["layout"] ? UiCustomizationLayoutSettingsEditDto.fromJS(data["layout"]) : <any>undefined;
            this.header = data["header"] ? UiCustomizationHeaderSettingsEditDto.fromJS(data["header"]) : <any>undefined;
            this.menu = data["menu"] ? UiCustomizationMenuSettingsEditDto.fromJS(data["menu"]) : <any>undefined;
            this.footer = data["footer"] ? UiCustomizationFooterSettingsEditDto.fromJS(data["footer"]) : <any>undefined;
        }
    }

    static fromJS(data: any): UiCustomizationSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UiCustomizationSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["layout"] = this.layout ? this.layout.toJSON() : <any>undefined;
        data["header"] = this.header ? this.header.toJSON() : <any>undefined;
        data["menu"] = this.menu ? this.menu.toJSON() : <any>undefined;
        data["footer"] = this.footer ? this.footer.toJSON() : <any>undefined;
        return data;
    }
}

export interface IUiCustomizationSettingsEditDto {
    layout: UiCustomizationLayoutSettingsEditDto | undefined;
    header: UiCustomizationHeaderSettingsEditDto | undefined;
    menu: UiCustomizationMenuSettingsEditDto | undefined;
    footer: UiCustomizationFooterSettingsEditDto | undefined;
}

export class UiCustomizationLayoutSettingsEditDto implements IUiCustomizationLayoutSettingsEditDto {
    layoutType!: string | undefined;
    contentSkin!: string | undefined;
    theme!: string | undefined;
    themeColor!: string | undefined;

    constructor(data?: IUiCustomizationLayoutSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.layoutType = data["layoutType"];
            this.contentSkin = data["contentSkin"];
            this.theme = data["theme"];
            this.themeColor = data["themeColor"];
        }
    }

    static fromJS(data: any): UiCustomizationLayoutSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UiCustomizationLayoutSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["layoutType"] = this.layoutType;
        data["contentSkin"] = this.contentSkin;
        data["theme"] = this.theme;
        data["themeColor"] = this.themeColor;
        return data;
    }
}

export interface IUiCustomizationLayoutSettingsEditDto {
    layoutType: string | undefined;
    contentSkin: string | undefined;
    theme: string | undefined;
    themeColor: string | undefined;
}

export class UiCustomizationHeaderSettingsEditDto implements IUiCustomizationHeaderSettingsEditDto {
    desktopFixedHeader!: boolean | undefined;
    mobileFixedHeader!: boolean | undefined;
    headerSkin!: string | undefined;

    constructor(data?: IUiCustomizationHeaderSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.desktopFixedHeader = data["desktopFixedHeader"];
            this.mobileFixedHeader = data["mobileFixedHeader"];
            this.headerSkin = data["headerSkin"];
        }
    }

    static fromJS(data: any): UiCustomizationHeaderSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UiCustomizationHeaderSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["desktopFixedHeader"] = this.desktopFixedHeader;
        data["mobileFixedHeader"] = this.mobileFixedHeader;
        data["headerSkin"] = this.headerSkin;
        return data;
    }
}

export interface IUiCustomizationHeaderSettingsEditDto {
    desktopFixedHeader: boolean | undefined;
    mobileFixedHeader: boolean | undefined;
    headerSkin: string | undefined;
}

export class UiCustomizationMenuSettingsEditDto implements IUiCustomizationMenuSettingsEditDto {
    position!: string | undefined;
    asideSkin!: string | undefined;
    fixedAside!: boolean | undefined;
    allowAsideMinimizing!: boolean | undefined;
    defaultMinimizedAside!: boolean | undefined;
    allowAsideHiding!: boolean | undefined;
    defaultHiddenAside!: boolean | undefined;

    constructor(data?: IUiCustomizationMenuSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.position = data["position"];
            this.asideSkin = data["asideSkin"];
            this.fixedAside = data["fixedAside"];
            this.allowAsideMinimizing = data["allowAsideMinimizing"];
            this.defaultMinimizedAside = data["defaultMinimizedAside"];
            this.allowAsideHiding = data["allowAsideHiding"];
            this.defaultHiddenAside = data["defaultHiddenAside"];
        }
    }

    static fromJS(data: any): UiCustomizationMenuSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UiCustomizationMenuSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["position"] = this.position;
        data["asideSkin"] = this.asideSkin;
        data["fixedAside"] = this.fixedAside;
        data["allowAsideMinimizing"] = this.allowAsideMinimizing;
        data["defaultMinimizedAside"] = this.defaultMinimizedAside;
        data["allowAsideHiding"] = this.allowAsideHiding;
        data["defaultHiddenAside"] = this.defaultHiddenAside;
        return data;
    }
}

export interface IUiCustomizationMenuSettingsEditDto {
    position: string | undefined;
    asideSkin: string | undefined;
    fixedAside: boolean | undefined;
    allowAsideMinimizing: boolean | undefined;
    defaultMinimizedAside: boolean | undefined;
    allowAsideHiding: boolean | undefined;
    defaultHiddenAside: boolean | undefined;
}

export class UiCustomizationFooterSettingsEditDto implements IUiCustomizationFooterSettingsEditDto {
    fixedFooter!: boolean | undefined;

    constructor(data?: IUiCustomizationFooterSettingsEditDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.fixedFooter = data["fixedFooter"];
        }
    }

    static fromJS(data: any): UiCustomizationFooterSettingsEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UiCustomizationFooterSettingsEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["fixedFooter"] = this.fixedFooter;
        return data;
    }
}

export interface IUiCustomizationFooterSettingsEditDto {
    fixedFooter: boolean | undefined;
}

export class CreateUserDto implements ICreateUserDto {
    userName!: string;
    name!: string;
    surname!: string;
    emailAddress!: string;
    isActive!: boolean | undefined;
    roleNames!: string[] | undefined;
    password!: string;

    constructor(data?: ICreateUserDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userName = data["userName"];
            this.name = data["name"];
            this.surname = data["surname"];
            this.emailAddress = data["emailAddress"];
            this.isActive = data["isActive"];
            if (data["roleNames"] && data["roleNames"].constructor === Array) {
                this.roleNames = [];
                for (let item of data["roleNames"])
                    this.roleNames.push(item);
            }
            this.password = data["password"];
        }
    }

    static fromJS(data: any): CreateUserDto {
        data = typeof data === 'object' ? data : {};
        let result = new CreateUserDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userName"] = this.userName;
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["emailAddress"] = this.emailAddress;
        data["isActive"] = this.isActive;
        if (this.roleNames && this.roleNames.constructor === Array) {
            data["roleNames"] = [];
            for (let item of this.roleNames)
                data["roleNames"].push(item);
        }
        data["password"] = this.password;
        return data;
    }
}

export interface ICreateUserDto {
    userName: string;
    name: string;
    surname: string;
    emailAddress: string;
    isActive: boolean | undefined;
    roleNames: string[] | undefined;
    password: string;
}

export class UserDto implements IUserDto {
    userName!: string;
    name!: string;
    surname!: string;
    emailAddress!: string;
    isActive!: boolean | undefined;
    fullName!: string | undefined;
    lastLoginTime!: moment.Moment | undefined;
    creationTime!: moment.Moment | undefined;
    roleNames!: string[] | undefined;
    id!: number | undefined;

    constructor(data?: IUserDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.userName = data["userName"];
            this.name = data["name"];
            this.surname = data["surname"];
            this.emailAddress = data["emailAddress"];
            this.isActive = data["isActive"];
            this.fullName = data["fullName"];
            this.lastLoginTime = data["lastLoginTime"] ? moment(data["lastLoginTime"].toString()) : <any>undefined;
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            if (data["roleNames"] && data["roleNames"].constructor === Array) {
                this.roleNames = [];
                for (let item of data["roleNames"])
                    this.roleNames.push(item);
            }
            this.id = data["id"];
        }
    }

    static fromJS(data: any): UserDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["userName"] = this.userName;
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["emailAddress"] = this.emailAddress;
        data["isActive"] = this.isActive;
        data["fullName"] = this.fullName;
        data["lastLoginTime"] = this.lastLoginTime ? this.lastLoginTime.toISOString() : <any>undefined;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        if (this.roleNames && this.roleNames.constructor === Array) {
            data["roleNames"] = [];
            for (let item of this.roleNames)
                data["roleNames"].push(item);
        }
        data["id"] = this.id;
        return data;
    }
}

export interface IUserDto {
    userName: string;
    name: string;
    surname: string;
    emailAddress: string;
    isActive: boolean | undefined;
    fullName: string | undefined;
    lastLoginTime: moment.Moment | undefined;
    creationTime: moment.Moment | undefined;
    roleNames: string[] | undefined;
    id: number | undefined;
}

export class ListResultDtoOfRoleDto implements IListResultDtoOfRoleDto {
    items!: RoleDto[] | undefined;

    constructor(data?: IListResultDtoOfRoleDto) {
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
                    this.items.push(RoleDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfRoleDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfRoleDto();
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

export interface IListResultDtoOfRoleDto {
    items: RoleDto[] | undefined;
}

export class PagedResultDtoOfUserDto implements IPagedResultDtoOfUserDto {
    totalCount!: number | undefined;
    items!: UserDto[] | undefined;

    constructor(data?: IPagedResultDtoOfUserDto) {
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
                    this.items.push(UserDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfUserDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfUserDto();
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
}

export interface IPagedResultDtoOfUserDto {
    totalCount: number | undefined;
    items: UserDto[] | undefined;
}

export class PagedResultDtoOfUserListDto implements IPagedResultDtoOfUserListDto {
    totalCount!: number | undefined;
    items!: UserListDto[] | undefined;

    constructor(data?: IPagedResultDtoOfUserListDto) {
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
                    this.items.push(UserListDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfUserListDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfUserListDto();
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
}

export interface IPagedResultDtoOfUserListDto {
    totalCount: number | undefined;
    items: UserListDto[] | undefined;
}

export class UserListDto implements IUserListDto {
    name!: string | undefined;
    surname!: string | undefined;
    userName!: string | undefined;
    emailAddress!: string | undefined;
    phoneNumber!: string | undefined;
    profilePictureId!: string | undefined;
    isEmailConfirmed!: boolean | undefined;
    roles!: UserListRoleDto[] | undefined;
    lastLoginTime!: moment.Moment | undefined;
    isActive!: boolean | undefined;
    creationTime!: moment.Moment | undefined;
    id!: number | undefined;

    constructor(data?: IUserListDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.name = data["name"];
            this.surname = data["surname"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.phoneNumber = data["phoneNumber"];
            this.profilePictureId = data["profilePictureId"];
            this.isEmailConfirmed = data["isEmailConfirmed"];
            if (data["roles"] && data["roles"].constructor === Array) {
                this.roles = [];
                for (let item of data["roles"])
                    this.roles.push(UserListRoleDto.fromJS(item));
            }
            this.lastLoginTime = data["lastLoginTime"] ? moment(data["lastLoginTime"].toString()) : <any>undefined;
            this.isActive = data["isActive"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): UserListDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserListDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["phoneNumber"] = this.phoneNumber;
        data["profilePictureId"] = this.profilePictureId;
        data["isEmailConfirmed"] = this.isEmailConfirmed;
        if (this.roles && this.roles.constructor === Array) {
            data["roles"] = [];
            for (let item of this.roles)
                data["roles"].push(item.toJSON());
        }
        data["lastLoginTime"] = this.lastLoginTime ? this.lastLoginTime.toISOString() : <any>undefined;
        data["isActive"] = this.isActive;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface IUserListDto {
    name: string | undefined;
    surname: string | undefined;
    userName: string | undefined;
    emailAddress: string | undefined;
    phoneNumber: string | undefined;
    profilePictureId: string | undefined;
    isEmailConfirmed: boolean | undefined;
    roles: UserListRoleDto[] | undefined;
    lastLoginTime: moment.Moment | undefined;
    isActive: boolean | undefined;
    creationTime: moment.Moment | undefined;
    id: number | undefined;
}

export class UserListRoleDto implements IUserListRoleDto {
    roleId!: number | undefined;
    roleName!: string | undefined;

    constructor(data?: IUserListRoleDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.roleId = data["roleId"];
            this.roleName = data["roleName"];
        }
    }

    static fromJS(data: any): UserListRoleDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserListRoleDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["roleId"] = this.roleId;
        data["roleName"] = this.roleName;
        return data;
    }
}

export interface IUserListRoleDto {
    roleId: number | undefined;
    roleName: string | undefined;
}

export class GetUserForEditOutput implements IGetUserForEditOutput {
    profilePictureId!: string | undefined;
    user!: UserEditDto | undefined;
    roles!: UserRoleDto[] | undefined;
    allOrganizationUnits!: OrganizationUnitDto[] | undefined;
    memberedOrganizationUnits!: string[] | undefined;

    constructor(data?: IGetUserForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.profilePictureId = data["profilePictureId"];
            this.user = data["user"] ? UserEditDto.fromJS(data["user"]) : <any>undefined;
            if (data["roles"] && data["roles"].constructor === Array) {
                this.roles = [];
                for (let item of data["roles"])
                    this.roles.push(UserRoleDto.fromJS(item));
            }
            if (data["allOrganizationUnits"] && data["allOrganizationUnits"].constructor === Array) {
                this.allOrganizationUnits = [];
                for (let item of data["allOrganizationUnits"])
                    this.allOrganizationUnits.push(OrganizationUnitDto.fromJS(item));
            }
            if (data["memberedOrganizationUnits"] && data["memberedOrganizationUnits"].constructor === Array) {
                this.memberedOrganizationUnits = [];
                for (let item of data["memberedOrganizationUnits"])
                    this.memberedOrganizationUnits.push(item);
            }
        }
    }

    static fromJS(data: any): GetUserForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetUserForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["profilePictureId"] = this.profilePictureId;
        data["user"] = this.user ? this.user.toJSON() : <any>undefined;
        if (this.roles && this.roles.constructor === Array) {
            data["roles"] = [];
            for (let item of this.roles)
                data["roles"].push(item.toJSON());
        }
        if (this.allOrganizationUnits && this.allOrganizationUnits.constructor === Array) {
            data["allOrganizationUnits"] = [];
            for (let item of this.allOrganizationUnits)
                data["allOrganizationUnits"].push(item.toJSON());
        }
        if (this.memberedOrganizationUnits && this.memberedOrganizationUnits.constructor === Array) {
            data["memberedOrganizationUnits"] = [];
            for (let item of this.memberedOrganizationUnits)
                data["memberedOrganizationUnits"].push(item);
        }
        return data;
    }
}

export interface IGetUserForEditOutput {
    profilePictureId: string | undefined;
    user: UserEditDto | undefined;
    roles: UserRoleDto[] | undefined;
    allOrganizationUnits: OrganizationUnitDto[] | undefined;
    memberedOrganizationUnits: string[] | undefined;
}

export class UserEditDto implements IUserEditDto {
    id!: number | undefined;
    name!: string;
    surname!: string;
    userName!: string;
    emailAddress!: string;
    phoneNumber!: string | undefined;
    password!: string | undefined;
    isActive!: boolean | undefined;
    shouldChangePasswordOnNextLogin!: boolean | undefined;
    isTwoFactorEnabled!: boolean | undefined;
    isLockoutEnabled!: boolean | undefined;

    constructor(data?: IUserEditDto) {
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
            this.surname = data["surname"];
            this.userName = data["userName"];
            this.emailAddress = data["emailAddress"];
            this.phoneNumber = data["phoneNumber"];
            this.password = data["password"];
            this.isActive = data["isActive"];
            this.shouldChangePasswordOnNextLogin = data["shouldChangePasswordOnNextLogin"];
            this.isTwoFactorEnabled = data["isTwoFactorEnabled"];
            this.isLockoutEnabled = data["isLockoutEnabled"];
        }
    }

    static fromJS(data: any): UserEditDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserEditDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        data["name"] = this.name;
        data["surname"] = this.surname;
        data["userName"] = this.userName;
        data["emailAddress"] = this.emailAddress;
        data["phoneNumber"] = this.phoneNumber;
        data["password"] = this.password;
        data["isActive"] = this.isActive;
        data["shouldChangePasswordOnNextLogin"] = this.shouldChangePasswordOnNextLogin;
        data["isTwoFactorEnabled"] = this.isTwoFactorEnabled;
        data["isLockoutEnabled"] = this.isLockoutEnabled;
        return data;
    }
}

export interface IUserEditDto {
    id: number | undefined;
    name: string;
    surname: string;
    userName: string;
    emailAddress: string;
    phoneNumber: string | undefined;
    password: string | undefined;
    isActive: boolean | undefined;
    shouldChangePasswordOnNextLogin: boolean | undefined;
    isTwoFactorEnabled: boolean | undefined;
    isLockoutEnabled: boolean | undefined;
}

export class UserRoleDto implements IUserRoleDto {
    roleId!: number | undefined;
    roleName!: string | undefined;
    roleDisplayName!: string | undefined;
    isAssigned!: boolean | undefined;

    constructor(data?: IUserRoleDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.roleId = data["roleId"];
            this.roleName = data["roleName"];
            this.roleDisplayName = data["roleDisplayName"];
            this.isAssigned = data["isAssigned"];
        }
    }

    static fromJS(data: any): UserRoleDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserRoleDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["roleId"] = this.roleId;
        data["roleName"] = this.roleName;
        data["roleDisplayName"] = this.roleDisplayName;
        data["isAssigned"] = this.isAssigned;
        return data;
    }
}

export interface IUserRoleDto {
    roleId: number | undefined;
    roleName: string | undefined;
    roleDisplayName: string | undefined;
    isAssigned: boolean | undefined;
}

export class GetUserPermissionsForEditOutput implements IGetUserPermissionsForEditOutput {
    permissions!: FlatPermissionDto[] | undefined;
    grantedPermissionNames!: string[] | undefined;

    constructor(data?: IGetUserPermissionsForEditOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["permissions"] && data["permissions"].constructor === Array) {
                this.permissions = [];
                for (let item of data["permissions"])
                    this.permissions.push(FlatPermissionDto.fromJS(item));
            }
            if (data["grantedPermissionNames"] && data["grantedPermissionNames"].constructor === Array) {
                this.grantedPermissionNames = [];
                for (let item of data["grantedPermissionNames"])
                    this.grantedPermissionNames.push(item);
            }
        }
    }

    static fromJS(data: any): GetUserPermissionsForEditOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetUserPermissionsForEditOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.permissions && this.permissions.constructor === Array) {
            data["permissions"] = [];
            for (let item of this.permissions)
                data["permissions"].push(item.toJSON());
        }
        if (this.grantedPermissionNames && this.grantedPermissionNames.constructor === Array) {
            data["grantedPermissionNames"] = [];
            for (let item of this.grantedPermissionNames)
                data["grantedPermissionNames"].push(item);
        }
        return data;
    }
}

export interface IGetUserPermissionsForEditOutput {
    permissions: FlatPermissionDto[] | undefined;
    grantedPermissionNames: string[] | undefined;
}

export class EntityDtoOfInt64 implements IEntityDtoOfInt64 {
    id!: number | undefined;

    constructor(data?: IEntityDtoOfInt64) {
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
        }
    }

    static fromJS(data: any): EntityDtoOfInt64 {
        data = typeof data === 'object' ? data : {};
        let result = new EntityDtoOfInt64();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        return data;
    }
}

export interface IEntityDtoOfInt64 {
    id: number | undefined;
}

export class UpdateUserPermissionsInput implements IUpdateUserPermissionsInput {
    id!: number | undefined;
    grantedPermissionNames!: string[];

    constructor(data?: IUpdateUserPermissionsInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.grantedPermissionNames = [];
        }
    }

    init(data?: any) {
        if (data) {
            this.id = data["id"];
            if (data["grantedPermissionNames"] && data["grantedPermissionNames"].constructor === Array) {
                this.grantedPermissionNames = [];
                for (let item of data["grantedPermissionNames"])
                    this.grantedPermissionNames.push(item);
            }
        }
    }

    static fromJS(data: any): UpdateUserPermissionsInput {
        data = typeof data === 'object' ? data : {};
        let result = new UpdateUserPermissionsInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["id"] = this.id;
        if (this.grantedPermissionNames && this.grantedPermissionNames.constructor === Array) {
            data["grantedPermissionNames"] = [];
            for (let item of this.grantedPermissionNames)
                data["grantedPermissionNames"].push(item);
        }
        return data;
    }
}

export interface IUpdateUserPermissionsInput {
    id: number | undefined;
    grantedPermissionNames: string[];
}

export class CreateOrUpdateUserInput implements ICreateOrUpdateUserInput {
    user!: UserEditDto;
    assignedRoleNames!: string[];
    sendActivationEmail!: boolean | undefined;
    setRandomPassword!: boolean | undefined;
    organizationUnits!: number[] | undefined;

    constructor(data?: ICreateOrUpdateUserInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
        if (!data) {
            this.user = new UserEditDto();
            this.assignedRoleNames = [];
        }
    }

    init(data?: any) {
        if (data) {
            this.user = data["user"] ? UserEditDto.fromJS(data["user"]) : new UserEditDto();
            if (data["assignedRoleNames"] && data["assignedRoleNames"].constructor === Array) {
                this.assignedRoleNames = [];
                for (let item of data["assignedRoleNames"])
                    this.assignedRoleNames.push(item);
            }
            this.sendActivationEmail = data["sendActivationEmail"];
            this.setRandomPassword = data["setRandomPassword"];
            if (data["organizationUnits"] && data["organizationUnits"].constructor === Array) {
                this.organizationUnits = [];
                for (let item of data["organizationUnits"])
                    this.organizationUnits.push(item);
            }
        }
    }

    static fromJS(data: any): CreateOrUpdateUserInput {
        data = typeof data === 'object' ? data : {};
        let result = new CreateOrUpdateUserInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["user"] = this.user ? this.user.toJSON() : <any>undefined;
        if (this.assignedRoleNames && this.assignedRoleNames.constructor === Array) {
            data["assignedRoleNames"] = [];
            for (let item of this.assignedRoleNames)
                data["assignedRoleNames"].push(item);
        }
        data["sendActivationEmail"] = this.sendActivationEmail;
        data["setRandomPassword"] = this.setRandomPassword;
        if (this.organizationUnits && this.organizationUnits.constructor === Array) {
            data["organizationUnits"] = [];
            for (let item of this.organizationUnits)
                data["organizationUnits"].push(item);
        }
        return data;
    }
}

export interface ICreateOrUpdateUserInput {
    user: UserEditDto;
    assignedRoleNames: string[];
    sendActivationEmail: boolean | undefined;
    setRandomPassword: boolean | undefined;
    organizationUnits: number[] | undefined;
}

export class LinkToUserInput implements ILinkToUserInput {
    tenancyName!: string | undefined;
    usernameOrEmailAddress!: string;
    password!: string;

    constructor(data?: ILinkToUserInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.usernameOrEmailAddress = data["usernameOrEmailAddress"];
            this.password = data["password"];
        }
    }

    static fromJS(data: any): LinkToUserInput {
        data = typeof data === 'object' ? data : {};
        let result = new LinkToUserInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["usernameOrEmailAddress"] = this.usernameOrEmailAddress;
        data["password"] = this.password;
        return data;
    }
}

export interface ILinkToUserInput {
    tenancyName: string | undefined;
    usernameOrEmailAddress: string;
    password: string;
}

export class PagedResultDtoOfLinkedUserDto implements IPagedResultDtoOfLinkedUserDto {
    totalCount!: number | undefined;
    items!: LinkedUserDto[] | undefined;

    constructor(data?: IPagedResultDtoOfLinkedUserDto) {
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
                    this.items.push(LinkedUserDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): PagedResultDtoOfLinkedUserDto {
        data = typeof data === 'object' ? data : {};
        let result = new PagedResultDtoOfLinkedUserDto();
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
}

export interface IPagedResultDtoOfLinkedUserDto {
    totalCount: number | undefined;
    items: LinkedUserDto[] | undefined;
}

export class LinkedUserDto implements ILinkedUserDto {
    tenantId!: number | undefined;
    tenancyName!: string | undefined;
    username!: string | undefined;
    lastLoginTime!: moment.Moment | undefined;
    id!: number | undefined;

    constructor(data?: ILinkedUserDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.tenancyName = data["tenancyName"];
            this.username = data["username"];
            this.lastLoginTime = data["lastLoginTime"] ? moment(data["lastLoginTime"].toString()) : <any>undefined;
            this.id = data["id"];
        }
    }

    static fromJS(data: any): LinkedUserDto {
        data = typeof data === 'object' ? data : {};
        let result = new LinkedUserDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["tenancyName"] = this.tenancyName;
        data["username"] = this.username;
        data["lastLoginTime"] = this.lastLoginTime ? this.lastLoginTime.toISOString() : <any>undefined;
        data["id"] = this.id;
        return data;
    }
}

export interface ILinkedUserDto {
    tenantId: number | undefined;
    tenancyName: string | undefined;
    username: string | undefined;
    lastLoginTime: moment.Moment | undefined;
    id: number | undefined;
}

export class ListResultDtoOfLinkedUserDto implements IListResultDtoOfLinkedUserDto {
    items!: LinkedUserDto[] | undefined;

    constructor(data?: IListResultDtoOfLinkedUserDto) {
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
                    this.items.push(LinkedUserDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfLinkedUserDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfLinkedUserDto();
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

export interface IListResultDtoOfLinkedUserDto {
    items: LinkedUserDto[] | undefined;
}

export class UnlinkUserInput implements IUnlinkUserInput {
    tenantId!: number | undefined;
    userId!: number | undefined;

    constructor(data?: IUnlinkUserInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenantId = data["tenantId"];
            this.userId = data["userId"];
        }
    }

    static fromJS(data: any): UnlinkUserInput {
        data = typeof data === 'object' ? data : {};
        let result = new UnlinkUserInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenantId"] = this.tenantId;
        data["userId"] = this.userId;
        return data;
    }
}

export interface IUnlinkUserInput {
    tenantId: number | undefined;
    userId: number | undefined;
}

export class ListResultDtoOfUserLoginAttemptDto implements IListResultDtoOfUserLoginAttemptDto {
    items!: UserLoginAttemptDto[] | undefined;

    constructor(data?: IListResultDtoOfUserLoginAttemptDto) {
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
                    this.items.push(UserLoginAttemptDto.fromJS(item));
            }
        }
    }

    static fromJS(data: any): ListResultDtoOfUserLoginAttemptDto {
        data = typeof data === 'object' ? data : {};
        let result = new ListResultDtoOfUserLoginAttemptDto();
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

export interface IListResultDtoOfUserLoginAttemptDto {
    items: UserLoginAttemptDto[] | undefined;
}

export class UserLoginAttemptDto implements IUserLoginAttemptDto {
    tenancyName!: string | undefined;
    userNameOrEmail!: string | undefined;
    clientIpAddress!: string | undefined;
    clientName!: string | undefined;
    browserInfo!: string | undefined;
    result!: string | undefined;
    creationTime!: moment.Moment | undefined;

    constructor(data?: IUserLoginAttemptDto) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.tenancyName = data["tenancyName"];
            this.userNameOrEmail = data["userNameOrEmail"];
            this.clientIpAddress = data["clientIpAddress"];
            this.clientName = data["clientName"];
            this.browserInfo = data["browserInfo"];
            this.result = data["result"];
            this.creationTime = data["creationTime"] ? moment(data["creationTime"].toString()) : <any>undefined;
        }
    }

    static fromJS(data: any): UserLoginAttemptDto {
        data = typeof data === 'object' ? data : {};
        let result = new UserLoginAttemptDto();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["tenancyName"] = this.tenancyName;
        data["userNameOrEmail"] = this.userNameOrEmail;
        data["clientIpAddress"] = this.clientIpAddress;
        data["clientName"] = this.clientName;
        data["browserInfo"] = this.browserInfo;
        data["result"] = this.result;
        data["creationTime"] = this.creationTime ? this.creationTime.toISOString() : <any>undefined;
        return data;
    }
}

export interface IUserLoginAttemptDto {
    tenancyName: string | undefined;
    userNameOrEmail: string | undefined;
    clientIpAddress: string | undefined;
    clientName: string | undefined;
    browserInfo: string | undefined;
    result: string | undefined;
    creationTime: moment.Moment | undefined;
}

export class GetLatestWebLogsOutput implements IGetLatestWebLogsOutput {
    latestWebLogLines!: string[] | undefined;

    constructor(data?: IGetLatestWebLogsOutput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["latestWebLogLines"] && data["latestWebLogLines"].constructor === Array) {
                this.latestWebLogLines = [];
                for (let item of data["latestWebLogLines"])
                    this.latestWebLogLines.push(item);
            }
        }
    }

    static fromJS(data: any): GetLatestWebLogsOutput {
        data = typeof data === 'object' ? data : {};
        let result = new GetLatestWebLogsOutput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.latestWebLogLines && this.latestWebLogLines.constructor === Array) {
            data["latestWebLogLines"] = [];
            for (let item of this.latestWebLogLines)
                data["latestWebLogLines"].push(item);
        }
        return data;
    }
}

export interface IGetLatestWebLogsOutput {
    latestWebLogLines: string[] | undefined;
}

export enum SalesDatePeriod {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum SalesDatePeriod2 {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum IncomeStatisticsDateInterval {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum IncomeStatisticsDateInterval2 {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum State {
    _0 = 0,
    _1 = 1,
}

export enum SalesSummaryDatePeriod {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum SalesSummaryDatePeriod2 {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum DefaultTimezoneScope {
    _1 = 1,
    _2 = 2,
    _4 = 4,
    _7 = 7,
}

export enum IsTenantAvailableOutputState {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum EntityChangeListDtoChangeType {
    _0 = 0,
    _1 = 1,
    _2 = 2,
}

export enum FriendDtoState {
    _1 = 1,
    _2 = 2,
}

export enum ChatMessageDtoSide {
    _1 = 1,
    _2 = 2,
}

export enum ChatMessageDtoReadState {
    _1 = 1,
    _2 = 2,
}

export enum ChatMessageDtoReceiverReadState {
    _1 = 1,
    _2 = 2,
}

export enum UserNotificationState {
    _0 = 0,
    _1 = 1,
}

export enum TenantNotificationSeverity {
    _0 = 0,
    _1 = 1,
    _2 = 2,
    _3 = 3,
    _4 = 4,
}

export class AdditionalData implements IAdditionalData {
    paypal!: { [key: string]: string; } | undefined;

    constructor(data?: IAdditionalData) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property))
                    (<any>this)[property] = (<any>data)[property];
            }
        }
    }

    init(data?: any) {
        if (data) {
            if (data["Paypal"]) {
                this.paypal = {};
                for (let key in data["Paypal"]) {
                    if (data["Paypal"].hasOwnProperty(key))
                        this.paypal[key] = data["Paypal"][key];
                }
            }
        }
    }

    static fromJS(data: any): AdditionalData {
        data = typeof data === 'object' ? data : {};
        let result = new AdditionalData();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        if (this.paypal) {
            data["Paypal"] = {};
            for (let key in this.paypal) {
                if (this.paypal.hasOwnProperty(key))
                    data["Paypal"][key] = this.paypal[key];
            }
        }
        return data;
    }
}

export interface IAdditionalData {
    paypal: { [key: string]: string; } | undefined;
}

export enum CreatePaymentDtoEditionPaymentType {
    _0 = 0,
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum CreatePaymentDtoPaymentPeriodType {
    _30 = 30,
    _365 = 365,
}

export enum CreatePaymentDtoSubscriptionPaymentGatewayType {
    _1 = 1,
}

export enum CancelPaymentDtoGateway {
    _1 = 1,
}

export enum ExecutePaymentDtoGateway {
    _1 = 1,
}

export enum ExecutePaymentDtoEditionPaymentType {
    _0 = 0,
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum ExecutePaymentDtoPaymentPeriodType {
    _30 = 30,
    _365 = 365,
}

export enum DetailTransactionPaymentState {
    _100 = 100,
    _105 = 105,
    _110 = 110,
    _115 = 115,
    _120 = 120,
    _200 = 200,
    _300 = 300,
    _400 = 400,
    _500 = 500,
}

export enum DetailTransactionPaymentType {
    _100 = 100,
    _101 = 101,
    _102 = 102,
}

export enum DetailTransactionStatus {
    _0 = 0,
    _1 = 1,
    _2 = 2,
    _3 = 3,
    _4 = 4,
    _5 = 5,
}

export enum TenantLoginInfoDtoPaymentPeriodType {
    _30 = 30,
    _365 = 365,
}

export enum TransactionInfoPaymentState {
    _100 = 100,
    _105 = 105,
    _110 = 110,
    _115 = 115,
    _120 = 120,
    _200 = 200,
    _300 = 300,
    _400 = 400,
    _500 = 500,
}

export enum TransactionInfoPaymentType {
    _100 = 100,
    _101 = 101,
    _102 = 102,
}

export enum RegisterTenantInputSubscriptionStartType {
    _1 = 1,
    _2 = 2,
    _3 = 3,
}

export enum RegisterTenantInputGateway {
    _1 = 1,
}

export class SwaggerException extends Error {
    message: string;
    status: number;
    response: string;
    headers: { [key: string]: any; };
    result: any;

    constructor(message: string, status: number, response: string, headers: { [key: string]: any; }, result: any) {
        super();

        this.message = message;
        this.status = status;
        this.response = response;
        this.headers = headers;
        this.result = result;
    }

    protected isSwaggerException = true;

    static isSwaggerException(obj: any): obj is SwaggerException {
        return obj.isSwaggerException === true;
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