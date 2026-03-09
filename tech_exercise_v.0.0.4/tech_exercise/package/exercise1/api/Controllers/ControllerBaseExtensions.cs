using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace StargateAPI.Controllers
{
    public static class ControllerBaseExtensions
    {
        public static IActionResult GetResponse(this ControllerBase controllerBase, BaseResponse response)
        {
            var httpResponse = new ObjectResult(response);
            httpResponse.StatusCode = response.ResponseCode;
            return httpResponse;
        }

        /// <summary>
        /// Logs the full exception and returns a safe response for the client.
        /// Use in catch blocks to avoid exposing internal details (stack traces, implementation details).
        /// </summary>
        public static BaseResponse ToSafeResponse(this Exception ex, ILogger logger)
        {
            logger.LogError(ex, "Request failed: {Message}", ex.Message);

            if (ex is BadHttpRequestException badReq)
            {
                return new BaseResponse
                {
                    Message = badReq.Message,
                    Success = false,
                    ResponseCode = badReq.StatusCode
                };
            }

            if (ex is InvalidOperationException invOp)
            {
                var code = invOp.Message.Contains("not found", StringComparison.OrdinalIgnoreCase)
                    ? (int)HttpStatusCode.NotFound
                    : invOp.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase)
                        ? (int)HttpStatusCode.Conflict
                        : (int)HttpStatusCode.BadRequest;
                return new BaseResponse
                {
                    Message = invOp.Message,
                    Success = false,
                    ResponseCode = code
                };
            }

            return new BaseResponse
            {
                Message = "An error occurred while processing your request.",
                Success = false,
                ResponseCode = (int)HttpStatusCode.InternalServerError
            };
        }
    }
}