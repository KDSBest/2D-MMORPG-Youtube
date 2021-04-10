using ReliableUdp;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace CommonServer.Udp
{
    public class UdpManagerListener
    {
        private UdpManager udp;
        private readonly string connKey;
        private IUdpListener udpListener;

        public bool IsRunning
        {
            get
            {
                if (this.udpListener.UdpManager == null)
                    return false;

                return this.udpListener.UdpManager.IsRunning;
            }
        }

        public UdpManagerListener(string connKey, IUdpListener udpListener)
        {
            if (udpListener == null)
            {
                throw new ArgumentNullException(nameof(udpListener));
            }

            if(string.IsNullOrEmpty(connKey))
            {
                throw new ArgumentException(nameof(connKey));
            }

            this.connKey = connKey;
            this.udpListener = udpListener;
        }

        public void Update()
        {
            if (this.udpListener != null && this.udp != null)
                this.udpListener.Update();
        }

        public async Task Start(int port)
        {
            try
            {
                this.udp = new UdpManager(this.udpListener, connKey, int.MaxValue);

                if (!this.udp.Start(port))
                {
                    throw new InvalidOperationException("Udp Server couldn't start.");
                }
            }
            catch (Exception)
            {
                Close();
                throw;
            }
        }

        public void Close()
        {
            this.Stop();
        }

        private void Stop()
        {
            if (this.udp != null)
            {
                this.udpListener = null;
                this.udp.Stop();
            }
        }
    }
}
