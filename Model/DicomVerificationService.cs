using FellowOakDicom.Network;
using FellowOakDicom.Network.Client;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DicomEditor.Model
{
    public static class DicomVerificationService
    {
        public static async Task<bool> VerifyAsync(string serverHost, int serverPort, string serverAET, string appAET)
        {
            bool successful = false;
            var client = DicomClientFactory.Create(serverHost, serverPort, false, appAET, serverAET);
            client.NegotiateAsyncOps();
            var request = new DicomCEchoRequest();
            request.OnResponseReceived += (req, res) =>
            {
                if(res.Status.State == DicomState.Success)
                {
                    successful = true;
                }
            };
            await client.AddRequestAsync(request);
            await client.SendAsync();
            return successful;
        }
    }
}
