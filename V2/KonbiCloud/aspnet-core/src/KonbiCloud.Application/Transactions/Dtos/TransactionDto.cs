using KonbiCloud.Enums;
using System;
using System.Collections.Generic;

namespace KonbiCloud.Transactions.Dtos
{
    public class TransactionDto
    {
        public long Id { get; set; }
        public string TranCode { get; set; }
        public string Buyer { get; set; }
        public DateTime PaymentTime { get; set; }
        public PaymentType PaymentType { get; set; }
        public decimal Amount { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxPercentage { get; set; }
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
                return AmountBeforeTax * TaxPercentage/100;               
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
        public int PlatesQuantity { get; set; }
        public string States { get; set; }
        public ICollection<DishTransactionDto> Dishes { get; set; }
        public ICollection<ProductTransactionDto> Products { get; set; }
        public string Machine { get; set; }
        public string Session { get; set; }
        public string TransactionId { get; set; }
        public string BeginTranImage { get; set; }
        public string EndTranImage { get; set; }
        public string CardLabel { get; set; }
        public string CardNumber { get; set; }
        public string ApproveCode { get; set; }
        public decimal? CashlessPaidAmount { get; internal set; }
    }
}