# .NET 4.6 breaking change for WCF custom bindings using NETTCP protocol with SSL security

## Description

I discovered a case in which a pair of C# client/server applications targetting .NET 4.5.x stops working on computers that upgrade to .NET 4.6.

The issue was found on WCF services configured via custom binding with NETTCP tranmission protocol with SSL security.

```xml
<customBinding>
  <binding name="CustomNetTcp">
    <sslStreamSecurity/>
    <binaryMessageEncoding/>
    <tcpTransport transferMode="Buffered"/>
  </binding>
</customBinding>
```

## Conditions

- NET 4.6 is installed
- Application targets NET 4.5.x
- WCF service using custom binding with NETTCP transport and SSL security.

## Symptoms

The WCF service host starts with no issue.
	
When the client attempts to call the service, `CommunicationException` is encountered:

> The socket connection was aborted. This could be caused by an error processing your message 
> or a receive timeout being exceeded by the remote host, or an underlying network resource issue. 
> Local socket timeout was '00:10:00'.


WCF error tracing on the service reveals a `System.ComponentModel.Win32Exception` :

> The client and server cannot communicate, because they do not possess a common algorithm


## New sslProtocols attribute, a false workaround

The trace shows that issue is SSL related.  NET 4.6. introduced an enhancement to WCF that allows ssl protocols to be selected in the binding configuration.  Specifying sslProtocol on the server side binding resolves the issue.  However, it appears that this was only added for .NET 4.6, so this only works on machines that have 4.6 installed.  Oddly, even though the application still targets 4.5.x, the attribute can still be used when .NET 4.6 is installed.  

Had the new sslProtocols attribute been backported to 4.5.x, then at least it could have been used to resolve the issue.  Apparently it is a planned feature in the .NET core version of WCF: https://github.com/dotnet/wcf/issues/39

```xml
<customBinding>
  <binding name="CustomNetTcp">
    <sslStreamSecurity sslProtocols="Ssl3"/>
    <binaryMessageEncoding/>
    <tcpTransport transferMode="Buffered"/>
  </binding>
</customBinding>
```

