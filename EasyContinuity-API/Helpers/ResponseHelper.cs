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

            var problem = new ProblemDetails
            {
                Status = result.StatusCode,
                Title = result.Message,
                Detail = result.Message
            };

            return result.StatusCode switch
            {
                404 => new NotFoundObjectResult(problem),
                400 => new BadRequestObjectResult(problem),
                422 => new UnprocessableEntityObjectResult(problem),
                _ => new ObjectResult(problem) { StatusCode = result.StatusCode }
            };
        }
    }
}