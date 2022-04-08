using System;
using System.Collections.Generic;
using System.Text;

namespace Konbi.WindowServices.QRPayment
{
    public interface IApplication
    {
        void Run(string[] args);
    }
}
