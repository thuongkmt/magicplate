import { Component, ViewChild, Injector, Output, EventEmitter, NgZone } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { DiscsServiceProxy, CreateOrEditDiscDto, GetDiscForView, PlateLookupTableDto, DiscDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { ISubscription } from "rxjs/Subscription";
//add signalr
import { SignalRHelper } from 'shared/helpers/SignalRHelper';
import { MessageSignalrService } from '../../../../shared/common/signalr/message-signalr.service';
import { ParentChildService } from '../../../../shared/common/parent-child/parent-child.service'
import { stat } from 'fs';

@Component({
  selector: 'checkDishModal',
  templateUrl: './disc-check-modal.component.html',
  styleUrls: ['./disc-check-modal.component.css']
})
export class DiscCheckModalComponent extends AppComponentBase {

  @ViewChild('checkDishModal') modal: ModalDirective;
  @Output() modalSaveEvent: EventEmitter<any> = new EventEmitter<any>();

  active = false;
  saving = false;

  public lstAllPlate: Array<PlateLookupTableDto> = new Array<PlateLookupTableDto>();

  public listDishAvailabe: Array<GetDiscForView> = new Array<GetDiscForView>();
  public lstScanningDish: Array<DiscDtoForDisplay> = new Array<DiscDtoForDisplay>();
  public lstListNewDish: Array<DiscDtoForDisplay> = new Array<DiscDtoForDisplay>();
  public lstDelete: Array<DiscDtoForDisplay> = new Array<DiscDtoForDisplay>();

  plateId = '';
  plateCode = '';
  plateName = '';
  isCreateForPlate = false;

  private onDetectDishFromTable: ISubscription

  constructor(
    injector: Injector,
    private _discsServiceProxy: DiscsServiceProxy,
    private _messageSignalrService: MessageSignalrService,
    public _zone: NgZone,
    private comParentChildService: ParentChildService
  ) {
    super(injector);
  }


  ngOnInit() {

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

  show(listAllDish: Array<GetDiscForView>): void {
    this.getAllPlate();
    this.listDishAvailabe = listAllDish
    this.lstDelete = new Array<DiscDtoForDisplay>();
    this.active = true;
    this.modal.show();
  }

  close(): void {
    this.plateId = '';
    this.plateName = '';

    this.lstScanningDish = new Array<DiscDtoForDisplay>();
    this.lstListNewDish = new Array<DiscDtoForDisplay>();
    this.lstDelete = new Array<DiscDtoForDisplay>();
    this.active = false;
    this.modal.hide();

  }

  getAllPlate() {
    this._discsServiceProxy.getAllPlateForLookupTable("", "", 0, 1000).subscribe(result => {
      this.lstAllPlate = result.items;
    });
  }

  onMessageReceived(message) {
    let mess = JSON.parse(message.message);

    if (mess.type = 'RFIDTable_DetectedDisc') {
      if (!this.active) return

      for (let dish of mess.data.Plates) {
        if (this.lstScanningDish.find(x => x.code == dish.UType && x.uid == dish.UID)) continue;
        let newDish: any;
        let dishPlate = this.lstAllPlate.find(x => x.code == dish.UType)
        if (dishPlate) {
          let dishAvailable = this.listDishAvailabe.find(x => x.disc.code == dish.UType && x.disc.uid == dish.UID);

          if (dishAvailable) {
            newDish = new DiscDtoForDisplay(dish.UID, dish.UType, dishAvailable.disc.plateId, 1, dishAvailable.disc.id)
          }
          else {
            newDish = new DiscDtoForDisplay(dish.UID, dish.UType, dishPlate.id, 2, "")
            this.lstListNewDish.push(newDish)
          }
        } else {
          newDish = new DiscDtoForDisplay(dish.UID, dish.UType, "", 3, "")
          abp.message.error(newDish.status)
        }

        this._zone.run(() => {
          this.lstScanningDish.push(newDish);
        });

      }

    }
  };

  //delete dishes item in list create
  deleteDisc(dish) {
    this.lstScanningDish = this.lstScanningDish.filter(function (obj) {
      return obj.uid !== dish.uid;
    });
    this.lstListNewDish = this.lstListNewDish.filter(function (obj) {
      return obj.uid !== dish.uid;
    });
    if (dish.statusCode == 1) {
      this.lstDelete.push(dish);
    }
  }

  btnUpdateSyncClick() {
    //delete missing plate
    if (this.lstDelete.length > 0) {
      this.deleteMissingPlateInventory();
    }
    else if (this.lstListNewDish.length > 0) {
      this.addNewPlateInventory();
    }
  }

  deleteMissingPlateInventory() {
    let listDishDto = new Array<DiscDto>();
    this.lstDelete.forEach(disc => {
      var dto = new DiscDto();
      dto.id = disc.id;
      dto.code = disc.code;
      dto.plateId = disc.plateId;
      dto.uid = disc.uid;

      listDishDto.push(dto)
    });
    this.saving = true;
    this._discsServiceProxy.postDeleteDishes(listDishDto)
      .pipe(finalize(() => { this.saving = false; }))
      .subscribe(() => {
        if (this.lstListNewDish.length > 0) {
          this.addNewPlateInventory()
        } else {
          this.notify.info(this.l('Update Plate Inventory Successfully'));
          this.modalSaveEvent.emit(null);
          this.close();
        }
      });
  }

  addNewPlateInventory() {
    let lstCreateDish = new Array<CreateOrEditDiscDto>();
    this.lstListNewDish.forEach(element => {
      let newItem = new CreateOrEditDiscDto();
      newItem.uid = element.uid;
      newItem.code = element.code;
      newItem.plateId = element.plateId;
      lstCreateDish.push(newItem);
    });
    this.saving = true;
    this._discsServiceProxy.createOrEdit(lstCreateDish)
      .pipe(finalize(() => { this.saving = false; }))
      .subscribe(() => {
        this.notify.info(this.l('Update Plate Inventory Successfully'));
        this.modalSaveEvent.emit(null);
        this.close();
      });
  }

}

export class DiscDtoForDisplay {
  uid: string;
  code: string;
  plateId: string;
  id: string;
  status: string;
  statusStyle: string;
  statusCode: number; // 1: availabe, 2: new, 3: plate code not register

  constructor(_uid: string, _code: string, _plateId: string, _statusCode: number, _id: string) {
    this.uid = _uid;
    this.code = _code;
    this.plateId = _plateId;
    this.statusCode = _statusCode;
    this.statusStyle = "black";
    this.id = _id;
    if (this.statusCode == 1) this.status = "Plate Availabe";
    else if (this.statusCode == 2) this.status = "Plate New";
    else {
      this.status = '<div style="max-width: 200px;"><div>Plate Code ' + this.code + ' could not be found</div><div>Please add plate model to server first and sync to machine</div></div>';
      this.statusStyle = "red";
    }
  }

}
