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
        private SerialPortWatcher _watcher;
        public event EventHandler<string> CardConnected;
        public event EventHandler<string> CardDisconnected;

        public SerialRfidPlugin()
        {
            _watcher = new SerialPortWatcher();
            _watcher.PortsChanged += _watcher_PortsChanged;
            
        }

        private void _watcher_PortsChanged(object sender, EventArgs e)
        {
            OpenPortIfAvailable();
        }

        public Task BeginInit()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _watcher?.Dispose();
            sp?.Dispose();  
        }

        private void OpenPortIfAvailable()
        {
            if (_watcher.ComPorts.Any(p => p == "COM7"))
            {
                if(sp != null)
                {
                    sp.Dispose();
                }
                sp = new SerialPort("COM7");
                sp.DataReceived += (oo, ee) =>
                {
                    string indata = sp.ReadExisting();
                    var id = indata.Trim(new char[] { '\r', '\n' });
                    CardConnected?.Invoke(this, id);
                    CardDisconnected?.Invoke(this, id);
                };
                sp.Open();
            }
        }

        public Task EndInit()
        {
            OpenPortIfAvailable();
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
