namespace Kamaji.Common
{
    using ionix.Utils.Extensions;
    using System;
    using System.Threading.Tasks;


    public abstract class ResponseModel
    {
        public static Action<Exception> OnException { get; set; } = (ex) =>
        {
            var logger = Utility.CreateLogger("ResultModel", "OnException");
            logger.Error(ex).SaveAsync();
            ConsoleObserver.Instance.Notify("Api_OnException", "An error has benn occurred", ex);
        };

        protected static void OnExceptionDoSomething(Exception ex)//for logging
        {
            OnException?.Invoke(ex);
        }
    }

    public sealed class ResponseModel<T> : ResponseModel
    {
        private T _Data { get; set; }
        public ResponseModel<T> Data(Func<T> func)
        {
            if (null != func)
            {
                try
                {
                    this._Data = func();
                }
                catch (Exception ex)
                {
                    this._Error = ex.FindRoot();
                    OnExceptionDoSomething(this._Error);
                }
            }

            return this;
        }

        public async Task<ResponseModel<T>> DataAsync(Func<Task<T>> func)
        {
            if (null != func)
            {
                try
                {
                    this._Data = await func();
                }
                catch (Exception ex)
                {
                    this._Error = ex.FindRoot();
                    OnExceptionDoSomething(this._Error);
                }
            }

            return this;
        }

        private string _Message { get; set; }
        public ResponseModel<T> Message(string message)
        {
            this._Message = message;
            return this;
        }

        private sealed class Proxy
        {
            public static Proxy From(ResponseModel<T> parent)
            {
                Proxy ret = new Proxy();
                ret.Data = parent._Data;
                ret.Message = parent._Message;
                ret.Error = parent._Error;

                return ret;
            }

            public T Data { get; set; }
            public string Message { get; set; }
            public Exception Error { get; set; }
        }

        private Exception _Error { get; set; }

        public DefaultJsonResult AsJsonResult() => new DefaultJsonResult(Proxy.From(this));
    }
}
