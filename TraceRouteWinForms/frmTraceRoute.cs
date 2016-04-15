/*

 MIT License

 Copyright (c) 2016, SNOEI.NET (Ton Snoei)

 Permission is hereby granted, free of charge, to any person
 obtaining a copy of this software and associated documentation
 files (the "Software"), to deal in the Software without
 restriction, including without limitation the rights to use,
 copy, modify, merge, publish, distribute, sublicense, and/or sell
 copies of the Software, and to permit persons to whom the
 Software is furnished to do so, subject to the following
 conditions:

 The above copyright notice and this permission notice shall be
 included in all copies or substantial portions of the Software.

 THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
 EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
 OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
 HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
 WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
 FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
 OTHER DEALINGS IN THE SOFTWARE.

 */

using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TraceRouteWinForms
{
    public partial class frmTraceRoute : Form
    {
        public frmTraceRoute()
        {
            InitializeComponent();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            await Task.Run(() => TraceRouteTask(30, textBox1.Text));
        }

        private void TraceRouteTask(int hopLimit, string hostName)
        {
            List<TraceRouteHopInfo> hops = TraceRoute(hopLimit, hostName);
        }

        private List<TraceRouteHopInfo> TraceRoute(int hopLimit, string hostName)
        {
            List<TraceRouteHopInfo> result = new List<TraceRouteHopInfo>();

            bool dontFragment = true;
            string data = Guid.NewGuid().ToString();

            for (int hopIndex = 0; hopIndex < hopLimit; hopIndex++)
            {
                // Setting the TTL is the heart of the traceroute principle
                int ttl = hopIndex + 1;
                int timeout = 15000; // 15 seconds.
                byte[] dataBytes = Encoding.ASCII.GetBytes(data);
                Ping ping = new Ping();
                PingReply pingReply = default(PingReply);
                PingOptions pingOptions = new PingOptions(ttl, dontFragment);

                // Let's ping
                pingReply = ping.Send(hostName, timeout, dataBytes, pingOptions);

                TraceRouteHopInfo traceRouteHopInfo = new TraceRouteHopInfo() { HopIndex = hopIndex+1, PingReply = pingReply };
                result.Add(traceRouteHopInfo);

                UpdateUI(traceRouteHopInfo);

                if (pingReply.Status == IPStatus.Success)
                    // The ping reached the destination after all hops in between.
                    break;
            }

            return result;
        }

        private void UpdateUI(TraceRouteHopInfo traceRouteHopInfo)
        {
            if (this.InvokeRequired)
                this.Invoke(new Action<TraceRouteHopInfo>(UpdateUI), traceRouteHopInfo);
            else
            {
                listBox1.Items.Add(string.Format("{0}. {1}",
                    traceRouteHopInfo.HopIndex,
                    traceRouteHopInfo.PingReply.Address.ToString()));
            }

        }
    }



    public class TraceRouteHopInfo
    {
        public int  HopIndex { get; set; }
        public PingReply PingReply { get; set; }
    }
}
