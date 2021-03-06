﻿using ReproService;
using System;
using System.ServiceModel;

namespace ReproClient
{
    class ReproClient
    {
        static void Main(string[] args)
        {

            Console.WriteLine("Connecting to service.");
            using (var channelFactory = new ChannelFactory<INetTcpService>("NetTcpBinding_IService1"))
            {
                channelFactory.Open();
                var client = channelFactory.CreateChannel();

                //The line below will throw an exception if .NET 4.6 is installed, works fine with 4.5.x
                string result = client.SayHello(Environment.UserName);
                Console.WriteLine("Recieved response, '{0}'", result);
            }
            Console.ReadLine();
        }
    }
}
