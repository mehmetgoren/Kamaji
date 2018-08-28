namespace Kamaji.Controllers
{
    using Kamaji.Common;
    using Kamaji.Common.Models;
    using Kamaji.Data;
    using Kamaji.Data.Models;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Threading.Tasks;

    [DoNotAuthorize]
    public class AuthController : ApiControllerBase
    {
        public AuthController(IKamajiContext db)
            : base(db) { }

        [HttpPost]
        public Task<IActionResult> TakeAToken(TakeATokenModel model)
        {
            return this.ResultAsync(async () =>
            {
                Guid? ret = null;
                if (model.IsModelValid() && model.Password == "You are right boss")
                {
                    IAuthModel auth = await this.Db.Authes.GetBy(model.Address);
                    if (null == auth)
                    {
                        auth = this.Db.ModelFactory.CreateAuthModel();
                        auth.Address = model.Address;
                        auth.Token = Guid.NewGuid();

                        await this.Db.Authes.Save(auth);
                    }

                    ++auth.LoginCount;
                    await this.Db.Authes.EditLoginCount(auth.AuthId, auth.LoginCount);

                    ret = auth.Token;
                }

                return ret;
            });
        }
    }
}
