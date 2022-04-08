import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Http } from '@angular/http';
import { PlateServiceProxy, PlateDto, CreateOrEditPlateDto, ImportResult } from '@shared/service-proxies/plate-service-proxies';
import { NotifyService } from '@abp/notify/notify.service';
import { AppComponentBase } from '@shared/common/app-component-base';
import { TokenAuthServiceProxy } from '@shared/service-proxies/service-proxies';
import { CreateOrEditPlateModalComponent } from './create-or-edit-plate-modal.component';
import { ImportPlateModalComponent } from './import-plate-modal.component';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { FileDownloadService } from '@shared/utils/file-download.service';
import * as _ from 'lodash';
import * as moment from 'moment';
import { Angular5Csv } from 'angular5-csv/Angular5-csv';
import { Papa } from 'ngx-papaparse';
import { truncate } from 'fs';
import { PlateCategoryServiceProxy, PlateCategoryDto } from '@shared/service-proxies/plate-category-service-proxies';
import { Router } from '@angular/router';

@Component({
    templateUrl: './plates.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: [appModuleAnimation()],
    styleUrls: ['./plates.component.css']
})
export class PlatesComponent extends AppComponentBase {

    @ViewChild('createOrEditPlateModal') createOrEditPlateModal: CreateOrEditPlateModalComponent;
    @ViewChild('importPlateModal') importPlateModal: ImportPlateModalComponent;
    @ViewChild('dataTable') dataTable: Table;
    @ViewChild('paginator') paginator: Paginator;
    @ViewChild('import') myInputVariable: ElementRef;

    advancedFiltersAreShown = true;
    filterText = '';
    nameFilter = '';
    imageUrlFilter = '';
    descFilter = '';
    codeFilter = '';
    maxAvaiableFilter: number;
    maxAvaiableFilterEmpty: number;
    minAvaiableFilter: number;
    minAvaiableFilterEmpty: number;
    colorFilter = '';
    plateCategoryNameFilter = '';
    categories: PlateCategoryDto[] = [];
    isPlate = true;

    constructor(
        injector: Injector,
        private _platesServiceProxy: PlateServiceProxy,
        private _plateCategoriesServiceProxy: PlateCategoryServiceProxy,
        private papa: Papa,
        private router: Router,
    ) {
        super(injector);
        this.primengTableHelper.defaultRecordsCountPerPage = 25;

        if (this.router.url.includes('trays')) {
            this.isPlate = false;
        } else {
            this.isPlate = true;
        }
    }

    // tslint:disable-next-line:use-life-cycle-interface
    ngOnInit() {
        this.getPlateCategories();
      }

    getPlateCategories() {
        this._plateCategoriesServiceProxy.getAll(
            '',
            '',
            '',
            100,
            0
        ).subscribe(result => {
            result.items.forEach(element => {
                this.categories.push(element.plateCategory);
            });
        });
    }

    getPlates(event?: LazyLoadEvent) {
        if (this.primengTableHelper.shouldResetPaging(event)) {
            this.paginator.changePage(0);
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        this._platesServiceProxy.getAll(
            this.filterText,
            this.nameFilter,
            this.codeFilter,
            this.colorFilter,
            this.plateCategoryNameFilter,
            this.isPlate,
            this.primengTableHelper.getSorting(this.dataTable),
            this.primengTableHelper.getMaxResultCount(this.paginator, event),
            this.primengTableHelper.getSkipCount(this.paginator, event)
        ).subscribe(result => {
            this.primengTableHelper.totalRecordsCount = result.totalCount;
            this.primengTableHelper.records = result.items;
            this.primengTableHelper.hideLoadingIndicator();
        });
    }

    reloadPage(): void {
        this.paginator.changePage(this.paginator.getPage());
    }

    createPlate(): void {
        this.createOrEditPlateModal.show(this.isPlate);
    }

    deletePlate(plate: PlateDto): void {
        this.message.confirm(
            'Are you sure you want to delete?',
            (isConfirmed) => {
                if (isConfirmed) {
                    this._platesServiceProxy.delete(plate.id)
                        .subscribe(() => {
                            this.reloadPage();
                            this.notify.success(this.l('SuccessfullyDeleted'));
                        });
                }
            }
        );
    }

    plateInventoryManagerClick(plateId) {
        // debugger
        let routerUrl = 'app/main/plate/dishes/' + plateId;
        this.router.navigate([routerUrl]);
    }

    exportToCsv(): void {
        if (this.primengTableHelper.records != null && this.primengTableHelper.records.length > 0) {
            this.primengTableHelper.showLoadingIndicator();
            this._platesServiceProxy.getAll(
                this.filterText,
                this.nameFilter,
                this.codeFilter,
                this.colorFilter,
                this.plateCategoryNameFilter,
                this.isPlate,
                this.primengTableHelper.getSorting(this.dataTable),
                1000,
                0
            ).subscribe(result => {
                if (result.items != null) {
                    let csvData = new Array();
                    if (this.isPlate) {
                        const header = {
                            PlateCategoryName: 'Plate Category Name',
                            PlateName: 'Plate Name',
                            PlateImage: 'Plate Image',
                            PlateDescription: 'Plate Description',
                            PlateCode: 'Plate Code',
                            PlateColor: 'Plate Color'
                        };
                        csvData.push(header);
                    } else {
                        const header = {
                            TrayName: 'Tray Name',
                            TrayImage: 'Tray Image',
                            TrayDescription: 'Tray Description',
                            TrayCode: 'Tray Code',
                            TrayColor: 'Tray Color'
                        };
                        csvData.push(header);
                    }

                    for (let record of result.items) {
                        if (this.isPlate) {
                            csvData.push({
                                PlateCategoryName: (record.plateCategoryName == null) ? '' : record.plateCategoryName,
                                PlateName: (record.plate.name == null) ? '' : record.plate.name,
                                PlateImage: (record.plate.imageUrl == null) ? '' : record.plate.imageUrl,
                                PlateDescription: (record.plate.desc == null) ? '' : record.plate.desc,
                                PlateCode: '_' + record.plate.code,
                                PlateColor: (record.plate.color == null) ? '' : record.plate.color
                            });
                        } else {
                            csvData.push({
                                TrayName: (record.plate.name == null) ? '' : record.plate.name,
                                TrayImage: (record.plate.imageUrl == null) ? '' : record.plate.imageUrl,
                                TrayDescription: (record.plate.desc == null) ? '' : record.plate.desc,
                                TrayCode: '_' + record.plate.code,
                                TrayColor: (record.plate.color == null) ? '' : record.plate.color
                            });
                        }
                    }
                    if (this.isPlate) {
                        // tslint:disable-next-line:no-unused-expression
                        new Angular5Csv(csvData, 'Plates');
                    } else {
                        // tslint:disable-next-line:no-unused-expression
                        new Angular5Csv(csvData, 'Trays');
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

    importCsvClick() {
        this.importPlateModal.show(this.isPlate);
    }

    showImportMessage(result: ImportResult, title: string) {
        let content = '<div class="text-left" style="margin-left: 20px;"><div>- Success: ' + result.successCount + '</div>';
        content += '<div>- Failed: ' + result.errorCount + '</div>';
        content += '<div>- Failed Rows: ' + result.errorList + '</div></div>';

        abp.message.info(content, title, true);
    }
}
