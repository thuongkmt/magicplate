import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { TransactionServiceProxy, TransactionDto, TaxSettingsEditDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';

@Component({
    selector: 'transactionDetailModal',
    templateUrl: './transaction-detail.component.html'
})
export class TransactionDetailComponent extends AppComponentBase {

    @ViewChild('transactionDetailModal') modal: ModalDirective;
    @ViewChild('imageModal') imageModal: ModalDirective;

    transaction: TransactionDto;
    active = false;
    lazyLoadEvent: LazyLoadEvent;
    currencySymbol = '$';
    imgUrl = '';
    afterDiscounted = 0;
    discountedAmount = 0;
    afterTax = 0;
    taxAmount: any;
    taxSettings: TaxSettingsEditDto;
    defaultImage: string = 'assets/common/images/ic_nophoto.jpg'

    constructor(
        injector: Injector,
        private _transactionServiceProxy: TransactionServiceProxy
    ) {
        super(injector);
    }

    ngOnInit() {
        this.currencySymbol = abp.setting.get("CurrencySymbol");
    }

    show(record: TransactionDto, taxSettings: TaxSettingsEditDto): void {
        this.transaction = record;
        this.taxSettings = taxSettings;
        this.afterDiscounted = record.amount;
        if (record.discountPercentage > 0)
            this.afterDiscounted -= this.afterDiscounted * record.discountPercentage / 100;
        this.discountedAmount = record.amount - this.afterDiscounted;
        this.afterTax = this.afterDiscounted;
        if (record.taxPercentage > 0)
            this.afterTax += this.afterTax * record.taxPercentage / 100;
        this.taxAmount = this.afterTax - this.afterDiscounted;
        this.primengTableHelper.records = record.products;
        this.active = true;
        this.modal.show();
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }

    showFullImage(url: string) {
        if (url !== this.defaultImage) {
            this.imgUrl = url;
            this.imageModal.show();
        }
    }
    closeImageModal() {
        this.imageModal.hide();
        this.imgUrl = '';
    }
}
