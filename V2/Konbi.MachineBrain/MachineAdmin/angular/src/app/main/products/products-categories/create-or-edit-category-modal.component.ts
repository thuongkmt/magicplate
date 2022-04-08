import { Component, ViewChild, Injector, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { CategoryServiceProxy, CreateOrEditCategoryDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';


@Component({
    selector: 'createOrEditCategoryModal',
    templateUrl: './create-or-edit-category-modal.component.html'
})
export class CreateOrEditCategoryModalComponent extends AppComponentBase {

    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;

    category: CreateOrEditCategoryDto = new CreateOrEditCategoryDto();

    constructor(
        injector: Injector,
        private _categoriesServiceProxy: CategoryServiceProxy
    ) {
        super(injector);
    }

    show(categoryId?: string): void {
        if (!categoryId) {
            this.category = new CreateOrEditCategoryDto();
            this.active = true;
            this.modal.show();
        } else {
            this._categoriesServiceProxy.getCategoryForEdit(categoryId).subscribe(result => {
                this.category = result.category;
                this.active = true;
                this.modal.show();
            });
        }
    }

    save(): void {
        this.saving = true;
        this._categoriesServiceProxy.createOrEdit(this.category)
            .pipe(finalize(() => { this.saving = false; }))
            .subscribe(() => {
                this.notify.info(this.l('SavedSuccessfully'));
                this.close();
                this.modalSave.emit(null);
            });
    }

    close(): void {
        this.active = false;
        this.modal.hide();
    }
}
