using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;

namespace UKSF.Launcher.Network {
    public class HubWrapper {
        public static async Task<HubConnection> Connecthub(string path) {
            HubConnection hubConnection = new HubConnectionBuilder().WithUrl($"{Global.URL}/hub/{path}").Build();

            return hubConnection;
        }
    }
}
