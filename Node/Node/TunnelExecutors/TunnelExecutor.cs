using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Node.TunnelExecutors.Models;

namespace Node.TunnelExecutors
{
	public class TunnelExecutor : IDisposable
    {
        private readonly Guid _key;
        private readonly ILogger _logger;
        private readonly Socket _server; 
        private readonly Socket _client;
        private ISystemClock _systemClock;
        public DateTimeOffset LastAccessed;
        
        
        protected bool IsDisposed { get; private set; }
        private byte[] _ServerBuffer = new byte[Settings.BufferSize];
        private byte[] _ClientBuffer = new byte[Settings.BufferSize];
        private X509Certificate _serverCertificate;
        private readonly SslStream _sslRemoteStream;
        private readonly SslStream _sslClientStream;


        public TunnelExecutor(TunnelStructure tunnel, ILogger logger, X509Certificate certificate) {
            ArgumentNullException.ThrowIfNull(tunnel);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(certificate);

            _key = tunnel.key;
            _server = tunnel.remote;
            _client = tunnel.client;
            _logger = logger;
            _systemClock = new SystemClock();
            LastAccessed = _systemClock.UtcNow;
            _sslRemoteStream = tunnel.sslRemoteStream;
            _sslClientStream = tunnel.sslClientStream;
            _serverCertificate = certificate;
        }


        
        public static async Task CloseSocketAsync(Socket socket,
            SocketShutdown socketShutdown = SocketShutdown.Both, bool reuseSocket = false)
        {
            //ArgumentNullException.ThrowIfNull(socket);
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(socketShutdown);
                    Func<CancellationToken, Task> DisconnectAsyncDelegate = async cts
                        => await socket.DisconnectAsync(reuseSocket, cts);
                    await Commit(action: DisconnectAsyncDelegate);
                }
                socket.Close();
                socket.Dispose();
            }
            catch
            {
                throw;
            }
            finally
            {
                socket.Close();
                socket.Dispose();
            }
        }

        public static async Task CloseSSLStreamAsync(SslStream sslStream)
        {
            ArgumentNullException.ThrowIfNull(sslStream);
            try
            {
                sslStream.Close();
                await sslStream.DisposeAsync();
            }
            catch
            {
                throw;
            }
            finally
            {
                sslStream.Close();
                await sslStream.DisposeAsync();
            }
        }

        public bool IsExpired()
        {
            var utcNow = _systemClock.UtcNow;
            if (utcNow - LastAccessed > Settings.IdleTimeout)
            {
                return true;
            }
            return false;
        }

        public static async Task<int?> Commit(Func<CancellationToken, Task<int?>>? method = null, Func<CancellationToken, Task>? action = null, CancellationToken cancellationToken = default)
        {
            using (var timeout = new CancellationTokenSource(Settings.IoTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                try
                {
                    cts.Token.ThrowIfCancellationRequested();
                    if (action != null)
                    {
                        await action(cts.Token);
                    }
                    if (method != null)
                    {
                        return await method(cts.Token);
                    }
                    return -1;
                }
                catch (OperationCanceledException oex)
                {
                    if (timeout.Token.IsCancellationRequested)
                    {
                        throw new OperationCanceledException("Timed out waiting the operation.", oex, timeout.Token);
                    }
                    throw;
                }
            }
        }

        private async Task<int?> ReceiveAsyncExtend(SslStream socket, byte[] buffer, CancellationToken cts)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(socket);
            cts.ThrowIfCancellationRequested();
            if (!socket.CanRead)
            {
                throw new ArgumentException();
            }
            int result;
            int count = 0;
            do
            {
                result = await socket.ReadAsync(buffer, 0, buffer.Length, cts);
                count = count + result;
            }   while (result > 0);
            return count;
        }

        private void Log(string logMessage)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine(logMessage);
            }
        }

        private async Task<int?> ReceiveSocketAsyncExtend(Socket socket, byte[] buffer, CancellationToken cts)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(socket);
            cts.ThrowIfCancellationRequested();
            if (!socket.Connected)
            {
                throw new SocketException();
            }
            var receive = await socket.ReceiveAsync(buffer, SocketFlags.None, cts);
            return receive;
        }

        private async Task SendSocketAsyncExtend(Socket socket, byte[] buffer, int response, CancellationToken cts)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(socket);
            cts.ThrowIfCancellationRequested();
            if (response < 0 || buffer.Length < response)
            {
                throw new ArgumentOutOfRangeException(nameof(response), "Out of range.");
            }
            if (!socket.Connected)
            {
                throw new SocketException();
            }
            await socket.SendAsync(new ArraySegment<byte>(buffer, 0, response), SocketFlags.None);
        }

        private async Task SendAsyncExtend(SslStream socket, byte[] buffer, int response, CancellationToken cts)
        {
            ArgumentNullException.ThrowIfNull(buffer);
            ArgumentNullException.ThrowIfNull(socket);
            cts.ThrowIfCancellationRequested();
            if (response < 0 || buffer.Length < response)
            {
                throw new ArgumentOutOfRangeException(nameof(response), "Out of range.");
            }
            if (!socket.CanWrite)
            {
                throw new ArgumentException();
            }
            await socket.WriteAsync(buffer, 0, response, cts);
            await socket.FlushAsync(cts);
        }
        
        private async Task ListenServerAsync()
        {
            /*Func<CancellationToken, Task<int?>> ReceiveAsyncExtendDelegate = async cts =>
                await ReceiveSocketAsyncExtend(_server, _ServerBuffer, cts);*/
            Func<CancellationToken, Task<int?>> ReceiveAsyncExtendDelegate = async cts =>
                await ReceiveAsyncExtend(_sslRemoteStream, _ServerBuffer, cts);
            while (true)
            {
                    if (!_server.Connected)
                    {
                        _logger.LogError("Server has closed the socket!");
                        //    await CloseSocketAsync(_server);
                        //    await CloseSocketAsync(_client);
                        //await CloseSSLStreamAsync(_sslRemoteStream);
                        break;
                    }
                /*if (IsExpired())
                {
                    _logger.LogError("Session is expired!");
                    await CloseSocketAsync(_server);
                    await CloseSSLStreamAsync(_sslRemoteStream);
                    break;
                }*/
                
                    int? response = await Commit(ReceiveAsyncExtendDelegate);
                    if (response.Value > 0)
                    {
                        Func<CancellationToken, Task> SendAsyncExtendDelegate = async cts =>
                                    await SendSocketAsyncExtend(_client, _ServerBuffer, response.Value, cts);
                        await Commit(action: SendAsyncExtendDelegate);
                        LastAccessed = _systemClock.UtcNow;
                    }
            }
        }

        private async Task ListenClientAsync()
        {
            Func<CancellationToken, Task<int?>> ReceiveAsyncExtendDelegate = async cts =>
                await ReceiveSocketAsyncExtend(_client, _ClientBuffer, cts);
            while (true)
            {
                    if (!_client.Connected)
                    {
                        _logger.LogError("Client is not connected");
                        //_client.Shutdown(SocketShutdown.Both);
                        //await _client.DisconnectAsync(true);
                        //await CloseSocketAsync(_server);
                        //await CloseSocketAsync(_client);
                        //await CloseSSLStreamAsync(_sslRemoteStream);
                        break;
                    }
                    /*if (IsExpired())
                    {
                        _logger.LogError("Session is expired!");
                        await CloseSocketAsync(_server);
                        await CloseSSLStreamAsync(_sslRemoteStream);
                        break;
                    }*/
                    int? response = await Commit(ReceiveAsyncExtendDelegate);
                    if (response.Value > 0)
                    {
                        /*Func<CancellationToken, Task> SendAsyncExtendDelegate = async cts =>
                                    await SendSocketAsyncExtend(_server, _ClientBuffer, response.Value, cts);*/
                        Func<CancellationToken, Task> SendAsyncExtendDelegate = async cts =>
                                    await SendAsyncExtend(_sslRemoteStream, _ClientBuffer, response.Value, cts);
                        await Commit(action: SendAsyncExtendDelegate);
                        LastAccessed = _systemClock.UtcNow;
                    }
            }
        }

        public void StartTunneling()
        {
            Task.Run(async () =>
            {
                await ListenClientAsync();
            });
            Task.Run(async () =>
            {
                await ListenServerAsync();
            });
            
        }

        /*
        private async Task TunnelAsync(Socket client, Socket remote)
        {
            byte[] buffer = new byte[1024];
            try
            {
                while (true)
                {
                    int response = await client.ReceiveAsync(buffer, SocketFlags.None);
                    if (response == 0)
                    {
                        break;
                    }
                    if (remote.Connected)
                    {
                        await remote.SendAsync(new ArraySegment<byte>(buffer, 0, response), SocketFlags.None);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError("Tunnel error: {0}, {1}", e.Message);
            }
        }

        public void StartTunnelingAsync()
        {
            Task.Run(async () =>
            {
                List<Task> tasks = new List<Task>() {
                    Task.Run(async () =>
                    {
                        await TunnelAsync(_server, _client);
                    }),
                    Task.Run(async () =>
                    {
                        await TunnelAsync(_client, _server);
                    })};
                await Task.WhenAll(tasks);
                await CloseSocketAsync(_server);
                await CloseSocketAsync(_client);
            });
        }*/

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
            {
                return;
            }
            if (disposing)
            {
                _server.Dispose();
            }
            IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

