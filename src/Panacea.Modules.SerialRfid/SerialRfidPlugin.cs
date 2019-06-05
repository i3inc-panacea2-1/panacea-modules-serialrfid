using Panacea.Modularity.RfidReader;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Panacea.Modules.SerialRfid
{
    public class SerialRfidPlugin : IRfidReaderPlugin
    {
        private SerialPort sp;

        public event EventHandler<string> CardConnected;
        public event EventHandler<string> CardDisconnected;

        public SerialRfidPlugin()
        {
            sp = new SerialPort("COM7");
            sp.DataReceived += (oo, ee) =>
            {
                string indata = sp.ReadExisting();
                var id = indata.Trim(new char[] { '\r', '\n' });
                CardConnected?.Invoke(this, id);
                CardDisconnected?.Invoke(this, id);
            };
        }
        public Task BeginInit()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            sp.Dispose();  
        }

        public Task EndInit()
        {
            sp.Open();
            return Task.CompletedTask;
        }

        public Task Shutdown()
        {
            sp.Close();
            return Task.CompletedTask;
        }

        public void SimulateCardTap(string s)
        {
            CardConnected?.Invoke(this, s);
        }
    }
}
