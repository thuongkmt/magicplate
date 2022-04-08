import { Component, Injector, OnInit } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Observable } from 'rxjs/Rx';
import { StatusConnectServerServiceServiceProxy } from '@shared/service-proxies/service-proxies';

@Component({
    templateUrl: './footer.component.html',
    selector: 'footer-bar'
})
export class FooterComponent extends AppComponentBase implements OnInit {

    releaseDate: string;
    connected = false;

    constructor(
        injector: Injector,
        private _statusServiceProxy: StatusConnectServerServiceServiceProxy
    ) {
        super(injector);
        Observable.timer(0, 120000)
            .subscribe(() => {
                this._statusServiceProxy.getStatusConnectServer()
                .subscribe(result => {
                    if (result) {
                        this.connected = true;
                    } else {
                        this.connected = false;
                    }
                });
            });
    }

    ngOnInit(): void {
        this.releaseDate = this.appSession.application.releaseDate.format('YYYYMMDD');
    }
}
