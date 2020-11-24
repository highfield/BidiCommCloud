using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BidiCommCloud
{
    public interface ISommaService
    {
        Task<string> Calcola(string payload);
    }

    public class SommaService
        : ISommaService
    {
        async Task<string> ISommaService.Calcola(string payload)
        {
            var jinput = JObject.Parse(payload);
            var a = (double)jinput["a"];
            var b = (double)jinput["b"];

            await Task.Delay(1000); //simula impegno

            var joutput = new JObject()
            {
                ["result"] = a + b,
            };
            return joutput.ToString();
        }
    }
}
