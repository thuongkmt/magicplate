import { Component, Injector, ViewEncapsulation, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { CategoryServiceProxy, CategoryDto } from '@shared/service-proxies/service-proxies';
import { CreateOrEditCategoryModalComponent } from './create-or-edit-category-modal.component';

@Component({
  selector: 'app-products-categories',
  templateUrl: './products-categories.component.html',
  styleUrls: ['./products-categories.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})

export class ProductsCategoriesComponent extends AppComponentBase {

  @ViewChild('createOrEditCategoryModal') createOrEditCategoryModal: CreateOrEditCategoryModalComponent;
  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;

  advancedFiltersAreShown = false;
  filterText = '';
  nameFilter = '';
  descFilter = '';

  constructor(
    injector: Injector,
    private _categoriesServiceProxy: CategoryServiceProxy,
    private _notifyService: NotifyService,
    private _tokenAuth: TokenAuthServiceProxy,
    private _activatedRoute: ActivatedRoute
    ) {
    super(injector);
    this.primengTableHelper.defaultRecordsCountPerPage = 25;
  }

  // Get all category.
  getProductCategories(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }

    this.primengTableHelper.showLoadingIndicator();

    this._categoriesServiceProxy.getAll(
      this.filterText,
      this.nameFilter,
      this.descFilter,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    ).subscribe(result => {
      this.primengTableHelper.totalRecordsCount = result.totalCount;
      this.primengTableHelper.records = result.items;

      this.primengTableHelper.hideLoadingIndicator();
    });
  }

  createProductCategory() {
    this.createOrEditCategoryModal.show();
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  deleteCategory(category: CategoryDto): void {
    this.message.confirm(
      '',
      (isConfirmed) => {
        if (isConfirmed) {
          this._categoriesServiceProxy.delete(category.id)
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
      this._categoriesServiceProxy.syncProductCategoryData()
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
