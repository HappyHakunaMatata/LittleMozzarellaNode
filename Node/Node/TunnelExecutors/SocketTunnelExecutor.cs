using System;
using System.Net.Security;
using System.Net.Sockets;
using System.Threading;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Node.TunnelExecutors.Models;

namespace Node.TunnelExecutors
{
	public class SocketTunnelExecutor : TunnelExecutor
	{
        private Socket? _RemoteStream;
        private Socket? _ClientStream;

        public Socket RemoteSocket
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_RemoteStream);
                return _RemoteStream;
            }
            set
            {
                _RemoteStream = value;
            }
        }

        public Socket ClientSocket
        {
            get
            {
                ArgumentNullException.ThrowIfNull(_ClientStream);
                return _ClientStream;
            }
            set
            {
                _ClientStream = value;
            }
        }

        public SocketTunnelExecutor(ILogger logger): base(logger)
        {
        }

        public void SetSockets(TunnelStructure tunnel)
        {
            ClientSocket = tunnel.client;
            RemoteSocket = tunnel.remote;
        }

        public async Task<int> ReceiveAsync(Socket socket, byte[] buffer, CancellationToken cancellationToken = default)
        {
            using(var timeout = new CancellationTokenSource(Settings.IoTimeout))
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token, cancellationToken);
                try
                {
                    if (cts.IsCancellationRequested)
                    {
                        return 0;
                    }
                    if (socket.Connected)
                    {
                        return await socket.ReceiveAsync(buffer, SocketFlags.None, cts.Token);
                    }
                    return 0;
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


        public async Task<int> SendAsync(Socket socket, byte[] buffer, int response, CancellationToken cancellationToken = default)
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
                    if (socket.Connected)
                    {
                        return await socket.SendAsync(new ArraySegment<byte>(buffer, 0, response), SocketFlags.None, cts.Token);
                    }
                    return 0;
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

        private async Task DisconnectAsync(Socket socket, bool reuseSocket = false, CancellationToken cancellationToken = default)
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
                    await socket.DisconnectAsync(reuseSocket);
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

        public async Task CloseSocketAsync(Socket socket,
            SocketShutdown socketShutdown = SocketShutdown.Both, bool reuseSocket = false)
        {
            try
            {
                if (socket.Connected)
                {
                    socket.Shutdown(socketShutdown);
                    await DisconnectAsync(socket, reuseSocket);
                }
                socket.Close();
                socket.Dispose();
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            finally
            {
                socket.Close();
                socket.Dispose();
            }
        }


        public override async Task ListenClientAsync()
        {
            while (true)
            {
                
                /*if (IsExpired())
                {
                    _logger.LogError("Session is expired!");
                    await CloseSocketAsync(_server);
                    await CloseSSLStreamAsync(_sslRemoteStream);
                    break;
                }*/
                int response = await ReceiveAsync(ClientSocket, _ClientBuffer, _cancellationToken);
                if (RemoteSocket.Connected && response > 0)
                {
                    await SendAsync(RemoteSocket, _ClientBuffer, response);
                    LastAccessed = _systemClock.UtcNow;
                }
            }
        }

        public override async Task ListenServerAsync()
        {
            while (true)
            {
                
                /*if (IsExpired())
                {
                    _logger.LogError("Session is expired!");
                    await CloseSocketAsync(_server);
                    await CloseSSLStreamAsync(_sslRemoteStream);
                    break;
                }*/
                int response = await ReceiveAsync(RemoteSocket, _ServerBuffer, _cancellationToken);
                if (ClientSocket.Connected && response > 0)
                {
                    await SendAsync(ClientSocket, _ServerBuffer, response);
                    LastAccessed = _systemClock.UtcNow;
                }
                if (!RemoteSocket.Connected)
                {
                    _cancellationTokenSource.Cancel();
                }
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

