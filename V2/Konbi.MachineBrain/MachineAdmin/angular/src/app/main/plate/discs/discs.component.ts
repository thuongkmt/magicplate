import { Component, Injector, ViewEncapsulation, ViewChild, NgZone, AfterViewInit } from '@angular/core';
import { ActivatedRoute, Params } from '@angular/router';
import { Http } from '@angular/http';
import { DiscsServiceProxy, DiscDto } from '@shared/service-proxies/service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditDiscModalComponent } from './create-or-edit-disc-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';

import { MessageSignalrService } from '../../../shared/common/signalr/message-signalr.service';
import { PlateServiceProxy } from '@shared/service-proxies/plate-service-proxies';
import { PlateCategoryServiceProxy } from '@shared/service-proxies/plate-category-service-proxies';

@Component({
    templateUrl: './discs.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()]
})
export class DiscsComponent extends AppComponentBase implements AfterViewInit {

    @ViewChild('createOrEditDiscModal') createOrEditDiscModal: CreateOrEditDiscModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;

    discsHeaderInfo = '';

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
        public _zone: NgZone,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy

    ) {
        super(injector);
        this.primengTableHelper.defaultRecordsCountPerPage = 25;

        this.route.params.forEach((params: Params) => {
            if (params['plate_id'] !== undefined) {
                this.plateIdFilter = params['plate_id'];
                this.getPlateInfo(params['plate_id']);
            }
        });
    }

    ngOnInit(): void {

    }

    ngAfterViewInit() {
        if (this.plateIdFilter === '') {
            this.createOrEditDiscModal.show();
        }
    }

    getPlateInfo(plateId): void {
        this._platesServiceProxy.getPlateForView(plateId).subscribe(result => {
            this.plateIdFilter = result.plate.id;
            this.plateNameFilter = result.plate.name;
            this.plateCodeFilter = result.plate.code;
            this.discsHeaderInfo = 'Plate Code ' + result.plate.code;
            this.createOrEditDiscModal.show();
            //this.dataTable.onLazyLoad.emit();
            this.getDiscs();
        });
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

    createDisc(): void {
        this.createOrEditDiscModal.show();
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

    exportToExcel(): void {
        this._discsServiceProxy.getDiscsToExcel(
            this.filterText,
            this.uidFilter,
            this.codeFilter,
            this.plateNameFilter,
        )
            .subscribe(result => {
                this._fileDownloadService.downloadTempFile(result);
            });
    }

    syncData() {
        this.primengTableHelper.showLoadingIndicator();
        // Sync plate category.
        // this._plateCategoriesServiceProxy.syncData()
        // .finally(() => {})
        // .subscribe(() => {
        //     // Sync plate.
        //     this._platesServiceProxy.syncPlate()
        //         .finally(() => {})
        //         .subscribe(() => {
        //             // Sync disc.
        //             this._discsServiceProxy.syncData()
        //                 .finally(() => {
        //                     this.primengTableHelper.hideLoadingIndicator();
        //                 })
        //                 .subscribe(() => {
        //                     this.reloadPage();
        //                     this.primengTableHelper.hideLoadingIndicator();
        //                     this.notify.success('Data synced');
        //                 });
        //         });
        // });
        this._discsServiceProxy.syncPlateDataFromServer()
            .finally(() => {
                this.primengTableHelper.hideLoadingIndicator();
            })
            .subscribe(() => {
                this.reloadPage();
                this.primengTableHelper.hideLoadingIndicator();
                this.notify.success('Data synced');
            });
    }


    syncPlate() {
        // Sync plate category.
        this._plateCategoriesServiceProxy.syncData()
            .finally(() => { })
            .subscribe(() => {
                // Sync plate.
                this._platesServiceProxy.syncPlate()
                    .finally(() => {
                        this.primengTableHelper.hideLoadingIndicator();
                    })
                    .subscribe(() => {
                        this.primengTableHelper.hideLoadingIndicator();
                    });
            });
    }
}
