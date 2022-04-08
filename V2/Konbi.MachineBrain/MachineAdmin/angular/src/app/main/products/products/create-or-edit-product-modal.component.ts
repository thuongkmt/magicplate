import { Component, ViewChild, Injector, Output, EventEmitter, NgZone, OnDestroy } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { ProductServiceProxy, CreateOrEditProductDto, GetProductForEditOutput } from '@shared/service-proxies/service-proxies';
import { ProductImageServiceProxy } from '@shared/service-proxies/productImage-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';
import { Router } from '@angular/router';
import { ParentChildService } from '../../../shared/common/parent-child/parent-child.service';
import { CategoryLookupTableModalComponent } from './category-lookup-table-modal.component';
import { PlateServiceProxy, PlateDto } from '@shared/service-proxies/plate-service-proxies';
import { ProductSignalrService } from './product-signalr-service';
import { SignalRHelper } from 'shared/helpers/SignalRHelper';

@Component({
    selector: 'createOrEditProductModal',
    templateUrl: './create-or-edit-product-modal.component.html',
    styleUrls: ['./create-or-edit-product-modal.component.css']
})
export class CreateOrEditProductModalComponent extends AppComponentBase {
    // ngOnDestroy(): void {
    //     this._prodSignalrService.changeProductUpdateStatus();
    // }

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();
    @ViewChild('categoryLookupTableModal') categoryLookupTableModal: CategoryLookupTableModalComponent;

    active = false;
    saving = false;

    product: CreateOrEditProductDto = new CreateOrEditProductDto();
    categoryName = '';
    defaultProductImage = 'assets/common/images/ic_nophoto.jpg';

    //upload photo
    files: File[] = [];
    lastInvalids: any;
    lastFileAt: Date;
    plateModels: PlateDto[] = [];
    allowEditingSKU = false;
    barcodeValue: string = '';

    getDate() {
        if (this.files.length > 3) {
            alert('Please select maximun 3 images');
            this.files = [];
        }
        return new Date();
    }


    constructor(
        injector: Injector,
        private _platesServiceProxy: PlateServiceProxy,
        private _productsServiceProxy: ProductServiceProxy,
        private _prodSignalrService: ProductSignalrService,
        private _productImageServiceProxy: ProductImageServiceProxy,
        private router: Router,
        public _zone: NgZone,
        private comParentChildService: ParentChildService
    ) {
        super(injector);
    }

    // tslint:disable-next-line:use-life-cycle-interface
    ngOnInit(): void {
        // this.getPlateModels();
        this.registerEvents();
        if (this.appSession.application) {
            SignalRHelper.initSignalR(() => { this._prodSignalrService.init(); });
        }
    }

    registerEvents(): void {
        abp.event.on('app.product.barcode.messageReceived', ({ barcode }) => {
            this.product.barcode = barcode;
        });
    }

    // handleBarcodeFocus = () => {
    //     this._prodSignalrService.changeProductUpdateStatus(true);
    // }

    // handleBlurBarcode = () => {
    //     this._prodSignalrService.changeProductUpdateStatus(false);
    // }

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

    // updateChoosePlate(event: any) {
    //     this.choosePlateId = event.target.value;
    // }
    autoGenerateSKUChanged(event: any) {
        console.log(this.product);
        if (this.product.autoGenerateSKU && this.product.id == null) {
            this.product.sku = "";
        }
    }
    show(productId?: string): void {
        if (!productId) {
            this.product = new CreateOrEditProductDto();
            this.product.imageUrl = this.defaultProductImage;
            this.categoryName = '';
            this.active = true;
            //this.choosePlateId = null;
            // this.registerEvents()
            this.product.autoGenerateSKU = true;
            this.allowEditingSKU = true;
            this.modal.show();
        } else {
            this._productsServiceProxy.getProductForEdit(productId).subscribe(result => {
                this.product = result.product;
                if (!this.product.imageUrl) {
                    this.product.imageUrl = this.defaultProductImage;
                }
                if (this.product.id && this.product.sku) {
                    this.allowEditingSKU = false;
                } else {
                    this.allowEditingSKU = true;
                }
                this.categoryName = result.categoryName;
                this.active = true;
                this.modal.show();
            });
        }
    }


    save(): void {
        if (this.product.name == null || this.product.name == undefined || this.product.name.trim().length < 1) {
            abp.message.error('Invalid Product Name');
            return;
        }
        //Validate price
        if (this.product.price == null || this.product.price < 0) {
            abp.message.error("Invalid Price");
            return;
        }

        this.primengTableHelper.showLoadingIndicator();
        this.saving = true;
        //upload images first
        let productTemp = new GetProductForEditOutput();
        if (this.files.length > 0) {
            this._productImageServiceProxy.uploadFile(this.files)
                .subscribe(
                    response => {
                        if (response.result.length > 0) {
                            let arrImage = response.result;
                            let images = '';
                            for (let index = 0; index < arrImage.length; index++) {
                                (index === arrImage.length - 1) ? (images += arrImage[index]) : images += arrImage[index] + '|';
                            }
                            this.product.imageUrl = images;

                            productTemp.product = this.product;


                            this._productsServiceProxy.createOrEdit(productTemp)
                                .pipe(finalize(() => {
                                    this.saving = false;
                                }))
                                .subscribe((result) => {
                                    if (result.message != null) {
                                        abp.message.error(result.message);
                                    } else {

                                        this.files = [];
                                        this.close();
                                        this.modalSave.emit(null);
                                        this.notify.info(this.l('SavedSuccessfully'));
                                    }
                                });
                        }
                    }, err => {
                        console.log(err);
                    }
                );
        } else {
            productTemp.product = this.product;

            this._productsServiceProxy.createOrEdit(productTemp)
                .pipe(finalize(() => {
                    this.saving = false;
                }))
                .subscribe((result) => {
                    if (result.message != null) {
                        abp.message.error(result.message);
                    } else {

                        this.files = [];
                        this.close();
                        this.modalSave.emit(null);
                        this.notify.info(this.l('SavedSuccessfully'));
                    }
                });
        }
    }

    openSelectCategoryModal() {
        this.categoryLookupTableModal.id = this.product.categoryId;
        this.categoryLookupTableModal.displayName = this.categoryName;
        this.categoryLookupTableModal.show();
    }

    setCategoryIdNull() {
        this.product.categoryId = null;
        this.categoryName = '';
    }

    getNewCategoryId() {
        this.product.categoryId = this.categoryLookupTableModal.id;
        this.categoryName = this.categoryLookupTableModal.displayName;
    }

    onFileChanged(event) {
        this.readThis(event.target);
    }

    readThis(inputValue: any): void {
        let file: File = inputValue.files[0];
        if (file.size < 1048576) {
            let myReader: FileReader = new FileReader();
            myReader.onloadend = (e) => {
                this.product.imageUrl = myReader.result.toString();
            };
            myReader.readAsDataURL(file);
        } else {
            alert('File is too big!');
            inputValue.value = '';
        }
    }

    close(): void {
        this.files = [];
        this.active = false;
        this.modal.hide();
    }
}
