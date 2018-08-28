namespace Kamaji.Worker
{
    using System;

    public struct WorkerResult
    {
        public WorkerResult(object result, bool success, string failedReason)
        {
            this.Result = result;
            this.Success = success;
            this.FailedReason = failedReason;
        }

        public WorkerResult(object result)
            : this(result, true, null) { }

        public object Result { get; }

        public bool Success { get; }

        public string FailedReason { get; }
    }
}
