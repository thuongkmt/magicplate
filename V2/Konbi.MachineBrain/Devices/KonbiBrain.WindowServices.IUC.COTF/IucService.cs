using KonbiBrain.WindowServices.IUC.COTF.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC.COTF
{
    partial class IucService : ServiceBase
    {
        private IIucHandler _iucHandler;
        public IucService(IIucHandler iucHandler)
        {
            _iucHandler = iucHandler;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            // TODO: Add code here to start your service.
            Start();
        }

        protected override void OnStop()
        {
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        public void Start()
        {
            _iucHandler.Handle();
        }
    }
}
