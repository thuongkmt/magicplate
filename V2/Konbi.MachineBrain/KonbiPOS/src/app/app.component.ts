import { Component, OnInit, TemplateRef, ContentChild, ViewChild, HostListener } from '@angular/core';
import { BsModalService, BsModalRef } from 'ngx-bootstrap/modal';
import { NgxSpinnerService } from 'ngx-spinner';
import { Observable, Subject, EMPTY, throwError } from 'rxjs';
import {
  HubConnectionState, JsonHubProtocol
} from '@aspnet/signalr';
import * as _ from 'lodash';
import * as r from 'ramda';
import * as moment from 'moment';

import {
  ProductMenusServiceProxy,
  PagedResultDtoOfGetCategoryForView,
  PagedResultDtoOfGetSessionForView,
  ListResultDtoOfPosProductMenuOutput,
  PosProductMenuOutput,
  GetSessionForView,
  SessionDto,
  CategoryDto,
  CategoryServiceProxy,
  SessionsServiceProxy
} from '../core/api-services/api-service-proxies';
import { SignalRService, ManualPaymentInput } from '../core/signalr/signalr-service';
import { PaymentState } from '../shared/app-enums';
import { MSG_PLACE_TRAY } from '../shared/app-consts';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss', './components/virtual-keyboard/virtual-keyboard.component.scss']
})
export class AppComponent implements OnInit {
  cashPaymentConfirmationModal: BsModalRef;
  cashPaymentAlertModal: BsModalRef;
  currentCountDown: NodeJS.Timer;
  barcodeReaderTimer: NodeJS.Timer;
  countDownValue = 0;
  paymentMessageClass = 'payment-state-warning';
  totalItems = 0;
  totalPrice = 0;
  discount = 0;
  alertMessage = '';
  orderItemList: any[] = [];
  selectedOrderItem: any = {};
  currentSession: SessionDto = new SessionDto();
  sessionName = '';
  sessionFromTime: moment.Moment | undefined;
  sessionToTime: moment.Moment | undefined;
  productList: PosProductMenuOutput[] = [];
  currentProd: PosProductMenuOutput = new PosProductMenuOutput();
  categoryList: CategoryDto[] = [];
  currentCate: CategoryDto = new CategoryDto();
  paymentSate = 0;
  connectionState: HubConnectionState = 0;
  barcodeValue = '';
  isCancelSalesInprogress = false;
  isCashlessPaymentInprogress = false;
  isAddOrderItemInprogress = false;
  productMenuList = {};
  isAllowFacialRecog = false;

  @ViewChild('cashPaymentConfirmation')
  private cashPaymentConfirmation: TemplateRef<any>;

  @ViewChild('cashPaymentAlert')
  private cashPaymentAlert: TemplateRef<any>;
  paymentModes = [];
  orderInfo: any;
  constructor(
    // tslint:disable-next-line: variable-name
    private _cateApiService: CategoryServiceProxy,
    // tslint:disable-next-line: variable-name
    private _prodMenuService: ProductMenusServiceProxy,
    // tslint:disable-next-line: variable-name
    private _sessionService: SessionsServiceProxy,
    // tslint:disable-next-line: variable-name
    private _signalRService: SignalRService,
    private modalService: BsModalService,
    private spinnerService: NgxSpinnerService

  ) { }

  ngOnInit(): void {
    this.initialize();
  }

  initialize = () => {
    this._signalRService.initialize();
    this._signalRService.startConnection((state) => this.loadData(state));
    this._signalRService.reconnect((state) => this.loadData(state));
    this._signalRService.listenTransactionChanges(this.bindOrderList);
    this._signalRService.listenPaymentModes(this.updatePaymentModes);
    this._signalRService.listenSessionInfoChanged(this.updateSessionInfo);
    this._signalRService.listenCountDownChanges(({ countTime, isOff }) => this.runCountDown(countTime, isOff));
    this._signalRService.listenCashPaymentConfirm(() => {
      this.cashPaymentAlertModal = this.modalService.show(this.cashPaymentAlert, {
        backdrop: 'static',
        class: 'modal-dialog-centered modal-lg',
      });
    });
    this.listenBackendChanges();
  }

  listenBackendChanges = () => {
    this._signalRService.listenProductChanges(({ type }) => {
      if (!_.isEmpty(type)) {
        switch (_.toUpper(type)) {
          case 'SESSION':
            this.loadCurrentSession(() => this.loadCateList());
            break;
          case 'CATEGORY':
            this.loadCateList();
            break;
          case 'PRODUCT':
            this.loadMenuProductList(this.currentCate);
            break;
        }
      }
    });
  }

