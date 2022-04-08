import { Component, ViewChild, Injector, Output, EventEmitter} from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { finalize } from 'rxjs/operators';
import { SessionsServiceProxy, CreateOrEditSessionDto } from '@shared/service-proxies/service-proxies';
import { AppComponentBase } from '@shared/common/app-component-base';


@Component({
    selector: 'createOrEditSessionModal',
    templateUrl: './create-or-edit-session-modal.component.html'
})
export class CreateOrEditSessionModalComponent extends AppComponentBase {

    @ViewChild('Session_FromHrs') session_FromHrs;
    @ViewChild('Session_ToHrs') session_ToHrs;
    @ViewChild('createOrEditModal') modal: ModalDirective;
    @Output() modalSave: EventEmitter<any> = new EventEmitter<any>();

    active = false;
    saving = false;
    showError = false;
    errorMessage = "";
    session: CreateOrEditSessionDto = new CreateOrEditSessionDto();

    constructor(
        injector: Injector,
        private _sessionsServiceProxy: SessionsServiceProxy
    ) {
        super(injector);
    }

    show(sessionId?: string): void {
        if (!sessionId) {
            this.session = new CreateOrEditSessionDto();
            this.session.id = sessionId;
            let now = new Date();
            let hours = now.getHours().toString() + ':' + now.getMinutes();
            this.session.fromHrs = hours;
            this.session.toHrs = hours;
            this.active = true;
            this.modal.show();
        } else {
            this._sessionsServiceProxy.getSessionForEdit(sessionId).subscribe(result => {
                this.session = result.session;

                this.active = true;
                this.modal.show();
            });
        }
    }

    // After choose time.
    clickChooseTime() {
        this.session_FromHrs.overlayVisible = false;
        this.session_ToHrs.overlayVisible = false;
    }

    save(): void {
        var fromHrs = this.session.fromHrs.split(":");
        var fromHrsTime = new Date();
        fromHrsTime.setHours(parseInt(fromHrs[0]));

        var toHrs = this.session.toHrs.split(":");
        var toHrsTime = new Date();
        toHrsTime.setHours(parseInt(toHrs[0]));
        
        if (toHrsTime < fromHrsTime) {
            this.showError = true;
            this.errorMessage = "From hrs cannot be earlier than To hrs.";
            return;
        }

        this.saving = true;
        this._sessionsServiceProxy.createOrEdit(this.session)
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
