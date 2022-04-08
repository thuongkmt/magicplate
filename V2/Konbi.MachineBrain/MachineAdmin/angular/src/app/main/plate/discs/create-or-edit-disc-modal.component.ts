import { Component, ViewChild, Injector, Output, EventEmitter, NgZone } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { DiscsServiceProxy, CreateOrEditDiscDto, CheckInventoryDto } from '@shared/service-proxies/service-proxies';
import { PlateServiceProxy, PlateInventoryDto } from '@shared/service-proxies/plate-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PlateLookupTableModalComponent } from './plate-lookup-table-modal.component';
import { ISubscription } from "rxjs/Subscription";
import { ParentChildService } from '../../../shared/common/parent-child/parent-child.service';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { MessageSignalrService } from '../../../shared/common/signalr/message-signalr.service';

@Component({
    selector: 'createOrEditDiscModal',
    templateUrl: './create-or-edit-disc-modal.component.html',
    styleUrls: ['./create-or-edit-disc-modal.component.css'],
})
export class CreateOrEditDiscModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('plateLookupTableModal') plateLookupTableModal: PlateLookupTableModalComponent;

    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    public storedInventories: Array<CheckInventoryDto> = new Array<CheckInventoryDto>();
    public plates: Array<PlateInventoryDto> = new Array<PlateInventoryDto>();
    public lstCreateDish: Array<CreateOrEditDiscDto> = new Array<CreateOrEditDiscDto>();

    // plateId = '';
    // plateCode = '';
    // plateName = '';
    isCreateForPlate = false;

    private onDetectDishFromTable: ISubscription;

    constructor(
        injector: Injector,
        private _discsServiceProxy: DiscsServiceProxy,
        private _plateService: PlateServiceProxy,
        public _zone: NgZone,
        private comParentChildService: ParentChildService,
        private _messageSignalrService: MessageSignalrService,
    ) {
        super(injector);
    }

    ngOnInit(): void {
        if (this.appSession.application) {
            if (!this._messageSignalrService.isMessageConnected) {
                SignalRHelper.initSignalR(() => {
                    this._messageSignalrService.init();
                });
            }
        }
        if (!this.onDetectDishFromTable) {
            this.onDetectDishFromTable = this.comParentChildService.on('app.message.messageReceived').subscribe($data => this.onMessageReceived($data));
        }
    }

    ngOnDestroy() {
        if (this.onDetectDishFromTable) {
            this.onDetectDishFromTable.unsubscribe();
        }
    }

    onMessageReceived(message) {
        if (!this.active) return;
        this.lstCreateDish = new Array<CreateOrEditDiscDto>();
        let mess = JSON.parse(message.message);
        if (mess.type != 'RFIDTable_DetectedDisc') return;

        let tmpDishes: Array<CreateOrEditDiscDto> = new Array<CreateOrEditDiscDto>();
        debugger
        for (let dish of mess.data.Plates) {
            var existPlates = this.plates.filter(x => x.plateCode === dish.UType);
            if (existPlates.length <= 0) {
                let errMessage = 'The plate with code: ' + dish.UType + ' does not exist.';
                abp.message.error(errMessage);
                return;
            }

            if (this.storedInventories.filter(item => item.uid === dish.UID).length > 0
                || this.lstCreateDish.filter(item => item.uid === dish.UID).length > 0
                || tmpDishes.filter(item => item.uid === dish.UID).length > 0
            ) {
                let errMessage = 'The plate ID ' + dish.UID + ' is already registered, please use another plate.';
                abp.message.info(errMessage);
                return;
            }

            let newDish = new CreateOrEditDiscDto();
            newDish.plateId = existPlates[0].plateId;
            //newDish.plateName = existPlates[0].plateName;
            newDish.code = dish.UType;
            newDish.uid = dish.UID;
            tmpDishes.push(newDish);
        }

        this._zone.run(() => {
            this.lstCreateDish = this.lstCreateDish.concat(tmpDishes);
        });

        //abp.utils.formatString('{0}: {1}', 'aaaaaaaaaaaaa', abp.utils.truncateString(message.message, 100));
    }

    show(): void {
        abp.ui.setBusy();
        this._discsServiceProxy.getAllDish()
            .finally(() => abp.ui.clearBusy())
            .subscribe(result => {
                debugger
                this.storedInventories = result.items;
                abp.ui.clearBusy();
            });

        abp.ui.setBusy();
        this._plateService.getPlateInventory()
            .finally(() => abp.ui.clearBusy())
            .subscribe(result => {
                this.plates = result.items;
                abp.ui.clearBusy();
            });
        this.active = true;
        this.modal.show();
    }

    save(): void {
        this.saving = true;
        this._discsServiceProxy.createOrEdit(this.lstCreateDish)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    //delete dishes item in list create
    deleteDisc(dish) {
        this.lstCreateDish = this.lstCreateDish.filter(function (obj) {
            return obj.uid !== dish.uid;
        });
    }

    close(): void {
        // this.plateId = null;
        // this.plateId = '';
        // this.plateName = '';
        this.lstCreateDish = new Array<CreateOrEditDiscDto>();
        this.storedInventories = new Array<CheckInventoryDto>();
        this.active = false;
        this.modal.hide();
    }
}
