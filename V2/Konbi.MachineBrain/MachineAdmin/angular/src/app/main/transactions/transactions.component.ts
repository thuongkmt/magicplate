import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Params, Router, NavigationEnd } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import * as _ from 'lodash';
import { TransactionServiceProxy, TransactionDto, TenantSettingsServiceProxy, TenantSettingsEditDto, TaxSettingsEditDto } from '@shared/service-proxies/service-proxies';
import { Http } from '@angular/http';
import { NotifyService } from '@abp/notify/notify.service';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as moment from 'moment';
import { TransactionDetailComponent } from './transaction-detail/transaction-detail.component';
import { SessionsServiceProxy, GetSessionForView } from '@shared/service-proxies/service-proxies';

@Component({
  templateUrl: './transactions.component.html',
  styleUrls: ['./transactions.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class TransactionsComponent extends AppComponentBase {

  @ViewChild('transactionTable') transactionTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('transactionDetailModal') transactionDetailComponent: TransactionDetailComponent;

  fromDate = '';
  toDate = '';
  sessionFilter = '';
  stateFilter = '';
  currencySymbol = '$';
  sessions: GetSessionForView[] = [];
  transType = 1;
  taxSettings: TaxSettingsEditDto;

  constructor(
    injector: Injector,
    private router: Router,
    private _transactionService: TransactionServiceProxy,
    private _sessionService: SessionsServiceProxy,
    private _tenantSettingsServiceProxy: TenantSettingsServiceProxy
  ) {
    super(injector);

    if (this.router.url.includes("success")) this.transType = 1
    else this.transType = 0
  }

  // tslint:disable-next-line:use-life-cycle-interface
  ngOnInit() {
    this.primengTableHelper.showLoadingIndicator();
    this.getSessions();
    this.currencySymbol = abp.setting.get('CurrencySymbol');
    this.getTaxInfo();
  }

  reloadPage(): void {
    this.paginator.changePage(0);
  }
  getTaxInfo(): void {
    this._tenantSettingsServiceProxy.getAllSettings()
      .subscribe(result => {
        this.taxSettings = result.magicplate.taxSettings;
      });
  }
  getSessions() {
    this._sessionService.getAll(undefined, undefined, undefined, undefined, undefined, undefined, 0, 100)
      .subscribe(result => {
        this.sessions = result.items;
      });
  }

  getTransactions(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.primengTableHelper.showLoadingIndicator();
    this._transactionService.getAllTransactions(
      this.sessionFilter,
      this.stateFilter,
      this.transType,
      this.fromDate,
      this.toDate,
      this.primengTableHelper.getSorting(this.transactionTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event),
    )
      .finally(() => { })
      .subscribe(result => {
        this.primengTableHelper.totalRecordsCount = result.totalCount;
        this.primengTableHelper.records = result.items;
        this.primengTableHelper.hideLoadingIndicator();
      });
  }
  getDetail(record: TransactionDto): void {
    this.transactionDetailComponent.show(record, this.taxSettings);
  }

  // Export list transactions to csv.
  exportToCsv(): void {
    // Check exists data export.
    if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
      // Show icon loading.
      this.primengTableHelper.showLoadingIndicator();
      // Load transactions.
      this._transactionService.getAllTransactions(
        this.sessionFilter,
        this.stateFilter,
        this.transType,
        this.fromDate,
        this.toDate,
        this.primengTableHelper.getSorting(this.transactionTable),
        0,
        999999,
      ).subscribe(result => {
        if (result.items != null) {
          // Declare csv data.
          let csvData = new Array();
          // Declare const header.
          const header = {
            TransactionId: 'Transaction ID',
            Buyer: 'Buyer',
            PaymentTime: 'Payment Time',
            CardLabel: 'Card Label',
            CardNumber: 'Card Number',
            ApproveCode: 'Approve Code',
            Amount: 'Amount',
            Items: 'Items',
            Session: 'Session',
            States: 'States'
          };
          // Add header to csv data.
          csvData.push(header);
          // add content file csv.
          for (let record of result.items) {
            csvData.push({
              TransactionId: (record.transactionId == null) ? '' : record.transactionId,
              Buyer: (record.buyer == null) ? '' : record.buyer,
              PaymentTime: (record.paymentTime == null) ? '' : moment(record.paymentTime).format('DD/MM/YYYY HH:mm:ss'),
              CardLabel: (record.cardLabel == null) ? '' : record.cardLabel,
              CardNumber: (record.cardNumber == null) ? '' : record.cardNumber,
              ApproveCode: (record.approveCode == null) ? '' : '_' + record.approveCode,
              Amount: (record.amount == null) ? '' : '$' + record.amount.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'),
              Items: (record.products == null) ? '0' : record.products.length,
              Session: (record.session == null) ? '' : record.session,
              States: (record.states == null) ? '' : record.states
            });
          }
          // Export csv data.
          // tslint:disable-next-line:no-unused-expression
          new Angular5Csv(csvData, 'Transactions');
          // tslint:enable-next-line:no-unused-expression
        } else {
          this.notify.info(this.l('Data is Empty'));
        }
        // Hide icon loading.
        this.primengTableHelper.hideLoadingIndicator();
      });
    } else {
      this.notify.info(this.l('Data is Empty'));
    }
  }
}
