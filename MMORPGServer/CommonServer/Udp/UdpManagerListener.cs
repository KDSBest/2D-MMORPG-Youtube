using Common.Udp;
using ReliableUdp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace CommonServer.Udp
{
	public class UdpManagerListener
    {
        private UdpManager udp;
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

        public UdpManagerListener(IUdpListener udpListener)
        {
            if (udpListener == null)
            {
                throw new ArgumentNullException(nameof(udpListener));
            }

            this.udpListener = udpListener;
        }

        public async Task UpdateAsync()
        {
            if (this.udpListener != null && this.udp != null)
                await this.udpListener.UpdateAsync();
        }

        public async Task StartAsync(int port)
        {
            try
            {
                this.udp = new UdpManager(this.udpListener, int.MaxValue);

                if(File.Exists("mmo.pfx"))
				{
                    this.udp.Settings.Cert = File.ReadAllBytes("mmo.pfx");
				}

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
