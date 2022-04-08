
import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CategoryServiceProxy, ProductServiceProxy, CategoryDto, ProductDto, ImportResult, CreateOrEditProductDto } from '@shared/service-proxies/service-proxies';
import { PlateServiceProxy, PlateDto } from '@shared/service-proxies/plate-service-proxies';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { CreateOrEditProductModalComponent } from './create-or-edit-product-modal.component';
import { ImportProductModalComponent } from './import-product-modal.component';
import * as $ from 'jquery';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import { Papa } from 'ngx-papaparse';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class ProductsComponent extends AppComponentBase {
  @ViewChild('importProductModal') importProductModal: ImportProductModalComponent;
  @ViewChild('createOrEditProductModal') createOrEditProductModal: CreateOrEditProductModalComponent;
  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('import') myInputVariable: ElementRef;

  advancedFiltersAreShown = true;
  filterText = '';
  nameFilter = '';
  skuFilter = '';
  barcodeFilter = '';
  categoryNameFilter = '';
  categories: CategoryDto[] = [];
  plateModels: PlateDto[] = [];

  constructor(
    injector: Injector,
    private _activatedRoute: ActivatedRoute,
    private _platesServiceProxy: PlateServiceProxy,
    private _categoriesServiceProxy: CategoryServiceProxy,
    private _productsServiceProxy: ProductServiceProxy,
    private papa: Papa
  ) {
    super(injector);
    this.primengTableHelper.defaultRecordsCountPerPage = 25;
  }

  // tslint:disable-next-line:use-life-cycle-interface
  ngOnInit() {
    this.getCategories();
    this.getPlateModels();
  }

  updatePrice(record: ProductDto) {
    if (record === null || record === undefined) {
      return;
    }
    if (record.price < 0) {
      this.notify.warn(this.l('PriceZero'));
      return;
    }

    if (record && record.price === null) {
      this.notify.warn('Price is not empty.');
      return;
    }

    this._productsServiceProxy.updatePrice(record).subscribe(result => {
      if (result === true) {
        this.notify.success(this.l('SavedSuccessfully'));
      } else {
        this.notify.error(this.l('SaveFailed'));
      }
    });
  }

  updateDisplayOrder(record: ProductDto) {
    if (record === null || record === undefined) {
      return;
    }
    if (record.displayOrder < 0) {
      this.notify.warn(this.l('PriceZero'));
      return;
    }

    if (record && record.displayOrder === null) {
      this.notify.warn('Display Order is not empty.');
      return;
    }

    this._productsServiceProxy.updateDisplayOrder(record).subscribe(result => {
      if (result === true) {
        this.notify.success(this.l('SavedSuccessfully'));
      } else {
        this.notify.error(this.l('SaveFailed'));
      }
    });
  }

  updateContractorPrice(record: ProductDto) {
    if (record === null || record === undefined) {
      return;
    }
    if (record.price < 0) {
      this.notify.warn(this.l('PriceZero'));
      return;
    }

    if (record && record.price === null) {
      this.notify.warn('Contractor Price is not empty.');
      return;
    }

    this._productsServiceProxy.updateContractorPrice(record).subscribe(result => {
      if (result === true) {
        this.notify.success(this.l('SavedSuccessfully'));
      } else {
        this.notify.error(this.l('SaveFailed'));
      }
    });
  }

  getPlateModels() {
    this.primengTableHelper.showLoadingIndicator();
    this._platesServiceProxy.getAll(
      '',
      '',
      '',
      '',
      '',
      true,
      '',
      9999,
      0
    ).subscribe(result => {
      result.items.forEach(element => {
        this.plateModels.push(element.plate);
      });
      this.primengTableHelper.hideLoadingIndicator();
    });
  }

  getCategories() {
    this._categoriesServiceProxy.getAll(
      '',
      '',
      '',
      '',
      0,
      9999
    ).subscribe(result => {
      result.items.forEach(element => {
        this.categories.push(element.category);
      });
    });
  }

  getProducts(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.primengTableHelper.showLoadingIndicator();
    this._productsServiceProxy.getAll(
      this.filterText,
      this.nameFilter,
      this.barcodeFilter,
      this.skuFilter,
      this.categoryNameFilter,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;

      this.primengTableHelper.hideLoadingIndicator();
    });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  createProduct(): void {
    this.createOrEditProductModal.show();
  }

  deleteProduct(product: ProductDto): void {
    this.message.confirm(
      'Are you sure you want to delete plate?',
      (isConfirmed) => {
        if (isConfirmed) {
          this._productsServiceProxy.delete(product.id)
            .subscribe(() => {
              this.reloadPage();
              this.notify.success(this.l('SuccessfullyDeleted'));
            });
        }
      }
    );
  }

  // Open modal import CSV.
  importCsvClick() {
    this.importProductModal.show();
  }

  // Action import file CSV.
  importCSV(files: File[]) {
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
    myReader.onloadend = ((e) => {
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

            let mess = new ImportResult();
            mess.successCount = 0;
            mess.errorCount = result.errors.length;
            mess.errorList = errorList;
            this.showImportMessage(mess, 'Invalid Data');
            this.myInputVariable.nativeElement.value = '';
            return;
          }

          //check empty plate code
          let csvImportData = new Array<CreateOrEditProductDto>();
          let errorCount = 0;
          for (let i = 0; i < result.data.length; i++) {
            let record = result.data[i];
            let item = new CreateOrEditProductDto();
            item.categoryId = record['Plate Category Id'];
            item.name = record['Product Name'];
            item.barcode = record['Barcode'];
            // item.sku = record['SKU'];
            item.imageUrl = record['Product Image'];
            item.desc = record['Product Description'];
            item.price = record['Product Price'];

            if (item.name === '') {
              errorList += ('<div>Row ' + (i + 2) + ' product name is empty</div>');
              errorCount++;
            }
            csvImportData.push(item);
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

          //check duplicate product name.
          let sorted_arr = csvImportData.slice().sort((a, b) => b.name < a.name ? 1 : -1);
          let resultsError = [];
          for (let i = 0; i < sorted_arr.length - 1; i++) {
            if (sorted_arr[i + 1].name === sorted_arr[i].name) {
              resultsError.push(sorted_arr[i]);
            }
          }
          if (resultsError.length > 0) {
            resultsError.forEach(element => {
              errorList += ('<div>Duplicate product name ' + element.name + '</div>');
            });
            let mess = new ImportResult();
            mess.successCount = 0;
            mess.errorCount = resultsError.length;
            mess.errorList = errorList;
            this.showImportMessage(mess, 'Invalid Data');
            this.myInputVariable.nativeElement.value = '';
            return;
          }

          // validate ok, submit data
          this._productsServiceProxy.importProduct(csvImportData)
            .subscribe((result) => {
              this.showImportMessage(result, 'Import Result');
              this.myInputVariable.nativeElement.value = '';
              this.reloadPage();
            });
        }
      });
    });
    myReader.readAsText(file);
  }

  // Action export file CSV.
  exportToCsv(): void {
    if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
      this.primengTableHelper.showLoadingIndicator();
      this._productsServiceProxy.getAll(
        this.filterText,
        this.nameFilter,
        this.barcodeFilter,
        this.skuFilter,
        this.categoryNameFilter,
        this.primengTableHelper.getSorting(this.dataTable),
        0,
        10000
      ).subscribe(result => {
        if (result.items != null) {
          let csvData = new Array();
          const header = {
            // ProductCategoryId: 'Product Category Id',
            ProductCategoryName: 'Product Category Name',
            ProductName: 'Product Name',
            Sku: 'SKU',
            Barcode: 'Barcode',
            // SKU: 'SKU',
            ProductImage: 'Product Image',
            ProductDescription: 'Product Description',
            ProductPrice: 'Product Price',
            displayOrder: 'Display Order'
          };
          csvData.push(header);

          for (let record of result.items) {
            csvData.push({
              // ProductCategoryId: (record.product.categoryId == null) ? '' : record.product.categoryId,
              PlateCategoryName: (record.categoryName == null) ? '' : record.categoryName,
              ProductName: (record.product.name == null) ? '' : record.product.name,
              Sku: (record.product.sku == null) ? '' : record.product.sku,
              // PlateId: (record.product.plateId == null) ? '' : record.product.plateId,
              // PlateName: (record.plateName == null) ? '' : record.plateName,
              // PlateCode: (record.plateCode == null) ? '' : '_' + record.plateCode,
              Barcode: (record.product.barcode == null) ? '' : '_' + record.product.barcode,
              // SKU: (record.product.sku == null) ? '' : record.product.sku,
              ProductImage: (record.product.imageUrl == null) ? '' : record.product.imageUrl,
              ProductDescription: (record.product.desc == null) ? '' : record.product.desc,
              ProductPrice: (record.product.price == null) ? '' : record.product.price,
              displayOrder: (record.product.displayOrder == null) ? '' : record.product.displayOrder
            });
          }
          // tslint:disable-next-line:no-unused-expression
          var d = new Date();
          var dateStr = "" + d.getFullYear() + (d.getMonth() + 1) + d.getDate();
          new Angular5Csv(csvData, 'Products-' + dateStr);
        } else {
          this.notify.info(this.l('DateEmpty'));
        }
        this.primengTableHelper.hideLoadingIndicator();
      });
    } else {
      this.notify.info(this.l('DateEmpty'));
    }
  }

  showImportMessage(result: ImportResult, title: string) {
    let content = '<div class="text-left" style="margin-left: 20px;"><div>- Success: ' + result.successCount + '</div>';
    content += '<div>- Failed: ' + result.errorCount + '</div>';
    content += '<div>- Failed Rows: ' + result.errorList + '</div></div>';

    abp.message.info(content, title, true);
  }
}
