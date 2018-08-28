namespace Kamaji
{
    using Kamaji.Common;
    using Kamaji.Data;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Primitives;
    using System;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IKamajiContext db)
        {
            if (context.Request.Path.HasValue)
            {
                string[] arr = context.Request.Path.Value.Split('/');// ie "/api/Node/Register" 
                if (arr.Length > 2)
                {
                    string controllerTypeName = new StringBuilder().Append(nameof(Kamaji)).Append('.').Append(nameof(Kamaji.Controllers)).Append('.').Append(arr[2]).Append("Controller").ToString();
                    Type controllerType = Type.GetType(controllerTypeName);
                    if (null != controllerType)
                    {
                        bool isValid = controllerType.GetCustomAttribute<DoNotAuthorizeAttribute>() != null;// doesn't matter if header exist or not.

                        if (!isValid && context.Request.Headers is Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http.HttpRequestHeaders headers)
                        {
                            StringValues values = headers.HeaderAuthorization;
                            string token = values.FirstOrDefault();
                            if (!String.IsNullOrEmpty(token))
                            {
                                arr = token.Split(' ');
                                if (arr.Length == 2 || arr[0] == "Token")//you can later optimize the codes below using cache machanics.
                                {

                                    string tokenValue = Serializer.FromBase64(arr[1]);
                                    Guid guid;
                                    if (Guid.TryParse(tokenValue, out guid))
                                    {
                                        isValid = await db.Authes.GetBy(guid) != null;
                                    }
                                }
                            }
                        }

                        if (isValid)
                        {
                            await _next.Invoke(context);
                            return;
                        }
                    }
                }
            }



            // context.Request.Headers[""]

            // Do something with context near the beginning of request processing.

            //Bunlara bitirince node belli aralıklarla 
            //public virtual float CpuUsage { get; set; }
            //public virtual float MemoryUsage { get; set; }
            //public virtual DateTime LastConnectionTime { get; set; }
            //bu alnları doldursun. Ayrıca burada IObserver ile ekranda da gerçek zamanlı gösterecek alt yapı sağlansın. (Yani web uygulamasıyla kendisini observer ile köprü kursun)

            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Access Denied 401");
        }
    }

    public static class AuthMiddlewareExtensions
    {
        public static IApplicationBuilder UseAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<AuthMiddleware>();
        }
    }
}
