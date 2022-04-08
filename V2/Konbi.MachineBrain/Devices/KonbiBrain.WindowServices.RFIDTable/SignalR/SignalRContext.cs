
using KonbiBrain.WindowServices.RFIDTable.Helper;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace KonbiBrain.WindowServices.RFIDTable.SignalR
{
    public class SignalRContext
    {
        private string lastMessageSignal = "";
        public  HubConnection Hub { get; protected set; }
        
        public SignalRContext()
        {
            Hub = new HubConnectionBuilder().WithUrl(ConfigurationManager.AppSettings["signalR.HubConnection"]).Build();            
           
            Hub.Closed += Hub_ClosedAsync;
            Hub.StartAsync().ContinueWith(task=> {
                if (task.IsFaulted)
                {

                }
                else
                {
                    if(Hub.State == HubConnectionState.Connected)
                    {
                        Hub.InvokeAsync("JoinGroup","TableDevice");
                    }
                }
            });


        }

        private async Task Hub_ClosedAsync(Exception arg)
        {
            await Task.Delay(new Random().Next(0, 5) * 1000);
            await Hub.StartAsync().ContinueWith(task => {
                if (task.IsFaulted)
                {

                }
                else
                {
                    if (Hub.State == HubConnectionState.Connected)
                    {
                        Hub.InvokeAsync("JoinGroup", "TableDevice");
                    }
                }
            });
        }
        public async Task PublishDishes(IEnumerable<Dish> dishes)
        {
            var messageSignal = GetMessageSignal(dishes);
            if(messageSignal != lastMessageSignal)
            {
                await Hub.InvokeAsync("UpdateDishes", dishes.Select(d => new Dto.DishInfo() { UID = d.UID.Trim(), UType = d.UType.Trim() })).ContinueWith(task => {
                    if (task.IsCompleted && task.IsFaulted == false)
                    {
                        lastMessageSignal = messageSignal;
                    }
                });
            }
            
        }

        private string GetMessageSignal(IEnumerable<Dish> dishes)
        {
            
            return string.Join("-", dishes.Select(el => el.UType + el.UID));
        }
    }
}
