using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Konbi.Simulator.TxTester
{
    public enum PaymentState
    {
        /// <summary>
        /// Init transaction
        /// </summary>
        Init = 100,

        /// <summary>
        /// when transaction is valid to start payment.
        /// </summary>
        ReadyToPay = 101,
        /// <summary>
        /// When receive payment commad, return Enable success/Enable Error
        /// </summary>
        ActivatingPayment = 105,
        ActivatedPaymentSuccess = 110,
        ActivatedPaymentError = 115,

        /// <summary>
        /// 
        /// </summary>
        Cancelled = 120,

        /// <summary>
        /// When card scanned, cash/coin start to inserting => InProgress
        /// </summary>
        InProgress = 200,

        /// <summary>
        /// Payment success
        /// </summary>
        Success = 300,

        /// <summary>
        /// Payment failure
        /// </summary>
        Failure = 400,
        /// <summary>
        /// in case  transaction was rejected e.g:  Plate alread sold for the session. cannot sell again
        /// </summary>
        Rejected = 500,

    }
    public class MenuItemInfo
    {
        public string Name { get; set; }

        public string ImageUrl { get; set; }

        public string Desc { get; set; }

        public string Code { get; set; }

        public string Color { get; set; }

        public decimal Price { get; set; }

        public decimal PriceContractor { get; set; }

        public Guid PlateId { get; set; }

        public Guid ProductId { get; set; }

        public string ProductName { get; set; }

        public PlateInfo Plate { get; set; }

        public Product Product { get; set; }

        public Guid UId { get; set; }

        public string BarCode { get; set; }
    }
    public class Product
    {
        public Product()
        {

        }
        public string SKU { get; set; }
        public string Name { get; set; }

        public int Status { get; set; }
        public string Unit { get; set; }
        public float Price { get; set; }
        public float? ContractorPrice { get; set; }
        public string ImageUrl { get; set; }
        public string ImageChecksum { get; set; }

      
        public Guid? CategoryId { get; set; }

        public int? TenantId { get; set; }
        public string Barcode { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string ShortDesc1 { get; set; }
        public string ShortDesc2 { get; set; }
        public string ShortDesc3 { get; set; }
        public string Desc { get; set; }
        public Guid? PlateId { get; set; }
       
        public int? DisplayOrder { get; set; }
    }
    public enum PlateType
    {
        Plate,
        Tray,
        TakeAway
    }
    public class PlateInfo
    {
        public string Uid { get; set; }
        public PlateType? Type { get; set; }

        public string Code { get; set; }

        public Guid PlateId { get; set; }
    }
    public enum PaymentType
    {
        MdbCashless = 100,
        Cyklone = 101,
        IucApi = 102,
        Iuc_CEPAS = 103,
        Iuc_Contactless = 104,
        Cash = 105
    }
    public class CashlessDetail 
    {
        public decimal Amount { get; set; }
        public string Tid { get; set; }
        public string Mid { get; set; }
        public string Invoice { get; set; }
        public string Batch { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string Rrn { get; set; }
        public string ApproveCode { get; set; }
        public string EntryMode { get; set; }
        public string AppLabel { get; set; }
        public string Aid { get; set; }
        public string Tc { get; set; }
    }
    public class TransactionInfo : INotifyPropertyChanged
    {
        private PaymentState _paymentState;
        public Guid Id { get; set; }
        public List<MenuItemInfo> MenuItems { get; set; }
        public decimal Amount => (MenuItems != null ? MenuItems.Sum(el => el.Price) : (decimal)0.0);
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
        public PaymentType PaymentType { get; set; }
        public string CustomerMessage { get; set; }
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
    }
}
