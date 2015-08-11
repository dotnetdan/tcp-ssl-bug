using System.Runtime.Serialization;
using System.ServiceModel;

namespace ReproService
{
    [ServiceContract]
    public interface INetTcpService
    {
        [OperationContract]
        string SayHello(string name);
    }
}
