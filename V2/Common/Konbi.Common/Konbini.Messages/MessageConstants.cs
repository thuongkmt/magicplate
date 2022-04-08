using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages
{
    public class MessageConstants
    {
        public const string SELECT_PAYMENT_METHOD = "Please Select Payment Method";
        //public const string DO_NOT_REMOVE_TRAY = "PLEASE COMPLETE TRANSACTION BEFORE REMOVING TRAY.";
        public const string DO_NOT_REMOVE_TRAY = "DO NOT REMOVE ITEMS.";
       
       
        
        public const string PAYMENT_FAILED = "Payment Failed";
        public const string PAYMENT_MOD_INVALID = "Invalid payment mode";
        public const string PAYMENT_PLEASE_TAP_CARD = "PLEASE TAP CARD";
        public const string PAYMENT_PLEASE_TAP_MEMBERSHIP_CARD = "PLEASE TAP MEMBERSHIP CARD";
        public const string PAYMENT_PLEASE_SCAN_QR_CODE = "PLEASE SCAN QR TO PAY";
        public const string PAYMENT_PLEASE_SCAN_YOUR_FACE = "PLEASE SCAN YOUR FACE";
        public const string PAYMENT_PAID = "Payment Successful";
        public const string TIMED_OUT = "TIMED OUT";
        public const string TRANSACTION_TIMED_OUT = "Transaction TIMEOUT, please put tray on table again to start new transaction.";
        public const string SCANNING = "Scanning, Please do not move your tray";
        public const string PLEASE_WAIT = "Please wait...";
        public const string PLEASE_PAY = "Please Select Payment Method";
        public const string ACTIVE_PAYMENT_TIME_OUT = "Activate payment device timeout";
        public const string ACTIVATING_PAYMENT = "Activating payment";
        public const string ACTIVATE_PAYMENT_FAILED = "Activate payment failed";
        //public const string PRICE_NOT_DEFINE = "Cannot process transaction because Price is not defined. Please contact administrator for support";
        public const string PRICE_NOT_DEFINE = "Price not defined. Please contact administrator.";
        public const string PRODUCT_NOT_ASSIGNED = "The Plate is not mapped to any Product.";
        public const string CANCELLING_TRANSACTION = "Cancelling transaction...";
        public const string CAN_NOT_DEACTIVE_PAYMENT_DEVICE = "Coudn't deactivate payment device";
        public const string CAN_NOT_FIND_SESSION = "Cannot find appropriate session";
        public const string NO_PLATE_REGISTERED = "There is no plate registered in the system. Sales not ready.";
        public const string NO_MENU_SESSION = "There is no menu schedule defined for the session";
        //public const string UNREGISTED_PLATE_IN_SESSION = "Unregistered plate in the current session detected, please contact canteen staff.";
        public const string UNREGISTED_PLATE_IN_SESSION = "Plate unregistered. Please contact administrator.";
        public const string UNREGISTED_PLATEMODEL_IN_SESSION = "Plate Model unregistered. Please contact administrator.";
        public const string SOLD_PLATE_DETECTED = "Sold plates detected, please contact canteen staff.";
        //FacialPayment
        public const string FACE_NOT_RECOGNISED = "Face not recognised";

    }
}