#### Full server-side exception
```xml
<E2ETraceEvent xmlns="http://schemas.microsoft.com/2004/06/E2ETraceEvent">
<System xmlns="http://schemas.microsoft.com/2004/06/windows/eventlog/system">
<EventID>131075</EventID>
<Type>3</Type>
<SubType Name="Error">0</SubType>
<Level>2</Level>
<TimeCreated SystemTime="2015-07-27T21:17:39.4856936Z" />
<Source Name="System.ServiceModel" />
<Correlation ActivityID="{b5d1ca44-bcd7-479e-a2a1-349f172e08c5}" />
<Execution ProcessName="SunGard.Vpm.Server.ServiceHost.vshost" ProcessID="14224" ThreadID="38" />
<Channel />
<Computer>VPM-NY-DLING</Computer>
</System>
<ApplicationData>
<TraceData>
<DataItem>
<TraceRecord xmlns="http://schemas.microsoft.com/2004/10/E2ETraceEvent/TraceRecord" Severity="Error">
<TraceIdentifier>http://msdn.microsoft.com/en-US/library/System.ServiceModel.Diagnostics.ThrowingException.aspx</TraceIdentifier>
<Description>Throwing an exception.</Description>
<AppDomain>SunGard.Vpm.Server.ServiceHost.vshost.exe</AppDomain>
<Exception>
<ExceptionType>System.ServiceModel.Security.SecurityNegotiationException, System.ServiceModel, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</ExceptionType>
<Message>A call to SSPI failed, see inner exception.</Message>
<StackTrace>
at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.CompleteAuthenticateAsServer(IAsyncResult result)
at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.OnAuthenticateAsServer(IAsyncResult result)
at System.Runtime.Fx.AsyncThunk.UnhandledExceptionFrame(IAsyncResult result)
at System.Net.LazyAsyncResult.Complete(IntPtr userToken)
at System.Net.LazyAsyncResult.ProtectedInvokeCallback(Object result, IntPtr userToken)
at System.Net.Security.SslState.FinishHandshake(Exception e, AsyncProtocolRequest asyncRequest)
at System.Net.Security.SslState.ReadFrameCallback(AsyncProtocolRequest asyncRequest)
at System.Net.AsyncProtocolRequest.CompleteRequest(Int32 result)
at System.Net.FixedSizeReader.CheckCompletionBeforeNextRead(Int32 bytes)
at System.Net.FixedSizeReader.ReadCallback(IAsyncResult transportResult)
at System.Runtime.AsyncResult.Complete(Boolean completedSynchronously)
at System.ServiceModel.Channels.ConnectionStream.IOAsyncResult.OnAsyncIOComplete(Object state)
at System.ServiceModel.Channels.TracingConnection.TracingConnectionState.ExecuteCallback()
at System.Net.Sockets.SocketAsyncEventArgs.OnCompleted(SocketAsyncEventArgs e)
at System.Net.Sockets.SocketAsyncEventArgs.FinishOperationSuccess(SocketError socketError, Int32 bytesTransferred, SocketFlags flags)
at System.Net.Sockets.SocketAsyncEventArgs.CompletionPortCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* nativeOverlapped)
at System.Threading._IOCompletionCallback.PerformIOCompletionCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* pOVERLAP)
</StackTrace>
<ExceptionString>System.ServiceModel.Security.SecurityNegotiationException: A call to SSPI failed, see inner exception. ---&gt; System.Security.Authentication.AuthenticationException: A call to SSPI failed, see inner exception. ---&gt; System.ComponentModel.Win32Exception: The client and server cannot communicate, because they do not possess a common algorithm
   --- End of inner exception stack trace ---
   at System.Net.Security.SslState.InternalEndProcessAuthentication(LazyAsyncResult lazyResult)
   at System.Net.Security.SslState.EndProcessAuthentication(IAsyncResult result)
   at System.ServiceModel.Channels.SslStreamSecurityUpgradeAcceptor.AcceptUpgradeAsyncResult.OnCompleteAuthenticateAsServer(IAsyncResult result)
   at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.CompleteAuthenticateAsServer(IAsyncResult result)
   --- End of inner exception stack trace ---</ExceptionString>
<InnerException>
<ExceptionType>System.Security.Authentication.AuthenticationException, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</ExceptionType>
<Message>A call to SSPI failed, see inner exception.</Message>
<StackTrace>
at System.Net.Security.SslState.InternalEndProcessAuthentication(LazyAsyncResult lazyResult)
at System.Net.Security.SslState.EndProcessAuthentication(IAsyncResult result)
at System.ServiceModel.Channels.SslStreamSecurityUpgradeAcceptor.AcceptUpgradeAsyncResult.OnCompleteAuthenticateAsServer(IAsyncResult result)
at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.CompleteAuthenticateAsServer(IAsyncResult result)
</StackTrace>
<ExceptionString>System.Security.Authentication.AuthenticationException: A call to SSPI failed, see inner exception. ---&gt; System.ComponentModel.Win32Exception: The client and server cannot communicate, because they do not possess a common algorithm
   --- End of inner exception stack trace ---
   at System.Net.Security.SslState.InternalEndProcessAuthentication(LazyAsyncResult lazyResult)
   at System.Net.Security.SslState.EndProcessAuthentication(IAsyncResult result)
   at System.ServiceModel.Channels.SslStreamSecurityUpgradeAcceptor.AcceptUpgradeAsyncResult.OnCompleteAuthenticateAsServer(IAsyncResult result)
   at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.CompleteAuthenticateAsServer(IAsyncResult result)</ExceptionString>
<InnerException>
<ExceptionType>System.ComponentModel.Win32Exception, System, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089</ExceptionType>
<Message>The client and server cannot communicate, because they do not possess a common algorithm</Message>
<StackTrace>
at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.CompleteAuthenticateAsServer(IAsyncResult result)
at System.ServiceModel.Channels.StreamSecurityUpgradeAcceptorAsyncResult.OnAuthenticateAsServer(IAsyncResult result)
at System.Runtime.Fx.AsyncThunk.UnhandledExceptionFrame(IAsyncResult result)
at System.Net.LazyAsyncResult.Complete(IntPtr userToken)
at System.Net.LazyAsyncResult.ProtectedInvokeCallback(Object result, IntPtr userToken)
at System.Net.Security.SslState.FinishHandshake(Exception e, AsyncProtocolRequest asyncRequest)
at System.Net.Security.SslState.ReadFrameCallback(AsyncProtocolRequest asyncRequest)
at System.Net.AsyncProtocolRequest.CompleteRequest(Int32 result)
at System.Net.FixedSizeReader.CheckCompletionBeforeNextRead(Int32 bytes)
at System.Net.FixedSizeReader.ReadCallback(IAsyncResult transportResult)
at System.Runtime.AsyncResult.Complete(Boolean completedSynchronously)
at System.ServiceModel.Channels.ConnectionStream.IOAsyncResult.OnAsyncIOComplete(Object state)
at System.ServiceModel.Channels.TracingConnection.TracingConnectionState.ExecuteCallback()
at System.Net.Sockets.SocketAsyncEventArgs.OnCompleted(SocketAsyncEventArgs e)
at System.Net.Sockets.SocketAsyncEventArgs.FinishOperationSuccess(SocketError socketError, Int32 bytesTransferred, SocketFlags flags)
at System.Net.Sockets.SocketAsyncEventArgs.CompletionPortCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* nativeOverlapped)
at System.Threading._IOCompletionCallback.PerformIOCompletionCallback(UInt32 errorCode, UInt32 numBytes, NativeOverlapped* pOVERLAP)
</StackTrace>
<ExceptionString>System.ComponentModel.Win32Exception (0x80004005): The client and server cannot communicate, because they do not possess a common algorithm</ExceptionString>
<NativeErrorCode>80090331</NativeErrorCode>
</InnerException>
</InnerException>
</Exception>
</TraceRecord>
</DataItem>
</TraceData>
</ApplicationData>
</E2ETraceEvent> 
```

