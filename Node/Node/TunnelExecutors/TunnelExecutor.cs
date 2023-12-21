using System.Net.Sockets;
using Microsoft.Extensions.Internal;
using Microsoft.Extensions.Logging;
using Node.TunnelExecutors.Models;

namespace Node.TunnelExecutors
{
	public abstract class TunnelExecutor : IDisposable
    {
        protected readonly ILogger _logger;
        protected ISystemClock _systemClock;
        protected DateTimeOffset LastAccessed;
        
        
        protected bool IsDisposed { get; private set; }
        protected byte[] _ServerBuffer = new byte[Settings.BufferSize];
        protected byte[] _ClientBuffer = new byte[Settings.BufferSize];

        protected readonly CancellationTokenSource _cancellationTokenSource;
        protected readonly CancellationToken _cancellationToken;

        public TunnelExecutor(ILogger logger) {
            ArgumentNullException.ThrowIfNull(logger);
            _logger = logger;
            _systemClock = new SystemClock();
            LastAccessed = _systemClock.UtcNow;
            _cancellationTokenSource = new CancellationTokenSource();
            _cancellationToken = _cancellationTokenSource.Token;
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


        public async Task ConnectAsync(Socket socket, System.Net.EndPoint endPoint, CancellationToken cancellationToken = default)
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
                    await socket.ConnectAsync(endPoint, cancellationToken: cts.Token);
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

        private void Log(string logMessage)
        {
            using (StreamWriter w = File.AppendText("log.txt"))
            {
                w.WriteLine(logMessage);
            }
        }


        public abstract Task ListenClientAsync();
        public abstract Task ListenServerAsync();

        public abstract bool SocketIsSet();


        public void StartTunneling()
        {
            if (!SocketIsSet())
            {
                _logger.LogInformation("Sockets are not set");
                return;
            }
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
               //_server.Dispose();
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

