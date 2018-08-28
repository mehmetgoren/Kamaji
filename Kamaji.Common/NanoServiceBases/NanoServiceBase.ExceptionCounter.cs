namespace Kamaji.Common
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    partial class NanoServiceBase
    {
        private sealed class ExceptionCounter
        {
            internal int MaxExceptionOccurNumber { get; }
            private readonly IDictionary<Type, int> _exceptionTypes = new ConcurrentDictionary<Type, int>();

            internal ExceptionCounter(int maxExceptionOccurNumber)
            {
                this.MaxExceptionOccurNumber = maxExceptionOccurNumber; 
            }

            internal bool CheckIfMaxExceptionOccur(Exception ex)
            {
                Type exceptionType = ex.GetType();

                this._exceptionTypes.TryGetValue(exceptionType, out int number);
                this._exceptionTypes[exceptionType] = ++number;

                return number >= this.MaxExceptionOccurNumber;
            }
        }
    }
}
