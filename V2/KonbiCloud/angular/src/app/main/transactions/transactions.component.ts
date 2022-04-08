import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
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
import { MachineServiceProxy, MachineDto, PagedResultDtoOfMachineDto } from "shared/service-proxies/machine-service-proxies";
import { FileDownloadService } from '@shared/utils/file-download.service';

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
  machineFilter = '';
  sessionFilter = '';
  stateFilter = '';
  currencySymbol = '$';
  sessions: GetSessionForView[] = [];
  transType = 1;
  machines: MachineDto[] = [];
  taxSettings: TaxSettingsEditDto;


  constructor(
    injector: Injector,
    private route: ActivatedRoute,
    private router: Router,
    private _transactionService: TransactionServiceProxy,
    private _sessionService: SessionsServiceProxy,
    private machinesService: MachineServiceProxy,
    private _tenantSettingsServiceProxy: TenantSettingsServiceProxy,
    private _fileDownloadService: FileDownloadService
  ) {
    super(injector);

    if (this.router.url.includes("success")) this.transType = 1
    else this.transType = 0
  }

  ngOnInit() {
    this.primengTableHelper.showLoadingIndicator();
    this.getSessions();
    this.getMachines();
    this.currencySymbol = abp.setting.get("CurrencySymbol");
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
    this._sessionService.getAll(undefined, undefined, undefined, undefined, undefined, 'session.name desc', 0, 100)
      .subscribe(result => {
        this.sessions = result.items;
      });
  }

  getMachines(event?: LazyLoadEvent) {
    this.machinesService.getAll(0, 100, 'name desc')
      .subscribe(result => {
        this.machines = result.items;
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
      this.machineFilter,


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
  exportToExcel(): void {
    this._transactionService.getAllTransactionsForExcel(
      this.sessionFilter,
      this.stateFilter,
      this.transType,
      this.fromDate,
      this.toDate,
      this.machineFilter,
      this.primengTableHelper.getSorting(this.transactionTable),
    )
      .subscribe(result => {
        this._fileDownloadService.downloadTempFile(result);
      });
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
        this.machineFilter,
        this.primengTableHelper.getSorting(this.transactionTable),
        0,
        999999

      ).subscribe(result => {
        if (result.items != null) {
          // Declare csv data.
          let csvData = new Array();
          // Declare const header.
          const header = {
            TransactionId: 'Transaction ID',
            Machine: 'Machine',
            Session: 'Session',
            Buyer: 'Buyer',
            PaymentTime: 'Payment Time',
            CardLabel: 'Payment Type',

            Amount: 'Amount',
            Plates: 'Items',
            Product: 'Product',
            Category: "Category",
            Price: 'Price',
            Discount: 'Discount',
            Tax: 'Tax',
            States: 'States'
          };
          // Add header to csv data.
          csvData.push(header);
          // add content file csv.
          for (let record of result.items) {
            var afterDiscounted = record.amount;
            if (record.discountPercentage > 0)
              afterDiscounted -= afterDiscounted * record.discountPercentage / 100;
            var discountedAmount = record.amount - afterDiscounted;
            var afterTax = afterDiscounted;
            if (record.taxPercentage > 0)
              afterTax += afterTax * record.taxPercentage / 100;
            csvData.push({
              TransactionId: (record.transactionId == null) ? '' : record.transactionId,
              Machine: (record.machine == null) ? '' : record.machine,
              Session: (record.session == null) ? '' : record.session,
              Buyer: (record.buyer == null) ? '' : record.buyer,
              PaymentTime: (record.paymentTime == null) ? '' : moment(record.paymentTime).format('DD/MM/YYYY HH:mm'),
              CardLabel: (record.cardLabel == null) ? '' : record.cardLabel,

              Amount: (record.amount == null) ? '' : '$' + afterTax.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'),
              Plates: (record.platesQuantity == null) ? '' : record.platesQuantity,
              Product: "",
              Category: "",
              Price: "",
              Discount: "",
              Tax: (afterTax - afterDiscounted).toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'),
              States: (record.states == null) ? '' : record.states
            });
            for (let p of record.products) {
              csvData.push({
                TransactionId: '',
                Machine: '',
                Session: '',
                Buyer: '',
                PaymentTime: '',
                CardLabel: '',
                CardNumber: '',
                Plates: '',
                Product: (p.product == null) ? 'Custom price' : p.product.name,
                Category: (p.product == null) ? '' : (p.product.category == null) ? '' : p.product.category.name,
                Price: (p.amount == null) ? '' : '$' + p.amount.toFixed(2).replace(/\d(?=(\d{3})+\.)/g, '$&,'),
                Discount: (record.discountPercentage > 0) ? '' : '$' + (p.amount * 100 / (100 + record.taxPercentage) * record.discountPercentage / 100).toFixed(2),
                Tax: "",
                States: (record.states == null) ? '' : record.states
              });
            }
          }
          // Export csv data.
          // tslint:disable-next-line:no-unused-expression
          new Angular5Csv(csvData, 'Transactions-' + moment().format('DD-MM-YYYY'));
          // tslint:enable-next-line:no-unused-expression
        } else {
          this.notify.info(this.l('Data is Empty'));
        }
        this.primengTableHelper.hideLoadingIndicator();
      });
    } else {
      this.notify.info(this.l('Data is Empty'));
    }
  }
}
