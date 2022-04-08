using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NfcPaymentDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            var cardIds = ConfigurationManager.AppSettings["CardIds"];
            var cardList = cardIds.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            while(true)
            {
                var  s= Console.ReadLine();
                if (s == "quit")
                    return;

                if(cardList.Any(x=>x==s))
                {
                    Console.WriteLine("Found!");
                }
            }
        }
    }
}
