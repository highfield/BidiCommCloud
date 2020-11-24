using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BidiCommCloud
{
    public class HubClientManager
    {

        private readonly ConcurrentDictionary<string, string> _clientMap = new ConcurrentDictionary<string, string>();
        private readonly ConcurrentDictionary<Guid, ClientRequestReply> _pendingRequests = new ConcurrentDictionary<Guid, ClientRequestReply>();


        public void Hello(string mittente, string connId)
        {
            this._clientMap[mittente] = connId;
        }


        public void Bye(string mittente)
        {
            this._clientMap.TryRemove(mittente, out _);
        }


        public async Task<string> ForwardAsync(IHubCallerClients clients, Message message)
        {
            if (this._clientMap.TryGetValue(message.Destinatario, out string connId))
            {
                var crr = new ClientRequestReply(clients.Client(connId));
                try
                {
                    this._pendingRequests.TryAdd(crr.Uid, crr);
                    return await crr.SendRequest(message);
                }
                finally
                {
                    this._pendingRequests.TryRemove(crr.Uid, out _);
                }
            }
            else
            {
                throw new KeyNotFoundException();
            }
        }


        public void ServeResponse(Guid uid, string payload)
        {
            if (this._pendingRequests.TryGetValue(uid, out ClientRequestReply crr))
            {
                crr.ServeResponse(payload);
            }
        }


        private class ClientRequestReply
        {
            public ClientRequestReply(IClientProxy proxy)
            {
                this._proxy = proxy;
            }

            private readonly IClientProxy _proxy;
            private readonly SemaphoreSlim _semaf = new SemaphoreSlim(0, 1);
            private string _response;
            public Guid Uid { get; } = Guid.NewGuid();

            public async Task<string> SendRequest(Message message)
            {
                await this._proxy.SendAsync(
                        "RequestReply",
                        this.Uid,
                        message
                        );

                await this._semaf.WaitAsync();
                return this._response;
            }

            public void ServeResponse(string payload)
            {
                this._response = payload;
                this._semaf.Release();
            }
        }

    }
}
