using KonbiCloud.Transactions;
using Konbini.Messages.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace KonbiCloud.RFIDTable
{
    public class TransactionInfo : INotifyPropertyChanged
    {
        private PaymentState _paymentState;
        public Guid Id { get; set; }
        public List<MenuItemInfo> MenuItems { get; set; }
        public decimal Amount => (MenuItems != null ? MenuItems.Sum(el => el.Price) : (decimal)0.0);
        public decimal DiscountAmount
        {
            get
            {
                return AmountBeforeTax * DiscountPercentage / 100;
            }
        }
        public decimal TaxAmount
        {
            get
            {
                return AmountBeforeTax * TaxPercentage / 100;
            }
        }
        public decimal AmountBeforeTax
        {
            get
            {
                return Amount / (1 + TaxPercentage / 100);
            }

        }
        public decimal SubTotal
        {
            get
            {
                return AmountBeforeTax - DiscountAmount;
            }
        }
        public decimal Total
        {
            get
            {
                return SubTotal + TaxAmount;
            }
        }
        public decimal DiscountPercentage = 0;
        public decimal TaxPercentage { get; set; }
        public string TaxName { get; set; }
        public int PlateCount => (MenuItems != null ? MenuItems.Count() : 0);

        public PaymentState PaymentState
        {
            get { return _paymentState; }
            set
            {
                if (_paymentState != value)
                {
                    _paymentState = value;
                    NotifyPropertyChanged();
                }
                
            }
        }
        public PaymentTypes PaymentType { get; set; }
        public string CustomerMessage { get; set; }

        // Konbi Credits Info
        public decimal KonbiCreditBalance { get; set; }
        public int IsSufficientFund { get; set; }
        public int IsTokenRetrieved { get; set; }
        public int IsUserExist { get; set; }

        public string Status { get; set; }
        public Guid SessionId { get; set; }
        public string Buyer { get; set; }
        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
        public CashlessDetail CashlessInfo { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public dynamic CustomData { get; set; }
    }
}
