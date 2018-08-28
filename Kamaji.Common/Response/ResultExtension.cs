namespace Kamaji.Common
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;

    public static class ResultExtensions
    {
        public static DefaultJsonResult Result<T>(this ControllerBase controller, Func<T> data, string message = null)
             => new ResponseModel<T>().Data(data).Message(message).AsJsonResult();


        public static async Task<IActionResult> ResultAsync<T>(this ControllerBase controller, Func<Task<T>> data, string message = null)
             => (await new ResponseModel<T>().DataAsync(data)).Message(message).AsJsonResult();


        public static DefaultJsonResult ResultAsMessage(this ControllerBase controller, string message)
             => new ResponseModel<object>().Message(message).AsJsonResult();
    }
}