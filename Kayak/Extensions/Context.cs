﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Kayak
{
    public static partial class Extensions
    {
        public static IObservable<IKayakContext> ToContexts(this IObservable<ISocket> sockets)
        {
            return sockets.ToContexts(s => s.CreateContext());
        }

        public static IObservable<IKayakContext> ToContexts(this IObservable<ISocket> sockets, Func<ISocket, IObservable<IKayakContext>> transform)
        {
            return Observable.CreateWithDisposable<IKayakContext>(o => sockets.Subscribe(s =>
                    {
                        transform(s).Subscribe(c =>
                            {
                                o.OnNext(c);
                            },
                            e =>
                            {
                                Console.WriteLine("Exception while creating context!");
                                Console.Out.WriteException(e);
                                s.Dispose();
                            });
                    },
                    e =>
                    {
                        o.OnError(e);
                    },
                    () =>
                    {
                        o.OnCompleted();
                    })
            );
        }

        public static IObservable<IKayakContext> CreateContext(this ISocket socket)
        {
            if (socket == null)
                throw new ArgumentNullException("socket");

            return CreateContextInternal(socket).AsCoroutine<IKayakContext>();
        }

        static IEnumerable<object> CreateContextInternal(ISocket socket)
        {
            KayakServerRequest request = null;
            yield return socket.CreateRequest().Do(r => request = r);

            var response = new KayakServerResponse(socket);

            yield return new KayakContext(socket, request, response);
        }

        public static void End(this IKayakContext context)
        {
            // TODO this is wrong.
            context.End(context.Response.Headers.GetContentLength() <= 0);
        }

        public static void End(this IKayakContext context, bool writeHeaders)
        {
            EndInternal(context, writeHeaders).AsCoroutine<Unit>().Subscribe(u => { }, e => 
            {
                Console.WriteLine("Exception while ending context!");
                Console.Out.WriteException(e);
            });
        }

        static IEnumerable<object> EndInternal(IKayakContext context, bool writeHeaders)
        {
            if (writeHeaders)
            {
                var headers = context.Response.CreateHeaderBuffer();
                yield return context.Socket.Write(headers, 0, headers.Length);
            }

            // close connection, we only support HTTP/1.0
            context.Socket.Dispose();

            Console.WriteLine("[{0}] {1} {2} {3} : {4} {5} {6}", DateTime.Now,
                context.Request.Verb, context.Request.Path, context.Request.HttpVersion,
                context.Response.HttpVersion, context.Response.StatusCode, context.Response.ReasonPhrase);
        }
    }
}