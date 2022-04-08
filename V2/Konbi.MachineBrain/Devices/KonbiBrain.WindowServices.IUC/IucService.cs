using KonbiBrain.WindowServices.IUC.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace KonbiBrain.WindowServices.IUC
{
    public partial class IucService : ServiceBase
    {
        private IIucHandler _iucHandler;
        public IucService(IIucHandler iucHandler)
        {
            _iucHandler = iucHandler;
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            Start();
        }

        protected override void OnStop()
        {
        }
        public void Start()
        {
            _iucHandler.Handle();

        }
    }
}
