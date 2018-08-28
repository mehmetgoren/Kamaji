namespace Kamaji.Common
{
    using System.Threading.Tasks;


    public interface INanoService
    {
        Task Start();
        Task Stop();
        bool IsRunning { get; }
    }
}