  loadData = (connState: HubConnectionState) => {
    this.connectionState = connState;
    if (connState === 1) {
      this.loadCurrentSession(() => this.loadCateList());
    } else {
      this.currentSession = new SessionDto();
      this.sessionName = '';
      this.sessionFromTime = null;
      this.sessionToTime = null;
      this.categoryList = [];
    }
  }

  updatePaymentModes = (modes) => {
    
    // tslint:disable-next-line: forin
    for (const key in modes) {
      if (modes[key].toLowerCase() === 'ezlink') {
        modes[key] = 'EZLink <br/> Flashpay';
      }
      if (modes[key].toLowerCase() === 'contactless') {
        modes[key] = 'Visa <br/> Mastercard';
      }
      if(modes[key].toLowerCase() === 'facial_recognition'){
        delete modes[key];
        this.isAllowFacialRecog = true;
      }
    }
    console.log("modes: " + JSON.stringify(modes));
    this.paymentModes = modes;
  }

  bindOrderList = (orderInfo) => {
    console.log('order info');
    console.log(orderInfo);
    this.orderInfo = orderInfo;
    if (_.isEmpty(orderInfo)) {
      this.alertMessage = MSG_PLACE_TRAY;
      this.orderItemList = [];
    } else {
      const {
        customerMessage: msg,
        menuItems: orderItemList,
        amount: totalPrice,
        paymentState: state,
      } = orderInfo;
      this.alertMessage = _.isEmpty(msg) ? MSG_PLACE_TRAY : msg;
      this.paymentSate = state;

      this.applyMessageStyleWithState(state);
      if (_.isEmpty(msg) || state === PaymentState.Success) {
        this.runCountDown(0, true);
        this.selectedOrderItem = {};
      }

      if (state === PaymentState.Init) {
        if (this.cashPaymentConfirmationModal) {
          this.cashPaymentConfirmationModal.hide();
        }

        if (this.cashPaymentAlertModal) {
          this.cashPaymentAlertModal.hide();
        }
      }

      if (_.isEmpty(orderItemList)) {
        this.orderItemList = [];
        this.totalPrice = 0;
        this.totalItems = 0;
        this.discount = 0;
      } else {
        this.orderItemList = _.map(orderItemList, ({
          productId,
          productName,
          price,
          code,
          uId
        }) => {
          return {
            prodId: productId,
            name: productName,
            price,
            code,
            uId
          };
        });
        if (orderInfo.discount > 0)
          this.discount = orderInfo.discount * 100;
        this.totalPrice = totalPrice;
        this.totalItems = orderItemList.length;

      }
    }
  }

  loadCurrentSession = (callback) => {
    this._sessionService.getAll(
      '',
      '',
      '',
      '',
      'true',
      '',
      0,
      100
    ).subscribe((result: PagedResultDtoOfGetSessionForView) => {
      if (_.isEmpty(result) || (!_.isEmpty(result) && _.isEmpty(result.items))) {
        this.currentSession = new SessionDto();
        this.sessionName = '';
        this.sessionFromTime = null;
        this.sessionToTime = null;
        callback();
        return;
      }

      const allSessions = result.items;
      const currentTime = moment(new Date());
      const currentSessions = _.filter(allSessions, (session: GetSessionForView) => {
        const fromHrs = session.session.fromHrs;
        const toHrs = session.session.toHrs;

        const fromTime = moment({ hour: Number(r.take(2, fromHrs)), minute: Number(r.takeLast(2, fromHrs)) });
        const toTime = moment({ hour: Number(r.take(2, toHrs)), minute: Number(r.takeLast(2, toHrs)) });
        return currentTime.isAfter(fromTime) && currentTime.isBefore(toTime);
      });

      if (!_.isEmpty(currentSessions)) {
        this.currentSession = currentSessions[0].session;
        this.sessionName = this.currentSession.name;
        this.sessionFromTime = moment({ hour: Number(r.take(2, this.currentSession.fromHrs)), minute: Number(r.takeLast(2, this.currentSession.fromHrs)) });
        this.sessionToTime = moment({ hour: Number(r.take(2, this.currentSession.toHrs)), minute: Number(r.takeLast(2, this.currentSession.toHrs)) });
        callback();
      } else {
        this.currentSession = new SessionDto();
        this.sessionName = '';
        this.sessionFromTime = null;
        this.sessionToTime = null;
      }
    });
  }
  updateSessionInfo = (sessionInfo) => {
    console.log('backend pushes session info');
    console.log(sessionInfo);
    if (sessionInfo) {
      this.currentSession = sessionInfo;
      this.sessionName = this.currentSession.name;
      this.sessionFromTime = moment({ hour: Number(r.take(2, this.currentSession.fromHrs)), minute: Number(r.takeLast(2, this.currentSession.fromHrs)) });
      this.sessionToTime = moment({ hour: Number(r.take(2, this.currentSession.toHrs)), minute: Number(r.takeLast(2, this.currentSession.toHrs)) });

    } else {
      this.currentSession = new SessionDto();
      this.sessionName = '';
      this.sessionFromTime = null;
      this.sessionToTime = null;
    }
  }

