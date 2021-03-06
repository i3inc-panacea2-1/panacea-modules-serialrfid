﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Panacea.Modules.SerialRfid
{

    public sealed class SerialPortWatcher : IDisposable
    {
        public SerialPortWatcher()
        {
            _taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
            ComPorts = new ObservableCollection<string>(SerialPort.GetPortNames().OrderBy(s => s));

            WqlEventQuery query = new WqlEventQuery("SELECT * FROM Win32_DeviceChangeEvent");

            _watcher = new ManagementEventWatcher(query);
            _watcher.EventArrived += (sender, eventArgs) => CheckForNewPorts(eventArgs);
            _watcher.Start();
        }

        private void CheckForNewPorts(EventArrivedEventArgs args)
        {
            // do it async so it is performed in the UI thread if this class has been created in the UI thread
            Task.Factory.StartNew(CheckForNewPortsAsync, CancellationToken.None, TaskCreationOptions.None, _taskScheduler);
        }

        private void CheckForNewPortsAsync()
        {
            IEnumerable<string> ports = SerialPort.GetPortNames().OrderBy(s => s);

            foreach (string comPort in ComPorts.ToList())
            {
                if (!ports.Contains(comPort))
                {
                    ComPorts.Remove(comPort);
                }
            }

            foreach (var port in ports.ToList())
            {
                if (!ComPorts.Contains(port))
                {
                    AddPort(port);
                }
            }
            PortsChanged?.Invoke(this, null);
        }

        private void AddPort(string port)
        {
            for (int j = 0; j < ComPorts.Count; j++)
            {
                if (port.CompareTo(ComPorts[j]) < 0)
                {
                    ComPorts.Insert(j, port);
                    break;
                }
            }

        }

        public ObservableCollection<string> ComPorts { get; private set; }

        #region IDisposable Members

        public void Dispose()
        {
            _watcher.Stop();
        }

        #endregion
        public event EventHandler PortsChanged;
        private ManagementEventWatcher _watcher;
        private TaskScheduler _taskScheduler;
    }
}
