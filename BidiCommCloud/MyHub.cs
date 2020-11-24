using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace BidiCommCloud
{
    public class MyHub
        : Hub
    {
        public MyHub(
            HubClientManager hubClientManager,
            ISommaService sommaService
            )
        {
            this._hubClientManager = hubClientManager;
            this._sommaService = sommaService;
        }

        private readonly HubClientManager _hubClientManager;
        private readonly ISommaService _sommaService;
        //private readonly ConcurrentDictionary<string, string> _clientMap = new ConcurrentDictionary<string, string>();
        //private readonly ConcurrentDictionary<Guid, ClientRequestReply> _pendingRequests = new ConcurrentDictionary<Guid, ClientRequestReply>();


        public async Task Hello(string mittente)
        {
            this._hubClientManager.Hello(mittente, this.Context.ConnectionId);
            await Task.CompletedTask;
        }


        public async Task Bye(string mittente)
        {
            this._hubClientManager.Bye(mittente);
            await Task.CompletedTask;
        }


        public async Task<string> RequestReply(Message message)
        {
            await this.Clients.All.SendAsync("ReceiveLog", JsonSerializer.Serialize(message));

            if (message.Destinatario == "server")
            {
                return message.NomeServizio switch
                {
                    "somma" => await this._sommaService.Calcola(message.Payload),
                    _ => throw new NotSupportedException(),
                };
            }
            else
            {
                return await this._hubClientManager.ForwardAsync(
                    this.Clients,
                    message
                    );
            }
        }


        public void ServeResponse(Guid uid, string payload)
        {
            this._hubClientManager.ServeResponse(uid, payload);
        }

    }

    public class Message
    {
        public string Mittente { get; set; }
        public string Destinatario { get; set; }
        public string NomeServizio { get; set; }
        public string Payload { get; set; }
    }
}
