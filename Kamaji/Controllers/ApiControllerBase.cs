namespace Kamaji.Controllers
{
    using Kamaji.Common;
    using Kamaji.Data;
    using System;

    public abstract class ApiControllerBase : ApiController
    {
        protected IKamajiContext Db { get; }

        protected ApiControllerBase(IKamajiContext db)
        {
            this.Db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public override void Dispose(bool isDisposing)
        {
            if (isDisposing)
                this.Db.Dispose();

            base.Dispose(isDisposing);
        }

    }
}
