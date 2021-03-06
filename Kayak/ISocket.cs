﻿using System;
using System.Net;
using System.Threading.Tasks;

namespace Kayak
{
    /// <summary>
    /// Represents a socket which supports asynchronous IO operations.
    /// </summary>
    public interface ISocket : IDisposable
    {
        /// <summary>
        /// The IP end point of the connected peer.
        /// </summary>
        IPEndPoint RemoteEndPoint { get; }

        /// <summary>
        /// Returns an observable which, upon subscription, begins an asynchronous write
        /// operation. When the operation completes, the observable yields the number of
        /// bytes written and completes.
        /// </summary>
        Task<int> Write(byte[] buffer, int offset, int count);

        /// <summary>
        /// Returns an observable which, upon subscription, begins copying a file
        /// to the socket. When the copy operation completes, the observable completes.
        /// </summary>
        Task WriteFile(string file);

        /// <summary>
        /// Returns an observable which, upon subscription, begins an asynchronous read
        /// operation. When the operation completes, the observable yields the number of
        /// bytes read and completes.
        /// </summary>
        Task<int> Read(byte[] buffer, int offset, int count);
    }

}