#### Full client-side exception:
```
System.ServiceModel.CommunicationException was unhandled
HResult=-2146233087
Message=The socket connection was aborted. This could be caused by an error processing your message or a receive timeout being exceeded by the remote host, or an underlying network resource issue. Local socket timeout was '00:10:00'.
Source=mscorlib
StackTrace:
  Server stack trace: 
     at System.ServiceModel.Channels.SocketConnection.ReadCore(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout, Boolean closing)
     at System.ServiceModel.Channels.SocketConnection.Read(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout)
     at System.ServiceModel.Channels.DelegatingConnection.Read(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout)
     at System.ServiceModel.Channels.ConnectionStream.Read(Byte[] buffer, Int32 offset, Int32 count)
     at System.Net.FixedSizeReader.ReadPacket(Byte[] buffer, Int32 offset, Int32 count)
     at System.Net.Security.SslState.StartReceiveBlob(Byte[] buffer, AsyncProtocolRequest asyncRequest)
     at System.Net.Security.SslState.CheckCompletionBeforeNextReceive(ProtocolToken message, AsyncProtocolRequest asyncRequest)
     at System.Net.Security.SslState.StartSendBlob(Byte[] incoming, Int32 count, AsyncProtocolRequest asyncRequest)
     at System.Net.Security.SslState.ForceAuthentication(Boolean receiveFirst, Byte[] buffer, AsyncProtocolRequest asyncRequest)
     at System.Net.Security.SslState.ProcessAuthentication(LazyAsyncResult lazyResult)
     at System.Net.Security.SslStream.AuthenticateAsClient(String targetHost, X509CertificateCollection clientCertificates, SslProtocols enabledSslProtocols, Boolean checkCertificateRevocation)
     at System.ServiceModel.Channels.SslStreamSecurityUpgradeInitiator.OnInitiateUpgrade(Stream stream, SecurityMessageProperty& remoteSecurity)
     at System.ServiceModel.Channels.StreamSecurityUpgradeInitiatorBase.InitiateUpgrade(Stream stream)
     at System.ServiceModel.Channels.ConnectionUpgradeHelper.InitiateUpgrade(StreamUpgradeInitiator upgradeInitiator, IConnection& connection, ClientFramingDecoder decoder, IDefaultCommunicationTimeouts defaultTimeouts, TimeoutHelper& timeoutHelper)
     at System.ServiceModel.Channels.ClientFramingDuplexSessionChannel.SendPreamble(IConnection connection, ArraySegment`1 preamble, TimeoutHelper& timeoutHelper)
     at System.ServiceModel.Channels.ClientFramingDuplexSessionChannel.DuplexConnectionPoolHelper.AcceptPooledConnection(IConnection connection, TimeoutHelper& timeoutHelper)
     at System.ServiceModel.Channels.ConnectionPoolHelper.EstablishConnection(TimeSpan timeout)
     at System.ServiceModel.Channels.ClientFramingDuplexSessionChannel.OnOpen(TimeSpan timeout)
     at System.ServiceModel.Channels.CommunicationObject.Open(TimeSpan timeout)
     at System.ServiceModel.Channels.ServiceChannel.OnOpen(TimeSpan timeout)
     at System.ServiceModel.Channels.CommunicationObject.Open(TimeSpan timeout)
     at System.ServiceModel.Channels.ServiceChannel.CallOpenOnce.System.ServiceModel.Channels.ServiceChannel.ICallOnce.Call(ServiceChannel channel, TimeSpan timeout)
     at System.ServiceModel.Channels.ServiceChannel.CallOnceManager.CallOnce(TimeSpan timeout, CallOnceManager cascade)
     at System.ServiceModel.Channels.ServiceChannel.EnsureOpened(TimeSpan timeout)
     at System.ServiceModel.Channels.ServiceChannel.Call(String action, Boolean oneway, ProxyOperationRuntime operation, Object[] ins, Object[] outs, TimeSpan timeout)
     at System.ServiceModel.Channels.ServiceChannelProxy.InvokeService(IMethodCallMessage methodCall, ProxyOperationRuntime operation)
     at System.ServiceModel.Channels.ServiceChannelProxy.Invoke(IMessage message)
  Exception rethrown at [0]: 
     at System.Runtime.Remoting.Proxies.RealProxy.HandleReturnMessage(IMessage reqMsg, IMessage retMsg)
     at System.Runtime.Remoting.Proxies.RealProxy.PrivateInvoke(MessageData& msgData, Int32 type)
     at ReproService.INetTcpService.SayHello(String name)
     at ReproClient.ReproClient.Main(String[] args) in C:\Users\daniel.ling\Source\wcf-net46-tcp-ssl-bug\ReproClient\Program.cs:line 16
     at System.AppDomain._nExecuteAssembly(RuntimeAssembly assembly, String[] args)
     at System.AppDomain.ExecuteAssembly(String assemblyFile, Evidence assemblySecurity, String[] args)
     at Microsoft.VisualStudio.HostingProcess.HostProc.RunUsersAssembly()
     at System.Threading.ThreadHelper.ThreadStart_Context(Object state)
     at System.Threading.ExecutionContext.RunInternal(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
     at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state, Boolean preserveSyncCtx)
     at System.Threading.ExecutionContext.Run(ExecutionContext executionContext, ContextCallback callback, Object state)
     at System.Threading.ThreadHelper.ThreadStart()
InnerException: 
     ErrorCode=10054
     HResult=-2147467259
     Message=An existing connection was forcibly closed by the remote host
     NativeErrorCode=10054
     Source=System
     StackTrace:
          at System.Net.Sockets.Socket.Receive(Byte[] buffer, Int32 offset, Int32 size, SocketFlags socketFlags)
          at System.ServiceModel.Channels.SocketConnection.ReadCore(Byte[] buffer, Int32 offset, Int32 size, TimeSpan timeout, Boolean closing)
     InnerException: 
```