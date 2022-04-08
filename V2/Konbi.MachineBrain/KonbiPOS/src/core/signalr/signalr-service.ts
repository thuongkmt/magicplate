import { Injectable, Inject, Optional, InjectionToken } from '@angular/core';
import {
    HubConnection,
    HubConnectionBuilder
} from '@aspnet/signalr';

import { POS_API_BASE_URL } from '../api-services/api-service-proxies';

@Injectable()
export class SignalRService {
    private baseUrl: string;
    private hubConnection: HubConnection;

    constructor(@Optional() @Inject(POS_API_BASE_URL) baseUrl?: string) {
        this.baseUrl = baseUrl ? baseUrl : '';
    }

    public initialize = () => {
        try {
            const url = this.baseUrl + '/signalr-rfidtable';
            this.hubConnection = new HubConnectionBuilder()
                .withUrl(url)
                .build();
        } catch (err) {

        }
    }

    public startConnection = (callback?) => {
        this.hubConnection
            .start()
            .then(() => {
                this.hubConnection.invoke('JoinGroup', 'CustomerUI')
                    .then(() => {
                    })
                    .catch((err) => {
                        console.log(err);
                    });
            })
            .catch(() => {
                if (this.hubConnection.state === 0) {
                    setTimeout(() => {
                        this.startConnection(() => callback(this.hubConnection.state));
                    }, 300);
                }
            }).finally(() => {
                if (callback) {
                    callback(this.hubConnection.state);
                }
            });
    }

    reconnect = (callback) => {
        this.hubConnection.onclose(() => {
            this.startConnection(() => callback(this.hubConnection.state));
        });
    }

    public listenTransactionChanges = (callback) => {
        this.hubConnection.on('updateTransactionInfo', (trans) => {
            if (callback) {
                callback(trans);
            }
        });
    }

    public listenPaymentModes = (callback) => {
        this.hubConnection.on('updatePaymentMethods', (paymentModes) => {
            if (callback) {
                callback(paymentModes);
            }
        });
    }

    public listenCountDownChanges = (callback) => {
        this.hubConnection.on('showCustomerCountDown', (countDownInfo) => {
            if (callback) {
                callback(countDownInfo);
            }
        });
    }

    public listenProductChanges = (callback) => {
        this.hubConnection.on('notifyProductChanges', (type) => {
            if (callback) {
                callback(type);
            }
        });
    }

    public listenCashPaymentConfirm = (callback) => {
        this.hubConnection.on('notifyCashPayment', () => {
            if (callback) {
                callback();
            }
        });
    }
    public listenSessionInfoChanged = (callback) => {
        this.hubConnection.on('updateSessionInfo', (result) => {
            if (callback) {
                callback(result);
            }
        });
    }

    public addOrderItems = (orderItem: ManualPaymentInput, callback) => {
        this.hubConnection.invoke('executeManualTransaction', orderItem)
            .then(() => { })
            .catch((err) => console.log(err))
            .finally(() => callback());
    }

    public executeCashlessPayment = (mode, callback) => {
        this.hubConnection.invoke('onClickPay', mode)
            .then(() => { })
            .catch(err => {
                console.log(err);
            })
            .finally(() => callback());
    }

    public executeCashPayment = (callback) => {
        this.hubConnection.invoke('payCash')
            .then(() => { })
            .catch(err => {
                console.log(err);
            })
            .finally(() => callback());
    }

    public executeBarcodeScanningTransaction = (barcodeValue, callback?) => {
        this.hubConnection.invoke('executeBarcodeScanningTransaction', barcodeValue)
            .then(() => { })
            .catch(err => {
                console.log(err);
            })
            .finally(() => { if (callback) callback(); });
    }

    public cancelSales = (callback) => {
        this.hubConnection.invoke('cancelTransaction')
            .then(() => { })
            .catch(err => {
                console.log('Err: ', err);
            }).finally(() => callback());
    }

    public removeOrderItem = (uId: string, callback) => {
        this.hubConnection.invoke('removeOrderItem', uId)
            .then(() => { callback(); })
            .catch(err => { console.log(err) });
    }

    public resetTransaction = () => {
        this.hubConnection.invoke('resetTransaction')
            .then(() => { })
            .catch(err => { console.log(err) });
    }
}


export class ManualPaymentInput implements IManualPaymentInput {
    plateCode: string | undefined;
    productId: string | undefined;
    productName: string | undefined;
    price: number | undefined;

    constructor(data?: IManualPaymentInput) {
        if (data) {
            for (var property in data) {
                if (data.hasOwnProperty(property)) {
                    (<any>this)[property] = (<any>data)[property];
                }
            }
        }
    }

    init(data?: any) {
        if (data) {
            this.plateCode = data["plateCode"];
            this.productId = data["productId"];
            this.productName = data["productName"];
            this.price = data["price"];
        }
    }

    static fromJS(data: any): ManualPaymentInput {
        data = typeof data === 'object' ? data : {};
        let result = new ManualPaymentInput();
        result.init(data);
        return result;
    }

    toJSON(data?: any) {
        data = typeof data === 'object' ? data : {};
        data["plateCode"] = this.plateCode;
        data["productId"] = this.productId;
        data["productName"] = this.productName;
        data["price"] = this.price;
        return data;
    }
}

export interface IManualPaymentInput {
    plateCode: string | undefined;
    productId: string | undefined;
    productName: string | undefined;
    price: number | undefined;
}