  loadCateList = () => {
    this._cateApiService.getAll('', '', '', '', 0, 25)
      .subscribe((result: PagedResultDtoOfGetCategoryForView) => {
        if (!_.isEmpty(result) && _.isArray(result.items)) {
          if (!_.isEmpty(result.items)) {
            this.categoryList = _.map(result.items, (item) => item.category);

            // Load product menu for cache.
            // tslint:disable-next-line: prefer-for-of
            for (let index = 0; index < this.categoryList.length; index++) {
              this.loadMenuProductListForCache(this.categoryList[index]);
            }

            if (_.isEmpty(this.currentCate)) {
              this.currentCate = this.categoryList[0];
            }
            this.changeCate(this.currentCate);
          }
        }
      });
  }

  changeCate = (cate: CategoryDto) => {
    this.currentCate = cate;
    if (this.productMenuList[cate.id + this.currentSession.id]) {
      this.productList = this.productMenuList[cate.id + this.currentSession.id];
      this.loadMenuProductListForCache(cate);
    } else {
      this.loadMenuProductList(cate);
    }
  }

  loadMenuProductListForCache(cate: CategoryDto) {
    this._prodMenuService.getPOSMenu(cate.id, this.currentSession.id)
      .subscribe((result: ListResultDtoOfPosProductMenuOutput) => {
        this.productMenuList[cate.id + this.currentSession.id] = [];
        this.productMenuList[cate.id + this.currentSession.id] = result.items;
      });
  }

  loadMenuProductList = (cate: CategoryDto) => {
    this.spinnerService.show();
    this._prodMenuService.getPOSMenu(cate.id, this.currentSession.id)
      .pipe(finalize(() => {
        this.spinnerService.hide();
      }))
      .subscribe((result: ListResultDtoOfPosProductMenuOutput) => {
        this.productList = result.items;
      });
  }

  applyMessageStyleWithState = (state: number) => {
    switch (state) {
      case PaymentState.Success:
      case PaymentState.InProgress:
      case PaymentState.ReadyToPay:
      case PaymentState.ActivatedPaymentSuccess:
        this.paymentMessageClass = 'payment-state-success';
        break;
      case PaymentState.Failure:
      case PaymentState.Rejected:
      case PaymentState.ActivatedPaymentError:
      case PaymentState.Cancelled:
        this.paymentMessageClass = 'payment-state-danger';
        break;
      default:
        this.paymentMessageClass = 'payment-state-warning';
        break;
    }
  }

  addOrderItem = (prod: PosProductMenuOutput, e) => {
    // un-focus on selected item to fix issue  that duplicate item while scanning barcode
    if (e.target.tagName == 'A')
      e.target.blur();
    else {
      var el = e.target.parentElement;
      var loopCount = 0;
      while (el.tagName != 'A' && loopCount < 10) {
        loopCount++;
        el = el.parentElement;
        if (!el)
          break;
      }
      if (el)
        el.blur();
    }

    if (this.paymentSate === PaymentState.InProgress) { return; }
    console.log('addOrderItem');

    this.currentProd = prod;
    const orderItem = new ManualPaymentInput();
    orderItem.init({
      plateCode: prod.plateCode,
      productId: prod.productId,
      productName: prod.productName,
      price: prod.price
    });

    if (this.isAddOrderItemInprogress) { return; }
    this.isAddOrderItemInprogress = true;
    this._signalRService.addOrderItems(orderItem, () => { this.isAddOrderItemInprogress = false; });
  }
  addManuallyOrderItem = (price: number) => {
    if (this.paymentSate === PaymentState.InProgress) { return; }
    if (price <= 0) { return; }

    const orderItem = new ManualPaymentInput();
    orderItem.init({
      plateCode: '',
      productId: '',
      productName: '',
      price
    });

    if (this.isAddOrderItemInprogress) { return; }
    this.isAddOrderItemInprogress = true;
    this._signalRService.addOrderItems(orderItem, () => { this.isAddOrderItemInprogress = false; });
  }
  cancelSales = () => {
    if (this.isCancelSalesInprogress) { return; }
    this.isCancelSalesInprogress = true;

    this._signalRService.cancelSales(() => {
      this.isCancelSalesInprogress = false;
      if (this.cashPaymentConfirmationModal) {
        this.cashPaymentConfirmationModal.hide();
      }
      if (this.cashPaymentAlertModal) {
        this.cashPaymentAlertModal.hide();
      }
    });
  }

