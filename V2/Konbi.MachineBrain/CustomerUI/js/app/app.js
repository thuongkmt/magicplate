if ('serviceWorker' in navigator) {
    navigator.serviceWorker
        .register('./service-worker.js')
        .then(function () {
            console.log('Service Worker Registered');
        });
}

$.fn.extend({
    animateCss: function (animationName, callback) {
        var animationEnd = (function (el) {
            var animations = {
                animation: 'animationend',
                OAnimation: 'oAnimationEnd',
                MozAnimation: 'mozAnimationEnd',
                WebkitAnimation: 'webkitAnimationEnd',
            };

            for (var t in animations) {
                if (el.style[t] !== undefined) {
                    return animations[t];
                }
            }
        })(document.createElement('div'));

        this.addClass('animated ' + animationName).one(animationEnd, function () {
            $(this).removeClass('animated ' + animationName);

            if (typeof callback === 'function') callback();
        });

        return this;
    },
});

/*------------------------------*/
const EMPTY = '';
const SWITCH_MODE_KEY = '*';
const CHANGE_MODE_KEYS = '***';
const CANCEL_PAYMENT_KEY = '.';
const CANCEL_PAYMENT_KEYS = '...';
const OVERRIDE_PAYMENT_SUCCESS_KEY = '0.0';
const ADD_KEY = '+';
const REMOVE_KEY = '-';
const EXECUTE_PAYMENT_KEY = 'Enter';
const CLEAR_ALL_KEY = '/';

const MAX_RECORD_LAST_KEYS = 100;

const PAYMENT_STATE_INIT = 100;
const PAYMENT_STATE_READYTOPAY = 101;
const PAYMENT_STATE_ACTIVATING_PAYMENT = 105;
const PAYMENT_STATE_ACTIVATED_PAYMENT_SUCCESS = 110;

const PAYMENT_STATE_ACTIVATED_PAYMENT_ERROR = 115;
const PAYMENT_STATE_CANCELLED = 120;
const PAYMENT_STATE_INPROGRESS = 200;
const PAYMENT_STATE_SUCCESS = 300;
const PAYMENT_STATE_FAILURE = 400;
const PAYMENT_STATE_REJECTED = 500;

var posMode = false;
var paymentState = 0;
var pressedKeys = '';
var lastPressedKey = '';
var lastPressedKeys = '';
var amountList = [];
var amountText = '';

/*------------------------------*/
$('document').ready(function () {
    $('#modal-guide').modal('show');

    $('#modal-guide').on('shown.bs.modal', function () {
        $('#modal-guide video').trigger('play');
    });

    $('#modal-guide').on('hidden.bs.modal', function () {
        $('#modal-guide video').trigger('pause');
    });

    $(window).bind('keypress', function (e) {
        handleKeyPress(e.key);
        console.log('handlekeypress: ' + e.key);
        console.log('keysequence: ' + pressedKeys);
    });

    $('#show-loading').hide();
    $('#select-mastercardvisa').hide();
    $('#select-ezlink-netsflashpay').hide();
    $('#print-receipt').hide();
    $('#change-payment-method').hide();
});

/*------------------------------*/

