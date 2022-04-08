import { Component, Injector, ViewEncapsulation, ViewChild, NgZone, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Http } from '@angular/http';
import { DiscsServiceProxy, DiscDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
// import { CreateOrEditDiscModalComponent } from './create-or-edit-disc-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

import { MessageSignalrService } from '../../../shared/common/signalr/message-signalr.service';
import { PlateServiceProxy } from '@shared/service-proxies/plate-service-proxies';

import { DiscCheckModalComponent } from './disc-check-modal/disc-check-modal.component';

@Component({
  selector: 'app-disc-check',
  templateUrl: './disc-check.component.html',
  styleUrls: ['./disc-check.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
})
export class DiscCheckComponent extends AppComponentBase {

  @ViewChild('dataTable') dataTable: Table;
  @ViewChild('paginator') paginator: Paginator;
  @ViewChild('checkDishModal') checkDishModal: DiscCheckModalComponent;
  advancedFiltersAreShown = false;
  filterText = '';
  uidFilter = '';
  codeFilter = '';
  plateNameFilter = '';
  plateCodeFilter = '';
  plateIdFilter = '';

  constructor(
    injector: Injector,
    private _discsServiceProxy: DiscsServiceProxy,
    private _platesServiceProxy: PlateServiceProxy,
    private _notifyService: NotifyService,
    private _tokenAuth: TokenAuthServiceProxy,
    private _activatedRoute: ActivatedRoute,
    private _fileDownloadService: FileDownloadService,
    private route: ActivatedRoute,
    private _messageSignalrService: MessageSignalrService,
    public _zone: NgZone
  ) {
    super(injector);
    this.primengTableHelper.defaultRecordsCountPerPage = 25;
  }

  ngOnInit() {
  }

  getDiscs(event?: LazyLoadEvent) {
    if (this.primengTableHelper.shouldResetPaging(event)) {
      this.paginator.changePage(0);
      return;
    }
    this.primengTableHelper.showLoadingIndicator();
    this._discsServiceProxy.getAll(
      this.filterText,
      this.uidFilter,
      this.codeFilter,
      this.plateNameFilter,
      this.plateIdFilter,
      this.primengTableHelper.getSorting(this.dataTable),
      this.primengTableHelper.getSkipCount(this.paginator, event),
      this.primengTableHelper.getMaxResultCount(this.paginator, event)
    )
      .finally(() => this.primengTableHelper.hideLoadingIndicator())
      .subscribe(result => {
        this.primengTableHelper.totalRecordsCount = result.totalCount;
        this.primengTableHelper.records = result.items;
        this.primengTableHelper.hideLoadingIndicator();
      });
  }

  reloadPage(): void {
    this.paginator.changePage(this.paginator.getPage());
  }

  deleteDisc(disc: DiscDto): void {
    this.message.confirm(
      '',
      (isConfirmed) => {
        if (isConfirmed) {
          this._discsServiceProxy.delete(disc.id, disc.code, disc.plateId, disc.id)
            .subscribe(() => {
              this.reloadPage();
              this.notify.success(this.l('SuccessfullyDeleted'));
            });
        }
      }
    );
  }

  btnShowCheckDishModalClick() {
    this.checkDishModal.show(this.primengTableHelper.records);
  }
}
