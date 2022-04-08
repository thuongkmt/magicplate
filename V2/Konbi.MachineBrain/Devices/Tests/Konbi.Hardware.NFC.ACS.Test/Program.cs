using Konbi.Hardware.NFCReader.ACSModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Konbi.Hardware.NFC.ACS.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var ascReaderService = new ASCReader();

           // initialize card reader lisener
            ascReaderService.Init();


            // listen to insert card event
            ascReaderService.OnInsertedCard = cardNumber => {
                Console.WriteLine($"Card detected: {cardNumber}");
            };
            // when NFC card is removed away from reader
            ascReaderService.OnRemovedCard  =() => {
                Console.WriteLine($"Card Removed");
            };

            // listen on reader attachment
            ascReaderService.OnConnectedReader = isConnected => { 
                if(isConnected)
                   
                {
                    Console.WriteLine($"Reader is connected");
                     var readers = ascReaderService.GetReaders();
                    foreach (var reader in readers)
                    {
                        Console.WriteLine($"{reader}");
                    }
                }
                else
                    Console.WriteLine($"Reader is disconnected");
            };
            
            Console.ReadLine();
        }
    }
}
