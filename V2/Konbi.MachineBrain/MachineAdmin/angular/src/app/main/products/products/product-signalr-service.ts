import { Injectable, Injector, NgZone } from '@angular/core';
import { AppComponentBase } from '@shared/common/app-component-base';
import { HubConnection } from '@aspnet/signalr';

@Injectable()
export class ProductSignalrService extends AppComponentBase {

    constructor(
        injector: Injector,
        public _zone: NgZone
    ) {
        super(injector);
    }

    prodHubConnection: HubConnection;
    isProdHubConnected = false;

    configureConnection(connection): void {
        this.prodHubConnection = connection;

        connection.onclose(e => {
            this.isProdHubConnected = false;
            if (e) {
                abp.log.debug('Chat connection closed with error: ' + e);
            } else {
                abp.log.debug('Chat disconnected');
            }

            if (!abp.signalr.autoConnect) {
                return;
            }

            setTimeout(() => {
                connection.start().then(result => {
                    this.isProdHubConnected = true;
                });
            }, 5000);
        });

        this.registerChatEvents(connection);
    }

    registerChatEvents(connection): void {
        connection.on('notifyBarcode', message => {
            abp.event.trigger('app.product.barcode.messageReceived', message);
        });
    }

    changeProductUpdateStatus = (isUpdating: boolean, callback?) => {
        this.prodHubConnection.invoke('changeProductUpdateStatus', isUpdating)
            .then(() => {
                if(callback) callback();
            })
            .catch(err => { console.log('change product update status failed: ', err); })
    }

    init(): void {
        this._zone.runOutsideAngular(() => {
            abp.signalr.connect();
            abp.signalr.startConnection(abp.appPath + '/signalr-rfidtable', connection => {
                this.isProdHubConnected = true;
                this.configureConnection(connection);
            }).then(() => {
                this.prodHubConnection.invoke('JoinGroup', 'CustomerUI').then(() => {});
            });
        });
    }
}
