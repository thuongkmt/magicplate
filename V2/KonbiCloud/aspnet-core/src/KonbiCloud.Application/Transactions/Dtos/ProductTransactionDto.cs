using KonbiCloud.Products;
using System;
using System.Collections.Generic;
using System.Text;

namespace KonbiCloud.Transactions.Dtos
{
    public class ProductTransactionDto
    {
        public Product Product { get; set; }
        public decimal Amount { get; set; }       
    }
}
