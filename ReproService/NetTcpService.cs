using System;

namespace ReproService
{
    public class NetTcpService : INetTcpService
    {
        public string SayHello(string name)
        {
            return string.Format("Hello, {0}!", name);
        }
    }
}
