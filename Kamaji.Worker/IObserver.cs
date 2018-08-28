namespace Kamaji.Worker
{
    using System;

    //web socket de gelecek.
    //Kamaji' ye referan verilmemsi için bu şekilde yapıldı.
    public interface IObserver
    {
        void Notify(string id, string message, object args);
    }
}
