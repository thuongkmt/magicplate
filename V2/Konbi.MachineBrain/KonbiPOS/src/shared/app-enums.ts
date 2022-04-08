export class PaymentState {
    static Init: number = 100;
    static ReadyToPay: number = 101;
    static ActivatingPayment: number = 105;
    static ActivatedPaymentSuccess: number = 110;
    static ActivatedPaymentError: number = 115;
    static Cancelled: number = 120;
    static InProgress: number = 200;
    static Success: number = 300;
    static Failure: number = 400;
    static Rejected: number = 500;
}