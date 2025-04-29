using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace SurveySystem.Web.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public string RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        public string ErrorMessage { get; set; }

        public void OnGet()
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

            var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                var exception = exceptionHandlerPathFeature.Error;
                ErrorMessage = exception.Message;

                _logger.LogError(exception,
                    "An unhandled exception occurred on path: {Path}. Request ID: {RequestId}",
                    exceptionHandlerPathFeature.Path,
                    RequestId);
            }
        }
    }
}

//using System.Diagnostics;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.RazorPages;

//namespace SurveySystem.Web.Pages;

//[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//[IgnoreAntiforgeryToken]
//public class ErrorModel : PageModel
//{
//    public string? RequestId { get; set; }

//    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

//    private readonly ILogger<ErrorModel> _logger;

//    public ErrorModel(ILogger<ErrorModel> logger)
//    {
//        _logger = logger;
//    }

//    public void OnGet()
//    {
//        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
//    }
//}

