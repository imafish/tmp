using System;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace DllTest
{
    public class Class1
    {
        public static void Run(string[] args)
        {
            var fqdn = "otg-test.titanhq.com";
            var port = 7771;
            Console.Out.WriteLine(Environment.Version.ToString());

            // download the CA
            // use http protocol as we don't have the CA of the server yet.
            string requestURL = "http://" + fqdn + "/ssl/ca.der";
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestURL);
            request.Method = "GET";
            request.AllowAutoRedirect = true;
            byte[] certData;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream dataStream = response.GetResponseStream())
            {
                var dataLength = int.Parse(response.Headers.Get("Content-Length"));
                certData = new byte[dataLength];
                dataStream.Read(certData, 0, dataLength);
            }
            X509Certificate2 ourCA = new X509Certificate2(certData);

            RemoteCertificateValidationCallback callback = (object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors) =>
            {
                return ServerCertificateValidationCallback(ourCA, sender, certificate, chain, sslPolicyErrors);
            };

            // try establish an SSL connection and verify the obtained cert.
            // Create a TCP/IP client socket.
            TcpClient client = new TcpClient(fqdn, port);
            // Create an SSL stream that will close the client's stream.
            SslStream sslStream = new SslStream(client.GetStream(), false, callback, null);
            // The server name must match the name on the server certificate.
            sslStream.AuthenticateAsClient(fqdn);
            // let's just fail if exception is thrown.

            Console.Out.WriteLine("We're good.");
        }

        static bool ServerCertificateValidationCallback(X509Certificate2 ourCA, object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if ((sslPolicyErrors & (SslPolicyErrors.RemoteCertificateNameMismatch | SslPolicyErrors.RemoteCertificateNotAvailable)) > 0)
            {
                return false;
            }

            X509Chain customChain = new X509Chain(false);
            customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            customChain.ChainPolicy.ExtraStore.Add(ourCA);
            if (!customChain.Build(chain.ChainElements[0].Certificate))
            {
                return false;
            }
            var chainElements = customChain.ChainElements;
            bool valid = chainElements[chainElements.Count - 1].Certificate.Thumbprint == ourCA.Thumbprint;
            customChain.Reset();

            return valid;
        }

    }
}