var handleKeyPress = (key) => {
    abp.closeGuideDialog();
    //initTransaction();
    lastPressedKeys += key;
    if (lastPressedKeys.length > MAX_RECORD_LAST_KEYS)
        lastPressedKeys = lastPressedKeys.slice(lastPressedKeys.length - MAX_RECORD_LAST_KEYS);
        console.log('last ' + MAX_RECORD_LAST_KEYS + ' Keys: ' + lastPressedKeys);

    ///author: nnthuong
    ///desciption: if user end with these keycode '0.0' our transaction will be Override payment. 
    ///             this will resolve the case customer pay cash
    ///date: 2021/01/27
    if (lastPressedKeys.endsWith(OVERRIDE_PAYMENT_SUCCESS_KEY)) {
        abp.signalr.hubs.common.invoke("overrideSuccessPayment").then((result) => {
            console.log('OVERRIDE PAYMENT: ' + result);
        });
    }
    if (key === SWITCH_MODE_KEY) {
        if (lastPressedKey == SWITCH_MODE_KEY || pressedKeys === EMPTY) {
            pressedKeys += key;
        } else {
            pressedKeys = EMPTY;
        }
    }
    ///author: nnthuong
    ///description: if user press the keycode is '.' then we will append to pressedKeys to handle cancel payment
    ///date: 2021/01/27
    ///link to process: PAYMENT_CANCEL
    if (key === CANCEL_PAYMENT_KEY && (paymentState != PAYMENT_STATE_INIT)) {
        if (lastPressedKey === CANCEL_PAYMENT_KEY || pressedKeys === EMPTY) {
            pressedKeys += key;
        } else {
            pressedKeys = EMPTY;
        }
    }

    if (key === ADD_KEY) {
        if (isInactiveUI()) return;
        if (amountText > 0) {
            addItem();
            amountText = '';
        }
    }

    if (key === EXECUTE_PAYMENT_KEY) {
        if (isInactiveUI()) return;
        if (_.isEmpty(amountList)) return;

        var paymentItems = _.map(amountList, (amt, idx) => {
            return {
                itemName: `Item #${idx + 1}`,
                itemAmount: amt
            }
        });
        abp.signalr.hubs.common.invoke("executeManualTransaction", paymentItems).then(() => {});
    }

    if (key === REMOVE_KEY) {
        if (isInactiveUI()) return;
        removeItem();
    }

    if (key === CLEAR_ALL_KEY) {
        if (isInactiveUI()) return;
        clearAll();
    }

    if (isNumberKey(key)) {
        console.log("isNumberKey: " + key);
        if (amountText === CANCEL_PAYMENT_KEYS) {
            console.log("amountText_CANCEL_PAYMENT_KEYS: " + amountText);
            amountText = EMPTY;
        }

        if (amountText.length < 5) {
            if (paymentState === PAYMENT_STATE_INPROGRESS || paymentState === PAYMENT_STATE_ACTIVATED_PAYMENT_ERROR) {
                if (key === CANCEL_PAYMENT_KEY) {
                    amountText += key;
                } else {
                    amountText = EMPTY;
                }
                console.log("key_CANCEL_PAYMENT_KEYS: " + key);
            } else {
                amountText += key;
            }

            if (!_.isEmpty(amountText)) {
                showInputAmount();
            }
        }
    }

    if (pressedKeys === CHANGE_MODE_KEYS) {
        if (isInactiveUI()) return;
        abp.signalr.hubs.common.invoke("changePosModeStatus").then(() => {});
    }

    //
    ///author: nnthuong
    ///description: touchless buttons
    var convertLowerKey = lastPressedKeys.toUpperCase();
    console.log("convertLowerKey: " + convertLowerKey);
    if(convertLowerKey.endsWith("EZLINK")){
    abp.signalr.hubs.common.invoke("onClickPay", "iuC_CEPAS").then(function (
        result) {
            console.log("result: " + result);
        });

    }
    if(convertLowerKey.endsWith("VISA")){
        abp.signalr.hubs.common.invoke("onClickPay", "iuC_CONTACTLESS").then(function (
            result) {
                console.log("result: " + result);
            });
    }
    if(convertLowerKey.endsWith("KONBI_CREDIT")){
        abp.signalr.hubs.common.invoke("onClickPay", "konbI_CREDITS").then(function (
            result) {
                console.log("result: " + result);
            });
    }
    if(convertLowerKey.endsWith("FACIAL_RECOG")){
        abp.signalr.hubs.common.invoke("onClickPay", "faciaL_RECOGNITION").then(function (
            result) {
                console.log("result: " + result);
            });
    }


    ///author: nnthuong
    ///description: if pressedKeys value is consecutive with '...'  then we do cancel payment
    ///date: 2021/01/27
    ///remark: PAYMENT_CANCEL
    if (pressedKeys === CANCEL_PAYMENT_KEYS) {
        console.log("CANCEL_PAYMENT_KEYS: " + pressedKeys);
        pressedKeys = EMPTY;

        closeInputAmount();
        abp.signalr.hubs.common.invoke("cancelTransaction").then(() => {});

    }

    lastPressedKey = key;
}

var addItem = () => {
    closeInputAmount();
    if (amountList.length > 29) return; // maximum is 30 items
    amountList = _.concat(amountList, amountText / 100);
    bindData();
}

var removeItem = () => {
    closeInputAmount();
    amountList = _.dropRight(amountList);
    amountText = EMPTY;
    bindData();
}

var clearAll = () => {
    closeInputAmount();
    makeEmpty();
    bindData();
}

var makeEmpty = () => {
    pressedKeys = '';
    lastPressedKey = '';
    amountText = '';
    amountList = [];
}

var handlePaymentStateChange = (state) => {
    var messageContainer = $('#btn-message-action-pay');
    if (state === PAYMENT_STATE_INIT) {
        makeEmpty();
        messageContainer.removeClass('btn-danger btn-success').addClass('btn-warning');
    } else if (state == PAYMENT_STATE_READYTOPAY || state == PAYMENT_STATE_ACTIVATING_PAYMENT || state == PAYMENT_STATE_ACTIVATED_PAYMENT_SUCCESS || state == PAYMENT_STATE_INPROGRESS) {
        messageContainer.removeClass('btn-danger btn-warning').addClass('btn-success');
    } else if (state == PAYMENT_STATE_CANCELLED || state == PAYMENT_STATE_FAILURE || state == PAYMENT_STATE_REJECTED || state == PAYMENT_STATE_ACTIVATED_PAYMENT_ERROR) {
        messageContainer.removeClass('btn-success btn-warning').addClass('btn-danger');
    }

}

