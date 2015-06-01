using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Thrift.Server;
using Thrift.Protocol;
using Thrift.Transport;

namespace Paul
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.WriteLine("args==" + args.Length);
                if (args.Length == 1 && args[0].Equals("cl"))
                {
                    TTransport transport = new TSocket("localhost", 9095);
                    transport.Open();
                    TProtocol protocol = new TBinaryProtocol(transport);
                    Service.Client client = new Service.Client(protocol);
                    Console.WriteLine("Connect to the server");

                    List<Work> result = client.checkcalc(10, 20);
                    for (int i = 0; i < result.Count; i++)
                    {
                        Console.WriteLine("Num1 : {0}, Num2 : {1}, Comment : {2}", result[i].Num1, result[i].Num2, result[i].Comment);
                    }
                }
                else
                {
                    Server handler = new Server();
                    Service.Processor processor = new Service.Processor(handler);
                    TServerTransport serverTransport = new TServerSocket(9095);
                    TServer server = new TSimpleServer(processor, serverTransport);
                    Console.WriteLine("Starting the server...");
                    server.Serve();
                }

            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.StackTrace);
            }

            Console.WriteLine("Bye");
        }
    }
}
