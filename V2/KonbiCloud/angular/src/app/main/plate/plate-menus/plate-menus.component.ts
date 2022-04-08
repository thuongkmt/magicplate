import { ProductMenuDto } from './../../../../shared/service-proxies/service-proxies';
import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import {
    SessionsServiceProxy, SessionDto, ProductServiceProxy,
    ProductDto, ProductMenusServiceProxy, PlateMenusInput,
    AssignPlateModelInput, Plate, ImportResult, ImportData, ReplicateInput,
    PlatesServiceProxy, GetPlateForView
} from '@shared/service-proxies/service-proxies';

import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import * as _ from 'lodash';
import * as moment from 'moment';
import { Papa } from 'ngx-papaparse';
import { ReplicateComponent } from './replicate/replicate.component';
import { CategoryServiceProxy, CategoryDto } from '@shared/service-proxies/service-proxies';
import * as $ from 'jquery';

@Component({
    templateUrl: './plate-menus.component.html',
    styleUrls: ['./plate-menus.component.css'],
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class PlateMenusComponent extends AppComponentBase {

    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    @ViewChild('import') myInputVariable: ElementRef;
    @ViewChild('replicateModal') replicateModal: ReplicateComponent;

    sessions: SessionDto[] = [];
    categories: CategoryDto[] = [];
    products: ProductDto[] = [];

    dateFilter;
    sessionId = null;
    paramSessionId = null;
    categoryId = '00000000-0000-0000-0000-000000000000';

    nameFilter = '';
    codeFilter = '';
    skuFilter = '';
    firstTimeLoadData = true;
    lazyLoadEvent: LazyLoadEvent;
    plateModels: GetPlateForView[] = [];

    constructor(
        injector: Injector,
        private _plateMenusServiceProxy: ProductMenusServiceProxy,
        private _sessionsServiceProxy: SessionsServiceProxy,
        private _productCategoriesServiceProxy: CategoryServiceProxy,
        private papa: Papa,
        private route: ActivatedRoute,
        private _productsServiceProxy: ProductServiceProxy,
        private _platesServiceProxy: PlatesServiceProxy,
    ) {
        super(injector);
    }

    // tslint:disable-next-line:use-life-cycle-interface
    ngOnInit() {
        this.route.params.forEach((params: Params) => {
            if (params['datefilter'] !== undefined) {
                this.dateFilter = params['datefilter'];
            } else {
                let current = new Date();
                let month = ('0' + (current.getMonth() + 1)).slice(-2);
                let date = ('0' + current.getDate()).slice(-2);
                let currentDate = current.getFullYear() + '-' + month + '-' + date;
                this.dateFilter = currentDate;
            }
            if (params['sessionfilter'] !== undefined) {
                this.paramSessionId = params['sessionfilter'];
            }
        });

        this.getSessions();
        this.getProductCategories();
        this.getPlateModels();

    }
    getPlateModels() {
        this.primengTableHelper.showLoadingIndicator();
        this._platesServiceProxy.getAll(
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            true,
            '',
            0,
            9999
        ).subscribe(result => {
            result.items.forEach(element => {
                this.plateModels.push(element);
            });
            this.primengTableHelper.hideLoadingIndicator();
        });
    }
    getProducts() {
        this.primengTableHelper.showLoadingIndicator();
        this._productsServiceProxy.getAll(
            '',
            '',
            '',
            '',
            '',
            '',
            0,
            9999
        ).subscribe(result => {
            result.items.forEach(element => {
                this.products.push(element.product);
            });

            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    getSessions() {
        this.primengTableHelper.showLoadingIndicator();
        this._sessionsServiceProxy.getAll(
            '',
            '',
            '',
            '',
            'true',
            '',
            0,
            100
        ).subscribe(result => {
            if (result.items != null) {
                let now = new Date();
                let hours = now.getHours().toString() + ':' + now.getMinutes();
                result.items.forEach(element => {
                    this.sessions.push(element.session);
                    if (this.paramSessionId != null) {
                        this.sessionId = this.paramSessionId;
                    } else if (element.session.toHrs != null) {
                        if (hours < element.session.toHrs) {
                            if (element.session.fromHrs != null) {
                                if (hours >= element.session.fromHrs) {
                                    this.sessionId = element.session.id;
                                }
                            }
                        }
                    } else if (element.session.fromHrs != null) {
                        if (hours >= element.session.fromHrs) {
                            this.sessionId = element.session.id;
                        }
                    }
                });
                if (this.sessionId == null && this.sessions.length > 0) {
                    this.sessionId = this.sessions[0].id;
                }
                this.callPlateMenusApi(this.lazyLoadEvent);
            }
        });
    }

    getProductCategories() {
        this._productCategoriesServiceProxy.getAll(
            '',
            '',
            '',
            '',
            0,
            100
        ).subscribe(result => {

            result.items.forEach(element => {
                this.categories.push(element.category);
            });
        });
    }

    getPlateMenus(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }
        if (this.firstTimeLoadData) {
            this.firstTimeLoadData = false;
            this.lazyLoadEvent = event;
            return;
        }
        this.callPlateMenusApi(event);
    }

    callPlateMenusApi(event?: LazyLoadEvent) {
        this.primengTableHelper.showLoadingIndicator();
        this._plateMenusServiceProxy.getAllProductMenus(
            this.nameFilter,
            this.codeFilter,
            this.categoryId,
            this.skuFilter,
            moment(this.dateFilter),
            this.sessionId,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            undefined,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getSkipCount(this.paginator, event),
            this.primengTableHelper.getMaxResultCount(this.paginator, event),

        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;

            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        if (this.dateFilter == null || this.dateFilter.length === 0) {
            this.primengTableHelper.records = [];
            this.notify.info(this.l('DateEmpty'));
            return;
        }
        this.paginator.changePage(this.paginator.getPage());
    }

    exportToCsv(): void {
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
            this.primengTableHelper.showLoadingIndicator();
            this._plateMenusServiceProxy.getAllProductMenus(
                this.nameFilter,
                this.codeFilter,
                this.categoryId,
                this.skuFilter,
                moment(this.dateFilter),
                this.sessionId,
                undefined,
                undefined,
                undefined,
                undefined,
                undefined,
                undefined,
                this.primengTableHelper.getSorting(this.dataTable),
                0,
                10000
            ).subscribe(result => {
                if (result.items != null) {
                    let csvData = new Array();
                    const header = {
                        Date: 'Date',
                        Session: 'Session',
                        ProductCategory: 'Product Category',
                        ProductName: 'Product Name',
                        Barcode: 'BarCode',
                        SKU: 'SKU',
                        PlateCode: 'Plate Code',
                        Price: 'Price',
                        ContractorPrice: 'Contractor Price'
                    };
                    csvData.push(header);

                    let sessions = this.sessions.filter(x => x.id === this.sessionId);
                    if (sessions != null && sessions.length > 0) {
                        for (let record of result.items) {
                            csvData.push({
                                Date: moment(this.dateFilter).format('DD/MM/YYYY'),
                                Session: sessions[0].name,
                                ProductCategory: record.categoryName,
                                ProductName: record.product.name,
                                Barcode: record.product.barcode,
                                SKU: record.product.sku ? record.product.sku : '',
                                PlateCode: (record.plate ? '_' + record.plate.code : ''),
                                Price: record.price,
                                ContractorPrice: record.priceStrategy
                            });
                        }
                        // tslint:disable-next-line:no-unused-expression
                        new Angular5Csv(csvData, 'Menu Scheduling');
                    } else {
                        this.notify.info(this.l('DateEmpty'));
                    }
                } else {
                    this.notify.info(this.l('DateEmpty'));
                }

                this.primengTableHelper.hideLoadingIndicator();
            });
        } else {
            this.notify.info(this.l('DateEmpty'));
        }
    }
    importCSV(files) {
        if (files.length === 0) {
            return;
        }
        let file: File = files[0];
        //validate file type
        let valToLower = file.name.toLowerCase();
        let regex = new RegExp('(.*?)\.(csv)$');
        if (!regex.test(valToLower)) {
            abp.message.error('Please select csv file');
            this.myInputVariable.nativeElement.value = '';
            return;
        }

        let myReader = new FileReader();
        myReader.onloadend = (e) => {
            let fileContent = myReader.result.toString();
            if (fileContent != null && fileContent !== undefined && fileContent.trim().length === 0) {
                abp.message.info('File empty');
                this.myInputVariable.nativeElement.value = '';
                return;
            }
            this.papa.parse(fileContent, {
                header: true,
                delimiter: ',',
                worker: false,
                skipEmptyLines: true,
                complete: (result) => {
                    if (result.errors.length > 0) {
                        let errorList = result.errors.map(e => e.row).join(',');
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = 0;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }

                    let importData = new Array<ImportData>();
                    for (let record of result.data) {
                        let item = new ImportData();
                        item.selectedDate = record['Date'];
                        item.sessionName = record['Session'];
                        item.plateCode = record['Plate Code'].replace(/_/g, '');
                        item.price = record['Price'];
                        item.contractorPrice = record['Contractor Price'];
                        item.productName = record['Product Name'];
                        item.barCode = record['BarCode'];
                        item.sku = record['SKU'];
                        importData.push(item);
                    }
                    let resultsErrorSku = [];
                    let resultsErrorName = [];
                    let errorList = '';
                    for (let i = 0; i < importData.length - 1; i++) {



                        if (importData[i].sku) {
                            var duplicates = _.filter(importData, function (el) { return el.sku == importData[i].sku });
                            if (duplicates && duplicates.length > 1) {
                                resultsErrorSku.push(importData[i]);
                            }
                        }
                        if (importData[i].productName) {
                            var duplicates = _.filter(importData, function (el) { return el.productName == importData[i].productName });
                            if (duplicates && duplicates.length > 1) {
                                resultsErrorName.push(importData[i]);
                            }
                        }

                    }
                    // Check Duplicate Sku.
                    if (resultsErrorSku.length > 0) {
                        resultsErrorSku.forEach(element => {
                            let index = importData.indexOf(element) + 2;
                            errorList += ('<div>Row ' + index + ': Duplicate SKU ' + element.sku + ' in file import.</div>');
                        });
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = resultsErrorSku.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }
                    // Check Duplicate Product Name.
                    if (resultsErrorName.length > 0) {
                        resultsErrorName.forEach(element => {
                            let index = importData.indexOf(element) + 2;
                            errorList += ('<div>Row ' + index + ': Duplicate Product Name ' + element.name + ' in file import.</div>');
                        });
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = resultsErrorName.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }
                    //submit data
                    this._plateMenusServiceProxy.importPlateMenu(importData)
                        .subscribe((result) => {
                            this.showImportMessage(result, 'Import Result');
                            this.myInputVariable.nativeElement.value = '';
                            this.reloadPage();
                        });
                }
            });

        };
        myReader.readAsText(file);
    }
    updatePrice(record: ProductMenuDto) {
        if (record === null || record === undefined) {
            return;
        }
        if (record.price < 0) {
            this.notify.warn(this.l('PriceZero'));
            return;
        }
        let input = new PlateMenusInput();
        input.id = record.id;
        input.price = record.price;

        if (record && record.price === null) {
            this.notify.warn('Price is not empty.');
            return;
        }

        this._plateMenusServiceProxy.updatePrice(input).subscribe(result => {
            if (result === true) {
                this.notify.success(this.l('SavedSuccessfully'));
            } else {
                this.notify.error(this.l('SaveFailed'));
            }
        });
    }

    updateDisplayOrder(record: ProductMenuDto) {
        if (record === null || record === undefined) {
            return;
        }

        let input = new PlateMenusInput();
        input.id = record.id;
        input.displayOrder = record.displayOrder;

        this._plateMenusServiceProxy.updateDisplayOrder(input).subscribe(result => {
            if (result === true) {
                this.notify.success(this.l('SavedSuccessfully'));
            } else {
                this.notify.error(this.l('SaveFailed'));
            }
        });
    }

    updatePriceStrategy(record: ProductMenuDto) {
        if (record === null || record === undefined) {
            return;
        }
        if (record.priceStrategy < 0) {
            this.notify.warn(this.l('PriceZero'));
            return;
        }
        let input = new PlateMenusInput();
        input.id = record.id;
        input.priceStrategy = record.priceStrategy;

        if (record && record.priceStrategy === null) {
            this.notify.warn('PriceStrategy is not empty.');
            return;
        }

        this._plateMenusServiceProxy.updatePriceStrategy(input).subscribe(result => {
            if (result === true) {
                this.notify.success(this.l('SavedSuccessfully'));
            } else {
                this.notify.error(this.l('SaveFailed'));
            }
        });
    }
    updatePlate(event: any, record: ProductMenuDto) {
        if (record === null || record === undefined) {
            return;
        }

        //record.plateId = event.target.value;
        var input = new AssignPlateModelInput();
        input.id = record.id;
        input.plateId = event.target.value;

        this._plateMenusServiceProxy.updatePlate(input).subscribe(result => {
            if (result) {
                if (record.plate) {
                    record.plate.id = input.plateId;
                } else {
                    record.plate = new Plate();
                    record.plate.id = input.id;
                }
                this.notify.success(this.l('SavedSuccessfully'));
            } else {
                $('#' + record.id).val('00000000-0000-0000-0000-000000000000');
                this.notify.error(this.l("Error"));
            }
        });
    }
    showImportMessage(result: ImportResult, title: string) {
        let content = '<div class=\'text-left\' style=\'margin-left: 20px;\'><div>- Success: ' + result.successCount + '</div>';
        content += '<div>- Failed: ' + result.errorCount + '</div>';
        content += '<div>- Failed Rows:</div>';
        content += '<div style=\'margin-left: 10px;\'>' + result.errorList + '</div></div>';

        abp.message.info(content, title, true);
    }

    replicateData() {
        this.replicateModal.show(this.dateFilter);
    }

    generateData() {
        if (this.dateFilter == null || this.dateFilter.length === 0) {
            this.primengTableHelper.records = [];
            this.notify.info(this.l('DateEmpty'));
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        let input = new ReplicateInput();
        input.dateFilter = moment(this.dateFilter);
        this._plateMenusServiceProxy.generateProductMenu(
            input
        ).finally(() => {

            this.primengTableHelper.hideLoadingIndicator();
        })
            .subscribe(result => {
                if (result) {
                    this.reloadPage();
                    abp.message.success('Generate Plate Menu successfull!');
                }
            });
    }
}
