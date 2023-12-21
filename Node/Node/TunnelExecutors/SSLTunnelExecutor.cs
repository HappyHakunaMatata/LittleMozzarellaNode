using System;
using System.Net.Security;
using System.Net.Sockets;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Node.TunnelExecutors.Models;

namespace Node.TunnelExecutors
{
    public class SSLTunnelExecutor : TunnelExecutor
    {
        private SslStream? _sslRemoteStream;
        private SslStream? _sslClientStream;

        public SslStream RemoteSocket
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_sslRemoteStream);
                return _sslRemoteStream;
            }
            set
            {
                _sslRemoteStream = value;
            }
        }

        public SslStream ClientSocket
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_sslClientStream);
                return _sslClientStream;
            }
            set
            {
                _sslClientStream = value;
            }
        }

        public SSLTunnelExecutor(ILogger logger) : base(logger)
        {
        }

        public void SetSockets(TunnelStructure tunnel)
        {
            ClientSocket = tunnel.sslClientStream;
            RemoteSocket = tunnel.sslRemoteStream;
        }


        private async Task WriteAsync(SslStream socket, byte[] buffer, int response, CancellationToken cts = default)
        {
            try
            {
                cts.ThrowIfCancellationRequested();
                if (response < 0 || buffer.Length < response)
                {
                    _logger.LogInformation("Response error");
                }
                if (!socket.CanWrite)
                {
                    _logger.LogInformation("Socket is not connected");
                }
                await socket.WriteAsync(buffer, 0, response, cts);
                await socket.FlushAsync(cts);
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
        }

        public async Task SendAsync(SslStream socket, byte[] buffer, int response, CancellationToken cancellationToken = default)
        {
            using (var timeout = new CancellationTokenSource(Settings.IoTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                try
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                    await WriteAsync(socket, buffer, response, cts.Token);
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
        private async Task<int> ReadAsync(SslStream socket, byte[] buffer, CancellationToken cts = default)
        {
            try
            {
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
                } while (result > 0);
                return count;
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            return -1;
        }


        public async Task<int> ReceiveAsync(SslStream socket, byte[] buffer, CancellationToken cancellationToken = default)
        {
            using (var timeout = new CancellationTokenSource(Settings.IoTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                try
                {
                    if (cts.IsCancellationRequested)
                    {
                        return 0;
                    }
                    return await ReadAsync(socket, buffer, cts.Token);
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


        public async Task AuthenticateAsServerAsync(SslStream socket, SslServerAuthenticationOptions options, CancellationToken cancellationToken = default)
        {
            using (var timeout = new CancellationTokenSource(Settings.IoTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                try
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                    await socket.AuthenticateAsServerAsync(options, cancellationToken: cts.Token);
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

        public async Task AuthenticateAsClientAsync(SslStream socket, SslClientAuthenticationOptions options, CancellationToken cancellationToken = default)
        {
            using (var timeout = new CancellationTokenSource(Settings.IoTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                try
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }
                    await socket.AuthenticateAsClientAsync(options, cancellationToken: cts.Token);
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


        public override async Task ListenServerAsync()
        {
            while (true)
            {
                if (!RemoteSocket.CanRead)
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

                int? response = await ReceiveAsync(RemoteSocket, _ServerBuffer);
                if (response.Value > 0)
                {
                    await SendAsync(ClientSocket, _ServerBuffer, response.Value);
                    LastAccessed = _systemClock.UtcNow;
                }
            }
        }

        public override async Task ListenClientAsync()
        {
            while (true)
            {
                if (!ClientSocket.CanRead)
                {
                    _logger.LogError("Client is not connected");
                    //_client.Shutdown(SocketShutdown.Both);
                    //await _client.DisconnectAsync(true);
                    //await CloseSocketAsync(_server);
                    //await CloseSocketAsync(_client);
                    //await CloseSSLStreamAsync(_sslRemoteStream);
                    //break;
                }
                /*if (IsExpired())
                {
                    _logger.LogError("Session is expired!");
                    await CloseSocketAsync(_server);
                    await CloseSSLStreamAsync(_sslRemoteStream);
                    break;
                }*/
                int? response = await ReceiveAsync(ClientSocket, _ClientBuffer);
                if (response.Value > 0)
                {
                    await SendAsync(RemoteSocket, _ClientBuffer, response.Value);
                    LastAccessed = _systemClock.UtcNow;
                }
            }
        }

        public async Task CloseSSLStreamAsync(SslStream sslStream)
        {
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

        public override bool SocketIsSet()
        {
            if (ClientSocket == null || RemoteSocket == null)
            {
                return false;
            }
            return true;
        }

    }
}


