namespace Kamaji.Node
{
    using Kamaji.Common.Models;
    using System;

    using System.Threading.Tasks;

    public sealed partial class KamajiClient
    {
        public static readonly KamajiClient Instance = new KamajiClient();
        private KamajiClient()
        {
            this.Nodes = new Node();
            this.Scans = new Scan();
        }

        public Task<Guid> TakeAToken()
        {
            TakeATokenModel model = new TakeATokenModel() { Address = DataSources.Jsons.AppSettings.Config.Address, Password = "You are right boss" };
            return RestClient.Instance.PostAsync<Guid>("api/Auth/TakeAToken", model);
        }
    }
}
