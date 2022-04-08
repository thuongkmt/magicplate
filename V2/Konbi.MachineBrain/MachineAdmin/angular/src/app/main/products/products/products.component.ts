import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { CategoryServiceProxy, ProductServiceProxy, CategoryDto, ProductDto } from '@shared/service-proxies/service-proxies';
import { PlateServiceProxy, PlateDto } from '@shared/service-proxies/plate-service-proxies';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { CreateOrEditProductModalComponent } from './create-or-edit-product-modal.component';
import * as $ from 'jquery';

@Component({
  selector: 'app-products',
  templateUrl: './products.component.html',
  styleUrls: ['./products.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class ProductsComponent extends AppComponentBase {

  @ViewChild('createOrEditProductModal') createOrEditProductModal: CreateOrEditProductModalComponent;
  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;

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
  ) {
    super(injector);
    this.primengTableHelper.defaultRecordsCountPerPage = 25;
  }

  ngOnInit() {
    this.getCategories();
    this.getPlateModels();
  }

  // updatePlate(event: any, record: ProductDto) {
  //   if (record === null || record === undefined) {
  //     return;
  //   }

  //   record.plateId = event.target.value;

  //   this._productsServiceProxy.updatePlate(record).subscribe(result => {
  //     if (result === 'ok') {
  //         this.notify.success(this.l('SavedSuccessfully'));
  //     } else {
  //       $('#' + record.id).val('00000000-0000-0000-0000-000000000000');
  //         this.notify.error(result);
  //     }
  //   });
  // }

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

  syncData() {
    this.primengTableHelper.showLoadingIndicator();
    this._productsServiceProxy.syncProductData()
      .finally(() => {
        this.primengTableHelper.hideLoadingIndicator();
      })
      .subscribe(() => {
        this.reloadPage();
        this.primengTableHelper.hideLoadingIndicator();
        this.notify.success('Data synced');
      });
  }
}
