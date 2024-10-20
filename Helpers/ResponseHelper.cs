using Microsoft.AspNetCore.Mvc;

namespace EasyContinuity_API.Helpers
{
    public static class ResponseHelper
    {
        public static ActionResult<T> HandleErrorAndReturn<T>(Response<T> result)
        {
            if (result.IsSuccess)
            {
                return new OkObjectResult(result.Data);
            }

            return new BadRequestObjectResult(new ProblemDetails { Status = result.ErrorCode, Title = result.ErrorMessage });
        }
    }
}