import { Component, Injector, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AppComponentBase } from '@shared/common/app-component-base';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { Table } from 'primeng/components/table/table';
import { Paginator } from 'primeng/components/paginator/paginator';
import { LazyLoadEvent } from 'primeng/components/common/lazyloadevent';
import { ServiceStatusService, ServiceStatusDto} from '@shared/service-proxies/service-status.service'
import * as _ from 'lodash';
import { forEach } from '@angular/router/src/utils/collection';

@Component({
  templateUrl: './service-status.component.html',
  styleUrls: ['./service-status.component.css'],
  encapsulation: ViewEncapsulation.None,
  animations: [appModuleAnimation()]
    
})
export class ServiceStatusComponent extends AppComponentBase {
    advancedFiltersAreShown = false;
    services : ServiceStatusDto[];
    archiveServices : ServiceStatusDto[];
    constructor(
      injector:Injector,
      private _serviceStatusService : ServiceStatusService
    ) { 
      super(injector);
    }

    ngOnInit() {
    }

    getAllServices(event?: LazyLoadEvent){
        this.primengTableHelper.showLoadingIndicator();
        this._serviceStatusService.getAllServices().subscribe(result => {
          this.primengTableHelper.records = result.items;
          this.services = result.items.filter(x => x.isArchived == false);
          this.archiveServices = result.items.filter(x => x.isArchived == true);

          this.services.forEach(item => {
            this._serviceStatusService.getServiceStatus(item.id).subscribe(result => {
                item.status = result.status;
                item.message = result.message;
            });
          });

          this.primengTableHelper.hideLoadingIndicator();
      });
    }

    updateService(service: ServiceStatusDto){
        service.isArchived = !service.isArchived;

        this._serviceStatusService.updateService(service)
        .finally(() => { })
        .subscribe(() => {
          this.getAllServices();
          this.notify.info(this.l('Update successfully'));
        });
    }
}
