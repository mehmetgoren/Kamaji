namespace Kamaji.Common
{
    using ionix.Utils.Extensions;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;


    public abstract partial class NanoServiceBase : INanoService, IDisposable
    {
        private static readonly object _syncRefisterTypes = new object();
        private static readonly HashSet<Type> _registeredTypes = new HashSet<Type>();

        private readonly CancellationTokenSource _cancellation;
        protected NanoServiceBase(bool checkSingleton, IObserver observer, TimeSpan interval)
        {
            if (checkSingleton)
            {
                lock(_syncRefisterTypes)
                {
                    Type myType = this.GetType();
                    if (_registeredTypes.Contains(myType))
                        throw new InvalidOperationException($"{myType} must be a singleton type");

                    _registeredTypes.Add(myType);
                }
            }

            this._cancellation = new CancellationTokenSource();
            this.Observer = observer ?? ConsoleObserver.Instance;
            this.Interval = interval;
        }

        protected NanoServiceBase(bool checkSingleton, IObserver observer)
            : this(checkSingleton, observer, TimeSpan.FromMilliseconds(1000)) { }


        private IObserver Observer { get; }

        public TimeSpan Interval { get; set; }

        protected abstract ITaskRunner CreateTaskRunner();

        protected abstract Task Execute(IObserver observer, CancellationToken cancellationToken);//Buradaki cancellation Token iç task lar için.



        public int ExecutedOperationCount { get; private set; } = -1;

        public virtual int MaxOperationLimit { get; set; }

        public virtual int MaxErrorLimit { get; set; } = 10;


        public virtual async Task Start()
        {
            try
            {
                await this.Stop();
            }
            catch (Exception ex)
            {
                await Utility.CreateLogger(nameof(NanoServiceBase), nameof(Start)).Code(911).Error(ex).SaveAsync();
            }



            CancellationToken token = this._cancellation.Token;
            //the Execute method will be called below so if a derived type does not use the cancellationToken then this will be the default behaviour. 
            try
            {
                token.Register(() =>
                {
                    this.IsRunning = false;
                    this.Notify("The service is cancelling.");
                    this.OnStateChangedSafely(ServiceState.Cancelling);
                });
            }
            catch (Exception)
            {
                this.IsRunning = false;
                this.Notify("An exception occurred while cancelling.");
                this.OnStateChangedSafely(ServiceState.ErrorOccuredWhenCancelling);
            }


            this.Notify("The service is startting.");
            this.OnStateChangedSafely(ServiceState.Starting);

            this.IsRunning = true;
            this.ExecutedOperationCount = -1;
            ExceptionCounter exceptionCounter = new ExceptionCounter(this.MaxErrorLimit < 1 ? int.MaxValue : this.MaxErrorLimit);//Burası Konfigure edilebilir.
            ITaskRunner runner = this.CreateTaskRunner();
            while (this.Continue())
            {
                try
                {
                    await runner.Run(this.Execute, this.Observer, token);
                }
                catch (Exception ex)
                {
                    await Utility.CreateLogger(nameof(NanoServiceBase), nameof(Start)).Code(911).Error(ex).SaveAsync();

                    if (exceptionCounter.CheckIfMaxExceptionOccur(ex))
                    {
                        this.IsRunning = false;
                        this.OnStateChangedSafely(ServiceState.FailedDueToMaxErrorExceed);
                        string msg = $"{this.GetType()} has been stopped due to {(this.MaxErrorLimit < 2 ? "an exception" : "a repeated exception.")}";
                        this.Notify(msg);
                        await Utility.CreateLogger(nameof(NanoServiceBase), nameof(Start)).Code(913).Warning(msg).SaveAsync();
                    }
                }

                await Wait(this.Interval.TotalMilliseconds.ConvertTo<int>());
            }

            this.IsRunning = false;//her iştimale karşı. örneğin CompletedDueToMaxOperationLimit' de.

            if (this._currentState == ServiceState.Starting)//yani herhangi bir nedenden durmamış. örneğin once seçilip çalıştırılıp tamamlanmış.
            {
                this.Notify("The service has been completed due to max operation limit.");
                this.OnStateChangedSafely(ServiceState.CompletedDueToMaxOperationLimit);
            }
            //else//Stop dan dolayı yapılmışsa burası zaten çalışıyor. CompletedDueToMaxOperationLimit ve FailedDueToMaxErrorExceed ise zastop edilmeden durmuş oluyor servis zaten.
            //{
            //    this.Notify("The Service has been stopped.");
            //    this.OnStateChangedSafely(ServiceState.Stopped);
            //}
        }



        public virtual async Task Stop()//Stop hiç test edilşmedi, edilsin.
        {
            if (this.IsRunning)
            {
                this.Cancel();

                int addOrminus = 5;
                while (this.IsRunning)
                {
                    await Wait(100 + addOrminus);//Thread.Sleep(1000 + addOrminus);
                    addOrminus *= -1;
                }

                this.Notify("The Service has been stopped.");
                this.OnStateChangedSafely(ServiceState.Stopped);
            }
        }


        public bool IsRunning { get; private set; }

        private bool Continue()
        {
            if (this.IsRunning)
            {
                if (this.MaxOperationLimit > 0)
                {
                    return ++this.ExecutedOperationCount < this.MaxOperationLimit;
                }

                return true;
            }

            return false;
        }


        private static Task Wait(int miliseconds) => Task.Delay(miliseconds);

        private void Cancel()
        {
            Task.Run(() =>
            {
                this._cancellation.Cancel();
                //this.IsRunning = false;
            });
        }

        public void Dispose()
        {
            this._cancellation.Dispose();
            try
            {
                this.OnDisposing();
            }
            catch(Exception ex)
            {
                Utility.CreateLogger(nameof(NanoServiceBase), nameof(Dispose)).Code(915).Error(ex).SaveAsync();
            }
        }
        protected virtual void OnDisposing() { }


        private ServiceState _currentState;
        private void OnStateChangedSafely(ServiceState state)
        {
            this._currentState = state;
           // şimdi eğer stop edilmesi ve düzgünce durmöasını ayırt etmek için current state diye bişf field ekle ve örneğin stopped due to max operation count ise stop yerine completed yazsın
            try
            {
                this.OnStateChanged(state);
            }
            catch (Exception ex)
            {
                Utility.CreateLogger(nameof(NanoServiceBase), nameof(OnStateChangedSafely)).Code(914).Error(ex).SaveAsync();
            }
        }
        protected virtual void OnStateChanged(ServiceState state) { }


        protected void Notify(string message)
        {
            try
            {
                NotifyInfo info = this.NotifyInfoProvider();

                this.Observer?.Notify(info.Key, message, info.Args);
            }
            catch(Exception ex)
            {
                Utility.CreateLogger(nameof(NanoServiceBase), nameof(Notify)).Code(915).Error(ex).SaveAsync();
            }
        }
        private Func<NotifyInfo> _notifyInfoProvider;
        protected Func<NotifyInfo> NotifyInfoProvider
        {
            get
            {
                if (null == this._notifyInfoProvider)
                    this._notifyInfoProvider = new Func<NotifyInfo>(() => new NotifyInfo() { Key = this.GetType().Name });
                return this._notifyInfoProvider;
            }

            set => this._notifyInfoProvider = value;
        }
        protected sealed class NotifyInfo
        {
            public string Key { get; set; }

            public object Args { get; set; }
        }
    }


    public enum ServiceState
    {
        Created=0,
        Starting,


        Stopped,
        Cancelling,
        ErrorOccuredWhenCancelling,
        FailedDueToMaxErrorExceed,
        CompletedDueToMaxOperationLimit
    }
}
