namespace Kamaji.Common
{
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Collections.Generic;

    [Route("api/[controller]/[action]")]
    [ApiController]
    public abstract class ApiController : ControllerBase, IDisposable
    {
        public virtual bool IsModelValid<TEntity>(TEntity model)
        {
            return model.IsModelValid();// EntityMetadaExtensions.IsModelValid(model);
        }

        public virtual bool IsModelValid<TEntity>(IEnumerable<TEntity> modelList)
        {
            return modelList.IsModelListValid();// EntityMetadaExtensions.IsModelListValid(modelList);
        }

        public void Dispose()
        {
            this.Dispose(true);
        }

        public virtual void Dispose(bool isDisposing)
        {

        }

        public JsonResult Json(object data)
        {
            return new DefaultJsonResult(data);
        }
    }
}
