import { Component, ViewChild, Injector, Output, EventEmitter, NgZone } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { PlateServiceProxy, CreateOrEditPlateDto } from '@shared/service-proxies/plate-service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { PlateCategoryLookupTableModalComponent } from './plateCategory-lookup-table-modal.component';
import { Router } from '@angular/router';

@Component({
    selector: 'createOrEditPlateModal',
    templateUrl: './create-or-edit-plate-modal.component.html',
    styleUrls: ['./create-or-edit-plate-modal.component.css']
})
export class CreateOrEditPlateModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @ViewChild('plateCategoryLookupTableModal') plateCategoryLookupTableModal: PlateCategoryLookupTableModalComponent;

    @ViewChild('scanPlateModal') public scanPlateModal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    isPlate = true;
    plate: CreateOrEditPlateDto = new CreateOrEditPlateDto();
    plateCategoryName = '';
    defaultPlateImage: string = 'assets/common/images/ic_nophoto.jpg'

    //upload photo
    files: File[] = []
    lastInvalids: any;
    lastFileAt: Date

    getDate() {
        if (this.files.length > 3) {
            alert("Please select maximun 3 images");
            this.files = []
        }
        return new Date()
    }

    constructor(
        injector: Injector,
        private _platesServiceProxy: PlateServiceProxy,
        public _zone: NgZone,
        private router: Router
    ) {
        super(injector);
    }

    ngOnInit(): void {
        // if (this.appSession.application) {
        //     SignalRHelper.initSignalR(() => {
        //         this._messageSignalrService.init();
        //     });
        // }
    }

    show(isPlate:boolean, plateId?: string): void {
        this.isPlate = isPlate;
        if (!plateId) {
            this.plate = new CreateOrEditPlateDto();
            this.plate.isPlate = this.isPlate;
            this.plate.imageUrl = this.defaultPlateImage;
            this.plateCategoryName = '';
            this.active = true;
            //this.registerEvents()
            this.modal.show();
        }
        else {
            this._platesServiceProxy.get(plateId).subscribe(result => {
                this.plate = result.plate;
                if (!this.plate.imageUrl) this.plate.imageUrl = this.defaultPlateImage;
                this.plateCategoryName = result.plateCategoryName;
                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
        this.primengTableHelper.showLoadingIndicator();
        this.saving = true;
        //upload images first
        if (this.files.length > 0) {
            this._platesServiceProxy.uploadFile(this.files)
                .subscribe(
                    response => {

                        if (response.result.length > 0) {
                            let arrImage = response.result
                            let images = '';
                            for (let index = 0; index < arrImage.length; index++) {
                                (index == arrImage.length - 1) ? (images += arrImage[index]) : images += arrImage[index] + '|'
                            }
                            this.plate.imageUrl = images

                            this._platesServiceProxy.createOrEdit(this.plate)
                                .pipe(finalize(() => {
                                    this.saving = false;
                                }))
                                .subscribe((result) => {
                                    if (result.message != null) {
                                        abp.message.error(result.message);
                                    }
                                    else {
                                        this.files = []
                                        this.close();
                                        this.modalSave.emit(null);
                                        this.notify.info(this.l('SavedSuccessfully'));
                                    }
                                });
                        }
                    }, err => {
                        console.log(err)
                    }
                )
        } else {
            this._platesServiceProxy.createOrEdit(this.plate)
                .pipe(finalize(() => {
                    this.saving = false;
                }))
                .subscribe((result) => {
                    if (result.message != null) {
                        abp.message.error(result.message);
                    }
                    else {
                        this.files = []
                        this.close();
                        this.modalSave.emit(null);
                        this.notify.info(this.l('SavedSuccessfully'));
                    }
                });
        }
    }

    openSelectPlateCategoryModal() {
        // this.plateCategoryLookupTableModal.id = this.plate.plateCategoryId;
        this.plateCategoryLookupTableModal.displayName = this.plateCategoryName;
        this.plateCategoryLookupTableModal.show(this.isPlate);
    }


    setPlateCategoryIdNull() {
        this.plate.plateCategoryId = null;
        this.plateCategoryName = '';
    }


    getNewPlateCategoryId() {
        this.plate.plateCategoryId = this.plateCategoryLookupTableModal.id;
        this.plateCategoryName = this.plateCategoryLookupTableModal.displayName;
    }

    onFileChanged(event) {
        this.readThis(event.target);
    }

    readThis(inputValue: any): void {
        var file: File = inputValue.files[0];
        if (file.size < 1048576) {
            var myReader: FileReader = new FileReader();
            myReader.onloadend = (e) => {
                this.plate.imageUrl = myReader.result.toString();
            }
            myReader.readAsDataURL(file);
        }
        else {
            alert('File is too big!');
            inputValue.value = '';
        }
    }

    close(): void {
        this.files = []
        this.active = false;
        this.modal.hide();
    }

    //scan plate model
    public tmpPlateCode = '';

    registerEvents(): void {
        const self = this;
        function onMessageReceived(message) {
            alert(message.message)
            let mess = JSON.parse(message.message);

            if (mess.type = 'RFIDTable_DetectedDisc') {
                if (!self.scanPlateModal.isShown) return

                if (mess.data.Plates.length > 0) {
                    self.tmpPlateCode = mess.data.Plates[0].UType
                }
            }
        };

        abp.event.on('app.message.messageReceived', message => {
            self._zone.run(() => {
                onMessageReceived(message);
            });
        });
    }

    onScanPlateModelClick() {
        this.scanPlateModal.show()
    }

    onCloseScanPlateModel() {
        this.scanPlateModal.hide()
    }

    onUseScanPlateModel() {
        this.plate.code = this.tmpPlateCode
        this.scanPlateModal.hide()
    }
    
    onGeneratePlateCode()
    {
        abp.ui.setBusy();
        this._platesServiceProxy.generatePlateCode()
        .finally(() => {
            abp.ui.clearBusy();
        })
        .subscribe((result) => {
            if (result.code != null && result.code != undefined) {
                this.plate.code = result.code;
            }
            else {
                abp.ui.clearBusy();
                this.notify.info(this.l('Can not generate plate code'));
            }
        });
    }
    getRouterLink(plate_id){
        let routerUrl = 'app/main/plate/dishes/' + plate_id;
        this.router.navigate([routerUrl]);
    }
}