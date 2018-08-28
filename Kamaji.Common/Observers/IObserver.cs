namespace Kamaji.Common
{
    using System;

    //web socket de gelecek.
    public interface IObserver
    {
        void Notify(string id, string message, object args);
    }
}
