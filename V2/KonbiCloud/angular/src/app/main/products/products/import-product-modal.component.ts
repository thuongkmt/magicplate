import { Component, ViewChild, Injector, Output, EventEmitter, ElementRef } from '@angular/core';
import * as _ from 'underscore';
import { ModalDirective } from 'ngx-bootstrap';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Papa } from 'ngx-papaparse';
import { CategoryServiceProxy, ProductServiceProxy, CategoryDto, ProductDto, ImportResult, CreateOrEditProductDto } from '@shared/service-proxies/service-proxies';
import { ProductImageServiceProxy } from '@shared/service-proxies/productImage-proxies';
@Component({
    selector: 'importProductModal',
    templateUrl: './import-product-modal.component.html',
    styleUrls: ['./create-or-edit-product-modal.component.css']
})

export class ImportProductModalComponent extends AppComponentBase {

    @ViewChild('importProductModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('import') myInputVariable: ElementRef;

    importStep = 1;
    active = false;
    saving = false;

    //upload photo
    files: File[] = [];
    lastInvalids: any;
    lastFileAt: Date;

    /**
     * Get current date.
     */
    getDate() {
        return new Date();
    }

    /**
     * constructor.
     * @param injector injector.
     * @param _platesServiceProxy plates sevice proxy.
     * @param papa papa.
     */
    constructor(
        injector: Injector,
        private _productsServiceProxy: ProductServiceProxy,
        private _productImageServiceProxy: ProductImageServiceProxy,
        private papa: Papa
    ) {
        super(injector);
    }

    show(): void {
        this.active = true;
        this.modal.show();
    }

    /**
     * Close modal.
     */
    close(): void {
        this.importStep = 1;
        this.files = [];
        if (this.myInputVariable) {
            this.myInputVariable.nativeElement.value = '';
        }
        this.active = false;
        this.saving = false;
        this.modal.hide();
    }

    /**
     * Click go to step 2 then upload file csv.
     */
    btnNextToStep2Click() {
        //upload images first
        if (this.files.length > 0) {
            this.saving = true;
            this._productImageServiceProxy.importCsvUploadImages(this.files)
                .subscribe(
                    response => {
                        this.saving = false;
                        if (response.result) {
                            this.importStep = 2;
                        } else {
                            abp.message.error('Upload error, please check image files and try again');
                        }
                    }, err => {
                        abp.message.error('Upload error, please check image files and try again');
                    }
                );
        } else {
            this.importStep = 2;
        }
    }

    /**
     * Import file.
     * @param files List file from upload file.
     */
    importCSV(files: File[]) {
        if (files.length === 0) {
            return;
        }
        let file: File = files[0];
        //validate file type
        let valToLower = file.name.toLowerCase();
        // regexp extention of file.
        let regex = new RegExp('(.*?)\.(csv)$');
        if (!regex.test(valToLower)) {
            abp.message.error('Please select csv file');
            this.myInputVariable.nativeElement.value = '';
            return;
        }
        // Reading file csv.
        let myReader = new FileReader();
        myReader.onloadend = (e) => {
            let fileContent = myReader.result.toString();
            this.papa.parse(fileContent, {
                header: true,
                delimiter: ',',
                worker: false,
                skipEmptyLines: true,
                complete: (result) => {
                    let errorList = '';
                    if (result.errors.length > 0) {

                        result.errors.forEach(element => {
                            errorList += ('<div>' + element + '</div>');
                        });
                        //var errorList = result.errors.map(e => e.row).join(",");
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = result.errors.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }

                    //check empty product name
                    let csvImportData = new Array<CreateOrEditProductDto>();
                    let csvData = new Array<CreateOrEditProductDto>();
                    let errorCount = 0;
                    for (let i = 0; i < result.data.length; i++) {
                        let record = result.data[i];
                        let item = new CreateOrEditProductDto();
                        item.categoryName = record['Product Category Name'];
                        item.sku = record['SKU'].trim();
                        item.name = record['Product Name'].trim();
                        item.barcode = record['Barcode'].trim().replace('_', '');
                        // item.sku = record['SKU'];
                        item.imageUrl = record['Product Image'];
                        item.desc = record['Product Description'];
                        item.price = record['Product Price'];
                        item.displayOrder = record['Display Order'];

                        if (item.name === '') {
                            errorList += ('<div>Row ' + (i + 2) + ' product name is empty</div>');
                            errorCount++;
                        }
                        csvImportData.push(item);
                        csvData.push(item);
                    }

                    if (errorList !== '') {
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = errorCount;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }

                    //check duplicate Barcode, Plate Code, Product Name.
                    let resultsError = [];
                    let resultsErrorSku = [];
                    let resultsErrorPlate = [];
                    let resultsErrorName = [];
                    let resultsErrorDisplayOrder = [];

                    for (let i = 0; i < csvData.length - 1; i++) {

                        if (csvData[i].barcode) {
                            var duplicates = _.filter(csvData, function (el) { return el.barcode == csvData[i].barcode });
                            if (duplicates && duplicates.length > 1) {
                                resultsError.push(csvData[i]);
                            }
                        }

                        if (csvData[i].sku) {
                            var duplicates = _.filter(csvData, function (el) { return el.sku == csvData[i].sku });
                            if (duplicates && duplicates.length > 1) {
                                resultsErrorSku.push(csvData[i]);
                            }
                        }
                        if (csvData[i].name) {
                            var duplicates = _.filter(csvData, function (el) { return el.name == csvData[i].name });
                            if (duplicates && duplicates.length > 1) {
                                resultsErrorName.push(csvData[i]);
                            }
                        }


                        if (isNaN(csvData[i + 1].displayOrder) && csvData[i + 1].displayOrder) {
                            resultsErrorDisplayOrder.push(csvData[i]);
                        }
                    }
                    // Check Duplicate Sku.
                    if (resultsErrorSku.length > 0) {
                        resultsErrorSku.forEach(element => {
                            let index = csvData.indexOf(element) + 2;
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

                    // Check Duplicate Barcode.
                    if (resultsError.length > 0) {
                        resultsError.forEach(element => {
                            let index = csvData.indexOf(element) + 2;
                            errorList += ('<div>Row ' + index + ': Duplicate Barcode ' + element.barcode + ' in file import.</div>');
                        });
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = resultsError.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }




                    // Check Duplicate Product Name.
                    if (resultsErrorName.length > 0) {
                        resultsErrorName.forEach(element => {
                            let index = csvData.indexOf(element) + 2;
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

                    // Check is Number with displayOrder.
                    if (resultsErrorDisplayOrder.length > 0) {
                        resultsErrorDisplayOrder.forEach(element => {
                            let index = csvData.indexOf(element) + 2;
                            errorList += ('<div>Row ' + index + ': Display Order ' + element.displayOrder + ' is not a Number in file import.</div>');
                        });
                        let mess = new ImportResult();
                        mess.successCount = 0;
                        mess.errorCount = resultsErrorDisplayOrder.length;
                        mess.errorList = errorList;
                        this.showImportMessage(mess, 'Invalid Data');
                        this.myInputVariable.nativeElement.value = '';
                        return;
                    }

                    // validate ok, submit data
                    abp.ui.setBusy();
                    this.close();
                    this._productsServiceProxy.importProduct(csvImportData)
                        .subscribe((result) => {
                            this.showImportMessage(result, 'Import Result');
                            //this.close();
                            this.modalSave.emit(null);
                            abp.ui.clearBusy();
                        });
                }
            });
        };
        myReader.readAsText(file);
    }

    /**
     * Show message after import.
     * @param result result import.
     * @param title Title message.
     */
    showImportMessage(result: ImportResult, title: string) {
        console.log(result);
        let content = '<div class="text-left" style="margin-left: 20px;"><div>- Success: ' + result.successCount + '</div>';
        if (result.errorCount > 0) {
            content += '<div>- Failed: ' + result.errorCount + '</div>';
            content += '<div>- Failed Rows: ' + result.errorList + '</div></div>';
        }


        abp.message.info(content, title, true);
    }
}