  cashPaymentConfirm = () => {
    this._signalRService.resetTransaction();
    this.cashPaymentAlertModal.hide();
  }

  openCashPaymentConfirmationModal = () => {
    this.cashPaymentConfirmationModal = this.modalService.show(this.cashPaymentConfirmation, {
      class: 'modal-dialog-centered modal-lg',
      backdrop: 'static'
    });
  }

  cancelCashPayment = () => {
    this.cashPaymentConfirmationModal.hide();
  }

  executeCashlessPayment = (mode) => {
    console.log("mode: " + mode);
    if (this.isCashlessPaymentInprogress) { return; }
    this.isCashlessPaymentInprogress = true;
    this._signalRService.executeCashlessPayment(mode, () => { this.isCashlessPaymentInprogress = false; });
  }

  executeCashPayment = () => {
    this._signalRService.executeCashPayment(() => { });
    this.cashPaymentConfirmationModal.hide();
  }

  selectOrderItem = (orderItem) => {
    if (this.paymentSate === 200) { return; }
    if (_.isEmpty(orderItem.uId) || (!_.isEmpty(orderItem.uId) && orderItem.uId === '00000000-0000-0000-0000-000000000000')) { return; }
    this.selectedOrderItem = orderItem;
  }

  removeOrderItem = () => {
    this._signalRService.removeOrderItem(this.selectedOrderItem.uId, () => this.selectedOrderItem = {});
  }

  handleVirtualKeyboardInput = (input) => {
    if (!_.isEmpty(input)) {
      this.addManuallyOrderItem(_.toNumber(input));
    }
  }

  isCancelSalesDisabled = () => {
    let result = false;

    if (this.paymentSate !== PaymentState.ActivatedPaymentSuccess) {
      result = true;
    }

    if (this.totalItems > 0) {
      result = false;
    }

    if (this.paymentSate === PaymentState.Success) {
      result = true;
    }

    return result;
  }
  // (this.paymentSate !== PaymentState.ActivatedPaymentSuccess || this.selectedOrderItem.length > 0);

  isVoidItemDisabled = () => (this.isDisabled && _.isEmpty(this.selectedOrderItem));

  isDisabled = () => {
    return _.isEmpty(this.orderItemList) || (this.paymentSate === PaymentState.InProgress || this.paymentSate === PaymentState.ActivatedPaymentSuccess);
  }

  isFacialRecog() {
    var check = true;
      if(this.isAllowFacialRecog){
        //display on the iu
        check = false;
      }
      console.log("check: " + check);
      return check;
  }

  isOrdered = (prod: PosProductMenuOutput) => {
    return !_.isEmpty(_.filter(this.orderItemList, ({ prodId }) => prodId === prod.productId));
  }

  runCountDown = (countTime, isOff) => {
    if (isOff) {
      clearInterval(this.currentCountDown);
      this.countDownValue = 0;
    } else {
      clearInterval(this.currentCountDown);
      this.currentCountDown = setInterval(() => {
        this.countDownValue = countTime;
        if (countTime < 0) {
          clearInterval(this.currentCountDown);
          this.countDownValue = 0;
          this.selectedOrderItem = {};
          this.cancelSales();
        }
        countTime--;
      }, 1000);
    }
  }

  // TODO(quyen): this is backup solution to listen barcode scanner, will be implemented if barcode reader listener
  // got problem....
  @HostListener('window:keypress', ['$event'])
  keyEvent(event: KeyboardEvent) {
    this.barcodeReaderTimer = setInterval(() => {
      this.barcodeValue = '';
      clearInterval(this.barcodeReaderTimer);
    }, 300);

    // tslint:disable-next-line: deprecation
    if (event.keyCode === KEY_CODE.ENTER) {
      if (_.isEmpty(this.barcodeValue)) { return; }
      this._signalRService.executeBarcodeScanningTransaction(this.barcodeValue);
      this.barcodeValue = '';
    }
    this.barcodeValue += event.key;
  }
}

enum KEY_CODE {
  ENTER = 13
}