var handlePOSModeChange = (isOn) => {
    console.log("handlePOSModeChange");

    posMode = isOn;
    $('#text-payment-mode').text(isOn ? 'POS Mode' : '');

    if (isInactiveUI()) return;
    makeEmpty();
    //abp.signalr.hubs.common.invoke("resetTransaction").then(() => {});
}

var bindData = () => {
    console.log('bindData', amountList);
    var hasValues = amountList.length > 0;
    var totalAmount = hasValues ? _.reduce(amountList, (sum, n) => {
        return sum + n;
    }) : 0;

    //var html = hasValues > 0 ? _.map(amountList, (amt, idx) => {
    //    return `
    //            <div class="row row-border-top vertical-center order-line">
    //            <div class="col-1">${idx + 1}</div>
    //            <div class="col-7 text-truncate">Item #${idx + 1}</div>
    //            <div class="col-4 text-truncate" style="text-align: right">$${amt.toFixed(2)}</div>
    //        </div>`;
    //}) : '';

    if (hasValues > 0) {
        _.map(amountList, (amt, idx) => {
            var itemOrder = `<li class="order-line"><a href="#">Item #${idx + 1}</a></li>`;
            var itemOrderPrice = `<li class="order-line"><a href="#">$${amt.toFixed(2)}</a></li>`;

            $('#rounded-list').append(itemOrder);
            $('#rounded-list-price').append(itemOrderPrice);
        });
    }

    //$('#plate-count').text(hasValues ? amountList.length : '0');
    //$('#plate-count').animateCss('bounce', function () {
    //    $('#plate-count').removeClass('bounce');
    //});
    $('#price').text(`$${hasValues ? totalAmount.toFixed(2) : '0.00'}`);

    $('.order-line').remove();
    //$('#list-order-content').append(html);

    if (amountList.length > 9) {
        autoScroll();
    }
}

var isInactiveUI = () => {
    console.log("isInactiveUI");
    return paymentState === PAYMENT_STATE_INPROGRESS ||
        paymentState === PAYMENT_STATE_ACTIVATED_PAYMENT_ERROR;
}

var canReset = () => {
    console.log("canReset");
    return paymentState === PAYMENT_STATE_CANCELLED ||
        paymentState === PAYMENT_STATE_SUCCESS ||
        paymentState === PAYMENT_STATE_REJECTED ||
        paymentState === PAYMENT_STATE_FAILURE;
}

var initTransaction = () => {
    console.log("initTransaction");
    if (canReset()) {
        abp.signalr.hubs.common.invoke("resetTransaction").then(() => {});
        amountList = [];
        pressedKeys = '';
        lastPressedKey = '';
        amountText = '';
    }
}

var autoScroll = () => {
    window.setInterval(function () {
        var elem = document.getElementById('list-order-content');
        elem.scrollTop = elem.scrollHeight;
    }, 5000);
}

var showInputAmount = () => {
    console.log("showInputAmount");
    
    if (canReset()) return;
    var isInprogress = paymentState === PAYMENT_STATE_INPROGRESS || paymentState === PAYMENT_STATE_ACTIVATED_PAYMENT_ERROR;
    var amtText = isInprogress ? amountText : `$${(amountText / 100).toFixed(2)}`;

    $('#modal-input-highlight').modal('show');
    $('#highlight-amount-text').text(amtText);
}

var closeInputAmount = () => {
    console.log("closeInputAmount");

    $('#highlight-amount-text').text(`$0.00`);
    $('#modal-input-highlight').modal('hide');
}

var isNumberKey = (key) => {
    return !_.isNaN(_.toNumber(key));
}

/**
 * Change background
 */
var background_choose_payment_method = function(){
    $("#rounded-list").addClass("bg-info");
    $(".column-summary").addClass("bg-info");
    $(".price-zone").addClass("bg-info");
} 

var background_paying = function(){
    $("#rounded-list").addClass("bg-warning");
    $(".column-summary").addClass("bg-warning");
    $(".price-zone").addClass("bg-warning");
} 

var background_success = function(){
    $("#rounded-list").addClass("bg-success");
    $(".column-summary").addClass("bg-success");
    $(".price-zone").addClass("bg-success");
} 

var background_failed = function(){
    $("#rounded-list").addClass("bg-failed");
    $(".column-summary").addClass("bg-failed");
    $(".price-zone").addClass("bg-failed");
} 

var background_clear = function(){
    $("#rounded-list").removeClass("bg-success");
    $(".column-summary").removeClass("bg-success");
    $(".price-zone").removeClass("bg-success");

    $("#rounded-list").removeClass("bg-failed");
    $(".column-summary").removeClass("bg-failed");
    $(".price-zone").removeClass("bg-failed");

    $("#rounded-list").removeClass("bg-warning");
    $(".column-summary").removeClass("bg-warning");
    $(".price-zone").removeClass("bg-warning");

    $("#rounded-list").removeClass("bg-info");
    $(".column-summary").removeClass("bg-info");
    $(".price-zone").removeClass("bg-info");

    $("#message-processing").removeClass("message-failed");
}