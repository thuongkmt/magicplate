import { mergeMap as _observableMergeMap, catchError as _observableCatch } from 'rxjs/operators';
import { Observable, throwError as _observableThrow, of as _observableOf } from 'rxjs';
import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import { HttpClient, HttpHeaders, HttpResponse, HttpResponseBase } from '@angular/common/http';
import { API_BASE_URL } from './service-proxies';

@Injectable()
export class ProductImageServiceProxy {
    private http: HttpClient;
    private baseUrl: string;
    protected jsonParseReviver: ((key: string, value: any) => any) | undefined = undefined;

    constructor(@Inject(HttpClient) http: HttpClient, @Optional() @Inject(API_BASE_URL) baseUrl?: string) {
        this.http = http;
        this.baseUrl = baseUrl ? baseUrl : "";
    }
    importCsvUploadImages(files: Array<File>): Observable<any> {
        var url_ = this.baseUrl + '/ProductImageUpload/ImportProductUploadFiles'

        const formData: any = new FormData()

        for (let i = 0; i < files.length; i++) {
            formData.append("files", files[i], files[i]['name']);
        }

        return this.http
            .post(url_, formData)
            .map(res => { return res })
            .catch(error => {
                let errMsg = (error.message) ? error.message :
                    error.status ? `${error.status} - ${error.statusText}` : 'Server error';
                console.error(errMsg);
                return Observable.throw(errMsg);
            });
    }

    uploadFile(files: Array<File>): Observable<any> {
        var url_ = this.baseUrl + '/ProductImageUpload/UploadFiles'

        const formData: any = new FormData()

        for (let i = 0; i < files.length; i++) {
            formData.append("files", files[i], files[i]['name']);
        }
        console.log('form data variable :   ' + formData.toString());

        return this.http
            .post(url_, formData)
            .map(res => { return res })
            .catch(error => {
                let errMsg = (error.message) ? error.message :
                    error.status ? `${error.status} - ${error.statusText}` : 'Server error';
                console.error(errMsg);
                return Observable.throw(errMsg);
            });
    }
}