using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using DnsClient;
using DnsClient.Protocol;
using System.Linq;
using System.Net.Sockets;
using System.Management;
using System.Text;

namespace DotNetCoreTest
{
    class Program
    {
        struct Test
        {
            public string Guid { get; set; }
            public string Haha { get; set; }
        }

        static void Main(string[] args)
        {
            string title = "THQ";
            string version = "\x10";
            string uuid = "123456781234123412340123456789ABCDEF";
            string locationTag = "10000000bdd8d0d50654e0e5115c69171a91735373da3d5418f6183de1cf2f";

            string concatenated = title + version + uuid + locationTag;
            StringBuilder sb = new StringBuilder(concatenated.Length * 2 + 1);

            foreach (var c in concatenated)
            {
                sb.AppendFormat("{0:X2}", (int)c);
            }

            string eDnsTag = sb.ToString();

            Console.WriteLine(eDnsTag);

            Console.WriteLine("\n\nDone");
            Console.ReadKey();
        }
    }
}
