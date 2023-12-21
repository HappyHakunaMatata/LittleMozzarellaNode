

namespace Node.TunnelExecutors.Models
{
	public static class Settings
	{
        private static int? _bufferSize;
        private const int DefaultBufferSize = 8192;
        public static int BufferSize
        {
            get
            {
                if (_bufferSize != null && _bufferSize.HasValue)
                {
                    return _bufferSize.Value;
                }
                else
                {
                    return DefaultBufferSize;
                }
            }
            set
            {
                if (value <= 0)
                {
                    _bufferSize = DefaultBufferSize;
                }
                else
                {
                    _bufferSize = value;
                }
            }
        }

        /// <summary>
        /// The maximum amount of time allowed to recive/send a request.
        /// </summary>
        private static TimeSpan? _ioTimeout;
        private const int DefaultIoTimeout = 30;
        public static TimeSpan IoTimeout
        {
            get
            {
                if (_ioTimeout != null && _ioTimeout.HasValue)
                {
                    return _ioTimeout.Value;
                }
                else
                {
                    return TimeSpan.FromSeconds(DefaultIoTimeout);
                }
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    _ioTimeout = TimeSpan.FromSeconds(DefaultIoTimeout);
                }
                else
                {
                    _ioTimeout = value;
                }
            }
        }

        /// <summary>
        /// The IdleTimeout indicates how long the session can be idle before its contents are abandoned.
        /// </summary>
        private static TimeSpan? _idleTimeout;
        private const int DefaultIdleTimeout = 2;
        public static TimeSpan IdleTimeout
        {
            get
            {
                if (_idleTimeout != null && _idleTimeout.HasValue)
                {
                    return _idleTimeout.Value;
                }
                else
                {
                    return TimeSpan.FromSeconds(DefaultIdleTimeout);
                }
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    _idleTimeout = TimeSpan.FromSeconds(DefaultIdleTimeout);
                }
                else
                {
                    _idleTimeout = value;
                }
            }
        }

        /// <summary>
        /// The IdleTimeout indicates how long the session can be idle before its contents are abandoned.
        /// </summary>
        private static TimeSpan? _expiredItemsDeletionInterval;
        private const int DefaultExpiredItemsDeletionInterval = 5;
        public static TimeSpan ExpiredItemsDeletionInterval
        {
            get
            {
                if (_expiredItemsDeletionInterval != null && _expiredItemsDeletionInterval.HasValue)
                {
                    return _expiredItemsDeletionInterval.Value;
                }
                else
                {
                    return TimeSpan.FromSeconds(DefaultExpiredItemsDeletionInterval);
                }
            }
            set
            {
                if (value <= TimeSpan.Zero)
                {
                    _expiredItemsDeletionInterval = TimeSpan.FromSeconds(DefaultExpiredItemsDeletionInterval);
                }
                else
                {
                    _expiredItemsDeletionInterval = value;
                }
            }
        }
    }
}

