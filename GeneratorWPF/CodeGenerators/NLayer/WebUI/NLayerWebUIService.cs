using GeneratorWPF.Extensions;
using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Linq;

namespace GeneratorWPF.CodeGenerators.NLayer.WebUI;

public class NLayerWebUIService
{
    private readonly EntityRepository _entityRepository;
    private readonly FieldRepository _fieldRepository;
    private readonly DtoRepository _dtoRepository;
    private readonly AppSetting _appSetting;
    public NLayerWebUIService(AppSetting appSetting)
    {
        _appSetting = appSetting;
        _entityRepository = new();
        _fieldRepository = new();
        _dtoRepository = new();
    }

    public string CreateProject(string path, string solutionName)
    {
        try
        {
            string projectPath = Path.Combine(path, "WebUI");
            string csprojPath = Path.Combine(projectPath, "WebUI.csproj");

            if (Directory.Exists(projectPath) && File.Exists(csprojPath))
                return "INFO: WebUI layer project already exists.";

            RunCommand(path, "dotnet", $"new mvc -n WebUI");
            RunCommand(path, "dotnet", $"sln {solutionName}.sln add WebUI/WebUI.csproj");
            RunCommand(projectPath, "dotnet", $"add reference ../Business/Business.csproj");

            RemoveFile(projectPath, "Program.cs");
            RemoveFile(projectPath, "appsettings.json");

            return "OK: WebUI Project Created Successfully";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while creating the WebUI project. \n\t Details:{ex.Message}");
        }
    }

    #region Package Methods
    public string AddPackage(string path, string packageName)
    {
        try
        {
            string projectPath = Path.Combine(path, "WebUI");
            string csprojPath = Path.Combine(projectPath, "WebUI.csproj");

            if (!File.Exists(csprojPath))
                throw new FileNotFoundException($"WebUI.csproj not found for adding package({packageName}).");

            var doc = XDocument.Load(csprojPath);

            var packageAlreadyAdded = doc.Descendants("PackageReference").Any(p => p.Attribute("Include")?.Value == packageName);

            if (packageAlreadyAdded)
                return $"INFO: Package {packageName} already exists in WebUI project.";

            RunCommand(projectPath, "dotnet", $"add package {packageName}");

            return $"OK: Package {packageName} added to WebUI project.";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding pacgace to WebUI project. \n\t Details:{ex.Message}");
        }
    }
    public string Restore(string path)
    {
        try
        {
            string projectPath = Path.Combine(path, "WebUI");

            RunCommand(projectPath, "dotnet", "restore");
            return "OK: Restored WebUI project.";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while restoring WebUI project. \n Details:{ex.Message}");
        }
    }
    #endregion

    #region Static Files
    public string GenerateUtils(string solutionPath)
    {
        List<string> results = new List<string>();
        string code_ValidationFilter = @"using Core.Enums;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace WebUI.Utils.ActionFilters;

public class ValidationFilter<TModel> : ActionFilterAttribute
{
    private readonly IEnumerable<IValidator<TModel>> _validators;
    public ValidationFilter(IEnumerable<IValidator<TModel>> validators) => _validators = validators;

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!_validators.Any()) return;

        var model = (TModel?)context.ActionArguments.Values.FirstOrDefault(f => f.GetType() == typeof(TModel));
        if (model == null) return;

        IEnumerable<FluentValidation.Results.ValidationFailure> validationFailures = _validators
            .Select(validator => validator.Validate(model))
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors)
            .ToList();

        if (!validationFailures.Any()) return;

        var isJsonRequest =
            context.HttpContext.Request.Headers[""Accept""].ToString().Contains(""application/json"") ||
            context.HttpContext.Request.Headers[""X-Requested-With""].ToString().Contains(""XMLHttpRequest"") ||
            context.HttpContext.Request.ContentType?.Contains(""application/json"") == true;


        if (isJsonRequest)
        {
            var problemDetails = new Core.Utils.ExceptionHandle.ProblemDetailModels.ValidationProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Type = ProblemDetailTypes.Validation.ToString(),
                Title = ""Validation error(s)"",
                Detail = ""One or more validation errors occurred."",
                Errors = validationFailures
            };

            context.Result = new BadRequestObjectResult(problemDetails);
        }
        else
        {
            foreach (var failure in validationFailures)
            {
                context.ModelState.AddModelError(failure.PropertyName, failure.ErrorMessage);
            }

            string? action = context.ActionDescriptor.RouteValues[""action""];
            context.Result = new ViewResult
            {
                ViewName = action,
                ViewData = new ViewDataDictionary(metadataProvider: new EmptyModelMetadataProvider(), modelState: context.ModelState)
                {
                    Model = model
                }
            };
        }
    }
}";

        string code_HttpContextExtensions = @"using System.Text;

namespace WebUI.Utils.Extensions;

public static class HttpContextExtensions
{
    public static bool IsJsonRequest(this HttpContext httpContext)
    {
        return
            httpContext.Request.Headers[""X-Request-Provider""].ToString().ToLowerInvariant() == ""json"" ||
            httpContext.Request.Headers[""Accept""].ToString().Contains(""application/json"") ||
            httpContext.Request.Headers[""X-Requested-With""].ToString().Contains(""XMLHttpRequest"") ||
            httpContext.Request.ContentType?.Contains(""application/json"") == true;
    }

    public static string GetUrl(this HttpContext httpContext)
    {
        string SchemeDelimiter = Uri.SchemeDelimiter;
        var scheme = httpContext.Request.Scheme ?? string.Empty;
        var host = httpContext.Request.Host.Value ?? string.Empty;
        var pathBase = httpContext.Request.PathBase.Value ?? string.Empty;
        var path = httpContext.Request.Path.Value ?? string.Empty;
        var queryString = httpContext.Request.QueryString.Value ?? string.Empty;

        var length = scheme.Length + SchemeDelimiter.Length + host.Length + pathBase.Length + path.Length + queryString.Length;

        return new StringBuilder(length)
            .Append(scheme)
            .Append(SchemeDelimiter)
            .Append(host)
            .Append(pathBase)
            .Append(path)
            .Append(queryString)
            .ToString();
    }

    public static string GetPath(this HttpContext httpContext)
    {
        var path = httpContext.Request.Path.Value ?? string.Empty;

        return path.Trim();
    }

    public static string GetBasePath(this HttpContext httpContext)
    {
        string? controller = httpContext.GetRouteData().Values[""controller""]?.ToString()?.Trim();
        string? action = httpContext.GetRouteData().Values[""action""]?.ToString()?.Trim();

        return $""/{controller}/{action}"";
    }


    public static string GetLightMode(this HttpContext httpContext)
    {
        return httpContext.Request.Cookies[""light_mode""] ?? ""system"";
    }

    public static void SetLightMode(this HttpContext httpContext, string mode)
    {
        httpContext.Response.Cookies.Append(""light_mode"", mode, new CookieOptions
        {
            Expires = DateTime.Now.AddDays(7),
        });
    }
}";

        string folderPathValidationFilter = Path.Combine(solutionPath, "WebUI", "Utils", "ActionFilters");
        string folderPathHttpContextExtensions = Path.Combine(solutionPath, "WebUI", "Utils", "Extensions");

        results.Add(AddFile(folderPathValidationFilter, "ValidationFilter", code_ValidationFilter));
        results.Add(AddFile(folderPathHttpContextExtensions, "HttpContextExtensions", code_HttpContextExtensions));

        return string.Join("\n", results);
    }

    public string GenerateExceptionHandler(string solutionPath)
    {
        string code = @"using Core.Enums;
using Core.Utils.ExceptionHandle.Exceptions;
using Core.Utils.ExceptionHandle.ProblemDetailModels;
using FluentValidation.Results;
using Newtonsoft.Json;
using Serilog;
using WebUI.Utils.Extensions;

namespace WebUI.ExceptionHandler;

public class ExceptionHandleMiddleware
{
    private readonly RequestDelegate _next;
    public ExceptionHandleMiddleware(RequestDelegate next) => _next = next;


    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception e)
        {
            if (context.IsJsonRequest())
            {
                await CatchJsonExceptionAsync(context.Response, e);
            }
            else
            {
                CatchPageException(context.Response, e);
            }
        }
    }


    private void CatchPageException(HttpResponse response, Exception exception)
    {
        //response.Clear();

        Type exceptionType = exception.GetType();

        if (exceptionType == typeof(ValidationRuleException))
        {
            var validException = (ValidationRuleException)exception;
            Log.ForContext(""Target"", ""Validation"").Error(
                $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
                $""Type(Validation) \n"" +
                $""Location: {validException.LocationName} \n"" +
                $""Detail: {validException.Message} \n"" +
                $""Description:{validException.Description} \n"" +
                $""Parameters: {validException.Parameters} \n"" +
                $""------- ------- ------- FINISH ------- ------- -------\n\n"");

            response.Redirect(""/Error/InvalidProcess"");
        }
        else if (exceptionType == typeof(BusinessException))
        {
            var businessException = (BusinessException)exception;
            Log.ForContext(""Target"", ""Business"").Error(
                $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
                $""Type(Business) \n"" +
                $""Location: {businessException.LocationName} \n"" +
                $""Detail: {businessException.Message} \n"" +
                $""Description:{businessException.Description} \n"" +
                $""Parameters: {businessException.Parameters} \n"" +
                $""Exception Raw: \n\n{businessException.ToString()} \n"" +
                $""------- ------- ------- FINISH ------- ------- -------\n\n"");

            response.Redirect(""/Error/InvalidProcess"");
        }
        else if (exceptionType == typeof(GeneralException))
        {
            var generalException = (GeneralException)exception;
            Log.ForContext(""Target"", ""Application"").Error(
                $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
                $""Type(General) \n"" +
                $""Location: {generalException.LocationName} \n"" +
                $""Detail: {generalException.Message} \n"" +
                $""Description:{generalException.Description} \n"" +
                $""Parameters: {generalException.Parameters} \n"" +
                $""Exception Raw: \n\n{generalException.ToString()} \n"" +
                $""------- ------- ------- FINISH ------- ------- -------\n\n"");

            response.Redirect(""/Error/InternalServer"");
        }
        else if (exceptionType == typeof(DataAccessException))
        {
            var dataAccessException = (DataAccessException)exception;
            Log.ForContext(""Target"", ""DataAccess"").Error(
                $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
                $""Type(DataAccess) \n"" +
                $""Location: {dataAccessException.LocationName} \n"" +
                $""Detail: {dataAccessException.Message} \n"" +
                $""Description:{dataAccessException.Description} \n"" +
                $""Parameters: {dataAccessException.Parameters} \n"" +
                $""Exception Raw: \n\n{dataAccessException.ToString()} \n"" +
                $""------- ------- ------- FINISH ------- ------- -------\n\n"");

            response.Redirect(""/Error/InternalServer"");
        }
        else if (response.StatusCode == 404 && !response.HasStarted)
        {
            Log.Error(
                $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
                $""Type(Not Found) \n"" +
                $""Detail: {exception.Message} \n"" +
                $""Exception Raw: \n\n{exception.ToString()} \n"" +
                $""------- ------- ------- FINISH ------- ------- -------\n\n"");
            response.Redirect(""/Error/NotFound"");
        }
        else
        {
            Log.Error(
                $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
                $""Type(Others) \n"" +
                $""Detail: {exception.Message} \n"" +
                $""Exception Raw: \n\n{exception.ToString()} \n"" +
                $""------- ------- ------- FINISH ------- ------- -------\n\n"");

            response.Redirect(""/Error/InternalServer"");
        }
    }

    private Task CatchJsonExceptionAsync(HttpResponse response, Exception exception)
    {
        response.ContentType = ""application/problem+json"";

        Type exceptionType = exception.GetType();

        if (exceptionType == typeof(ValidationRuleException)) return HandleValidationException(response, (ValidationRuleException)exception);
        if (exceptionType == typeof(DataAccessException)) return HandleDataAccessException(response, (DataAccessException)exception);
        if (exceptionType == typeof(BusinessException)) return HandleBusinessException(response, (BusinessException)exception);
        if (exceptionType == typeof(GeneralException)) return HandleGeneralException(response, (GeneralException)exception);

        return HandleOtherException(response, exception);
    }


    private Task HandleValidationException(HttpResponse response, ValidationRuleException exception)
    {
        Log.ForContext(""Target"", ""Validation"").Error(
            $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
            $""Type(Validation) \n"" +
            $""Location: {exception.LocationName} \n"" +
            $""Detail: {exception.Message} \n"" +
            $""Description:{exception.Description} \n"" +
            $""Parameters: {exception.Parameters} \n"" +
            $""------- ------- ------- FINISH ------- ------- -------\n\n"");

        response.StatusCode = StatusCodes.Status400BadRequest;
        IEnumerable<ValidationFailure> errors = exception.Errors;

        return response.WriteAsync(JsonConvert.SerializeObject(new ValidationProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Type = ProblemDetailTypes.Validation.ToString(),
            Title = ""Validation error(s)"",
            Detail = exception.Message,
            Errors = errors
        }));
    }

    private Task HandleBusinessException(HttpResponse response, BusinessException exception)
    {
        Log.ForContext(""Target"", ""Business"").Error(
            $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
            $""Type(Business) \n"" +
            $""Location: {exception.LocationName} \n"" +
            $""Detail: {exception.Message} \n"" +
            $""Description:{exception.Description} \n"" +
            $""Parameters: {exception.Parameters} \n"" +
            $""Exception Raw: \n\n{exception.ToString()} \n"" +
            $""------- ------- ------- FINISH ------- ------- -------\n\n"");

        response.StatusCode = StatusCodes.Status409Conflict;

        return response.WriteAsync(JsonConvert.SerializeObject(new BusinessProblemDetails
        {
            Status = StatusCodes.Status409Conflict,
            Type = ProblemDetailTypes.Business.ToString(),
            Title = ""Business Workflow Exception"",
            Detail = exception.Message
        }));
    }

    private Task HandleDataAccessException(HttpResponse response, DataAccessException exception)
    {
        Log.ForContext(""Target"", ""DataAccess"").Error(
            $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
            $""Type(DataAccess) \n"" +
            $""Location: {exception.LocationName} \n"" +
            $""Detail: {exception.Message} \n"" +
            $""Description:{exception.Description} \n"" +
            $""Parameters: {exception.Parameters} \n"" +
            $""Exception Raw: \n\n{exception.ToString()} \n"" +
            $""------- ------- ------- FINISH ------- ------- -------\n\n"");

        response.StatusCode = StatusCodes.Status500InternalServerError;

        return response.WriteAsync(JsonConvert.SerializeObject(new DataAccessProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = ProblemDetailTypes.DataAccess.ToString(),
            Title = ""Data Access Exception"",
            Detail = ""An error occurred during the process"",
        }));
    }

    private Task HandleGeneralException(HttpResponse response, GeneralException exception)
    {
        Log.ForContext(""Target"", ""Application"").Error(
            $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
            $""Type(General) \n"" +
            $""Location: {exception.LocationName} \n"" +
            $""Detail: {exception.Message} \n"" +
            $""Description:{exception.Description} \n"" +
            $""Parameters: {exception.Parameters} \n"" +
            $""Exception Raw: \n\n{exception.ToString()} \n"" +
            $""------- ------- ------- FINISH ------- ------- -------\n\n"");

        response.StatusCode = StatusCodes.Status500InternalServerError; // 500

        return response.WriteAsync(JsonConvert.SerializeObject(new Microsoft.AspNetCore.Mvc.ProblemDetails()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = ProblemDetailTypes.General.ToString(),
            Title = ""General exception"",
            Detail = ""An error occurred during the process""
        }));
    }

    private Task HandleOtherException(HttpResponse response, Exception exception)
    {
        Log.Error(
            $""\n\n------- ------- ------- Start ------- ------- ------- \n"" +
            $""Type(Others) \n"" +
            $""Detail: {exception.Message} \n"" +
            $""Exception Raw: \n\n{exception.ToString()} \n"" +
            $""------- ------- ------- FINISH ------- ------- -------\n\n"");

        response.StatusCode = StatusCodes.Status500InternalServerError; // 500

        return response.WriteAsync(JsonConvert.SerializeObject(new Microsoft.AspNetCore.Mvc.ProblemDetails()
        {
            Status = StatusCodes.Status500InternalServerError,
            Type = ProblemDetailTypes.General.ToString(),
            Title = ""General exception"",
            Detail = ""An error occurred during the process""
        }));
    }
}";

        string folderPath = Path.Combine(solutionPath, "WebUI", "ExceptionHandler");
        return AddFile(folderPath, "ExceptionHandleMiddleware", code);
    }

    public string GenerateSideMenuViewComponent(string solutionPath)
    {
        string section1 = @"
using Microsoft.AspNetCore.Mvc;
using WebUI.Models.UI;

namespace WebUI.ViewComponents;

public class SideMenuViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var menuItems = new List<MenuItem>()
        {
            new MenuItem
            {
                Title = ""Dashboard"",
                Icon = ""fa-brands fa-magento"",
                Path = ""/Home/Index"",
                Type = 1,
            },";

        string section2 = @"
        };
        return View(menuItems);
    }
}";

        StringBuilder stringBuilder = new StringBuilder();

        List<Entity> entities = _entityRepository.GetAll();

        foreach (Entity entity in entities)
        {
            string groupName = entities.FindIndex(f => f.Id == entity.Id) == 0 ? "GroupName = \"Pages\"," : "";

            stringBuilder.AppendLine($@"
      new MenuItem
            {{
                Title = ""{entity.Name}"",
                Icon = ""fa-regular fa-folder-open"",
                Type = 0,
                {groupName}
                SubMenuItems = new List<MenuItem>()
                {{
                    new MenuItem
                    {{
                        Title = ""Managment"",
                        Icon = ""fa-regular fa-file-lines"",
                        Path = ""/{entity.Name}/Index"",
                        Type = 1,
                    }},
                    new MenuItem
                    {{
                        Title = ""Create"",
                        Icon = ""fa-solid fa-file-circle-plus"",
                        Path= ""/{entity.Name}/Create"",
                        Type = 1,
                    }}
                }}
            }},");
        }
        string code = section1 + stringBuilder.ToString() + section2;
        string folderPathMenuItem = Path.Combine(solutionPath, "WebUI", "ViewComponents");
        return AddFile(folderPathMenuItem, "SideMenuViewComponent", code);
    }
    #endregion

    public string Generate_wwwroot(string solutionPath)
    {
        // ........
        return "wwwroot generated";
    }

    public string GenerateViewModels(string solutionPath)
    {
        var results = new List<string>();

        RoslynWebUIViewModelGenerator roslynWebUIControllerGenerator = new RoslynWebUIViewModelGenerator(_appSetting);

        var entities = _entityRepository.GetAll(f => f.Control == false);

        foreach (var entity in entities)
        {
            string code_VMIndex = roslynWebUIControllerGenerator.GenerateViewModelIndex(entity);
            string code_VMCreate = roslynWebUIControllerGenerator.GenerateViewModelCreate(entity);
            string code_VMUpdate = roslynWebUIControllerGenerator.GenerateViewModelUpdate(entity);

            string folderPath = Path.Combine(solutionPath, "WebUI", "Models", "ViewModels", entity.Name);

            results.Add(AddFile(folderPath, $"{entity.Name}ViewModel", code_VMIndex));
            results.Add(AddFile(folderPath, $"{entity.Name}CreateViewModel", code_VMCreate));
            results.Add(AddFile(folderPath, $"{entity.Name}UpdateViewModel", code_VMUpdate));
        }

        #region UI MenuItem
        string code_MenuItem = @"namespace WebUI.Models.UI;

public class MenuItem
{
    public string Title { get; set; } = ""Menu Item"";
    public int Type { get; set; } // 0 = group, 1 = route
    public string? Path { get; set; }
    public string Icon { get; set; } = string.Empty;
    public string GroupName { get; set; } = string.Empty;
    public string? Description { get; set; }

    public List<MenuItem>? SubMenuItems { get; set; }
}";

        string folderPathMenuItem = Path.Combine(solutionPath, "WebUI", "Models", "UI");
        results.Add(AddFile(folderPathMenuItem, "MenuItem", code_MenuItem));
        #endregion

        return string.Join("\n", results);
    }

    public string GenerateControllers(string solutionPath)
    {
        var results = new List<string>();

        string folderPath = Path.Combine(solutionPath, "WebUI", "Controllers");

        RoslynWebUIControllerGenerator roslynWebUIControllerGenerator = new RoslynWebUIControllerGenerator(_appSetting);

        var entities = _entityRepository.GetAll(f => f.Control == false, include: i => i.Include(x => x.Fields));

        foreach (var entity in entities)
        {
            var dtos = _dtoRepository.GetAll(
                filter: f => f.RelatedEntityId == entity.Id,
                include: i => i
                    .Include(x => x.DtoFields).ThenInclude(x => x.SourceField)
                    .Include(x => x.RelatedEntity).ThenInclude(ti => ti.Fields));

            string code_controller = roslynWebUIControllerGenerator.GeneraterController(entity, dtos);

            results.Add(AddFile(folderPath, $"{entity.Name}Controller", code_controller));
        }

        // Accout Controller
        if (_appSetting.IsThereIdentiy)
        {
            string code_AccountController = @"using Business.Abstract;
using Core.Utils.ExceptionHandle.Exceptions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Model.Auth.Login;
using Model.Auth.SignUp;
using WebUI.Utils.ActionFilters;

namespace WebUI.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _authService;
        public AccountController(IAuthService authService)
        {
            _authService = authService;
        }


        [HttpGet]
        public IActionResult Login()
        {
            var model = new LoginRequest();
            return View(model);
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilter<LoginRequest>))]
        public async Task<IActionResult> Login(LoginRequest loginRequest)
        {
            try
            {
                await _authService.LoginWebBaseAsync(loginRequest);

                return RedirectToAction(""Index"", ""Home"");
            }
            catch (Exception ex)
            {
                Type exType = ex.GetType();
                if (exType == typeof(BusinessException))
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ""İşlem Sırasında Bir Sorun Oluştu. Lütfen Daha Sonra Tekrar Deneyiniz!"");
                }
                return View(loginRequest);
            }
        }

        [HttpGet]
        public IActionResult SignUp()
        {
            return View();
        }

        [HttpPost]
        [ServiceFilter(typeof(ValidationFilter<SignUpRequest>))]
        public async Task<IActionResult> SignUp(SignUpRequest signUpRequest)
        {
            try
            {
                await _authService.SignUpWebBaseAsync(signUpRequest);

                return RedirectToAction(""Index"", ""Home"");
            }
            catch (Exception ex)
            {
                Type exType = ex.GetType();
                if (exType == typeof(BusinessException))
                {
                    ModelState.AddModelError(string.Empty, ex.Message);
                }
                else
                {
                    ModelState.AddModelError(string.Empty, ""İşlem Sırasında Bir Sorun Oluştu. Lütfen Daha Sonra Tekrar Deneyiniz!"");
                }
                return View();
            }
        }

        [HttpGet]
        public async Task<IActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction(""Login"", ""Account"");
        }
    }
}";

            results.Add(AddFile(folderPath, "AccountController", code_AccountController));
        }

        // Error Controller
        string code_ErrorController = @"using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class ErrorController : Controller
    {
        public IActionResult Forbidden()
        {
            return View();
        }

        public IActionResult InternalServer()
        {
            return View();
        }

        public IActionResult InvalidProcess()
        {
            return View();
        }

        public new IActionResult NotFound()
        {
            return View();
        }
    }
}";
        results.Add(AddFile(folderPath, "ErrorController", code_ErrorController));

        // UI Controller
        string code_UIController = @"using Microsoft.AspNetCore.Mvc;
using WebUI.Utils.Extensions;

namespace WebUI.Controllers;

public class UIController : Controller
{
    public IActionResult SetLightMode(string mode, string? returnUrl)
    {
        HttpContext.SetLightMode(mode);

        if (string.IsNullOrEmpty(returnUrl)) return RedirectToAction(""Index"", ""Home"");

        var uri = new Uri(returnUrl);
        if (uri.Host != Request.Host.Host) return RedirectToAction(""Index"", ""Home"");

        if (Request.Path.HasValue && (uri.LocalPath == Request.Path.Value)) return RedirectToAction(""Index"", ""Home"");

        return Redirect(returnUrl);
    }
}";
        results.Add(AddFile(folderPath, "UIController", code_UIController));

        return string.Join("\n", results);
    }

    public string GenerateProgramCs(string solutionPath)
    {
        StringBuilder sb = new();
        sb.AppendLine("using Autofac;");
        sb.AppendLine("using Autofac.Extensions.DependencyInjection;");
        sb.AppendLine("using Business;");
        sb.AppendLine("using Core;");
        sb.AppendLine("using DataAccess;");
        sb.AppendLine("using FluentValidation.AspNetCore;");
        sb.AppendLine("using Microsoft.AspNetCore.RateLimiting;");
        sb.AppendLine("using Model;");
        sb.AppendLine("using Serilog;");
        sb.AppendLine("using Serilog.Filters;");
        sb.AppendLine("using System.Threading.RateLimiting;");
        sb.AppendLine("using WebUI.ExceptionHandler;");
        if (_appSetting.IsThereIdentiy)
        {
            sb.AppendLine("using Core.Utils.Auth;");
            sb.AppendLine("using DataAccess.Contexts;");
            sb.AppendLine("using Microsoft.AspNetCore.Identity;");
            sb.AppendLine("using Model.Entities;");
        }

        sb.AppendLine("");
        sb.AppendLine("var builder = WebApplication.CreateBuilder(args);");
        sb.AppendLine("");
        sb.AppendLine("builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();");
        sb.AppendLine("");
        sb.AppendLine("builder.Services.AddFluentValidationAutoValidation();");
        sb.AppendLine("builder.Services.AddFluentValidationClientsideAdapters();");
        sb.AppendLine("");
        sb.AppendLine("");
        AddCORS(ref sb);
        AddRateLimiter(ref sb);
        AddLogImplemantation(ref sb);
        AddLayerRegistrations(ref sb);
        AddAutofacModules(ref sb);

        if (_appSetting.IsThereIdentiy)
        {
            // IDENTITY SETTINGS
            Entity? roleEntity = null;
            Entity? userEntity = null;
            string IdentityKeyType = "int";
            if (_appSetting.RoleEntityId != null)
            {
                roleEntity = _entityRepository.Get(f => f.Id == _appSetting.RoleEntityId);

                var uniqueFields = _fieldRepository.GetAll(f => f.EntityId == _appSetting.RoleEntityId && f.IsUnique);
                if (uniqueFields != null)
                {
                    IdentityKeyType = uniqueFields.First().MapFieldTypeName();
                }
            }
            if (_appSetting.UserEntityId != null)
            {
                userEntity = _entityRepository.Get(f => f.Id == _appSetting.UserEntityId);

                var uniqueFields = _fieldRepository.GetAll(f => f.EntityId == _appSetting.UserEntityId && f.IsUnique);
                if (uniqueFields != null)
                {
                    IdentityKeyType = uniqueFields.First().MapFieldTypeName();
                }
            }
            string IdentityUserType = $"IdentityUser<{IdentityKeyType}>";
            string IdentityRoleType = $"IdentityRole<{IdentityKeyType}>";
            if (userEntity != null) IdentityUserType = userEntity.Name;
            if (roleEntity != null) IdentityRoleType = roleEntity.Name;
            // IDENTITY SETTINGS 

            AddIdentityImplemantation(ref sb, IdentityUserType, IdentityRoleType);
        }
        AddCookieOptions(ref sb);
        AddActionFilters(ref sb);
        sb.AppendLine("");
        sb.AppendLine("var app = builder.Build();");
        sb.AppendLine("");
        sb.AppendLine("app.UseMiddleware<ExceptionHandleMiddleware>();");
        sb.AppendLine("");
        sb.AppendLine("//app.UseStaticFiles();");
        sb.AppendLine("");
        sb.AppendLine("if (!app.Environment.IsDevelopment())");
        sb.AppendLine("{");
        sb.AppendLine("\tapp.UseExceptionHandler(\"/Home/InvalidProcess\");");
        sb.AppendLine("");
        sb.AppendLine("\t// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.");
        sb.AppendLine("\tapp.UseHsts();");
        sb.AppendLine("}");
        sb.AppendLine("");
        sb.AppendLine("app.UseStatusCodePagesWithReExecute(\"/Error/NotFound\");");
        sb.AppendLine("");
        sb.AppendLine("app.UseHttpsRedirection();");
        sb.AppendLine("");
        sb.AppendLine("app.UseCors(\"policy_cors\");");
        sb.AppendLine("");
        sb.AppendLine("app.UseRouting();");
        sb.AppendLine("");
        sb.AppendLine("app.UseAuthentication();");
        sb.AppendLine("app.UseAuthorization();");
        sb.AppendLine("");
        sb.AppendLine("app.MapStaticAssets();");
        sb.AppendLine("");
        sb.AppendLine("app.UseRateLimiter();");
        sb.AppendLine("");
        sb.AppendLine(@"
app.MapControllerRoute(
    name: ""default"",
    pattern: ""{controller=Home}/{action=Index}/{id?}"")
    .WithStaticAssets()
    .RequireRateLimiting(""policy_rate_limiter"");
");
        sb.AppendLine("");
        sb.AppendLine("app.Run();");

        string folderPath = Path.Combine(solutionPath, "WebAPI");

        return AddFile(folderPath, "Program", sb.ToString());
    }

    public string GenerateAppSettings(string solutionPath)
    {
        string code = @"{
  ""Logging"": {
    ""LogLevel"": {
      ""Default"": ""Information"",
      ""Microsoft.AspNetCore"": ""Warning""
    }
  },
  ""ConnectionStrings"": {
    ""Database"": ""Data Source=SERVAN; Initial Catalog=GeneratedProjectDB; Integrated Security=SSPI; Trusted_Connection=True; TrustServerCertificate=True;""
  },
  ""AllowedHosts"": ""*""
}
";

        string folderPath = Path.Combine(solutionPath, "WebUI");
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, "appsettings.json");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, code);
                return $"OK: File appsettings.json added to WebUI project.";
            }
            else
            {
                return $"INFO: File appsettings.json already exists in WebUI project.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding file(appsettings.json) to WebUI project. \n Details:{ex.Message}");
        }

    }


    // **************** HEPLERS ****************
    private string AddFile(string folderPath, string fileName, string code)
    {
        try
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string filePath = Path.Combine(folderPath, $"{fileName}.cs");

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, code);
                return $"OK: File {fileName} added to WebUI project.";
            }
            else
            {
                return $"INFO: File {fileName} already exists in WebUI project.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding file({fileName}) to WebUI project. \n Details:{ex.Message}");
        }
    }

    private string RemoveFile(string folderPath, string fileName)
    {
        try
        {
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return $"OK: File {fileName} removed from WebUI project.";
            }
            else
            {
                return $"INFO: File {fileName} does not exist in WebUI project.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while removing file ({fileName}) from WebUI project. \n Details: {ex.Message}");
        }
    }

    private string RunCommand(string workingDirectory, string fileName, string arguments)
    {
        var processInfo = new ProcessStartInfo(fileName, arguments)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = Process.Start(processInfo))
        {
            string output = process!.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process!.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Command failed: {error}");
            }
            else
            {
                return output;
            }
        }
    }



    #region Program.cs Implemantations
    private void AddCORS(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- CORS -------
builder.Services.AddCors(options =>
{
    options.AddPolicy(""policy_cors"", builder =>
    {
        builder
            .AllowAnyOrigin()
            //.WithOrigins(""https://www.frontend.com"")
            //.AllowCredentials() // AllowAnyOrigin and AllowCredentials cannot using together use with WithOrigins option 
            .WithHeaders(""Content-Type"", ""Authorization"")
            .AllowAnyMethod()
            .SetPreflightMaxAge(TimeSpan.FromMinutes(10));
    });
});
// ------- CORS -------
");
    }

    private void AddRateLimiter(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- Rate Limiter -------
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddSlidingWindowLimiter(policyName: ""policy_rate_limiter"", slidingOptions =>
    {
        slidingOptions.PermitLimit = 15;
        slidingOptions.Window = TimeSpan.FromSeconds(3);
        slidingOptions.SegmentsPerWindow = 4;
        slidingOptions.QueueLimit = 5;
        slidingOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});
// ------- Rate Limiter -------
");
    }

    private void AddLogImplemantation(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- Logger Implementation -------
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override(""Microsoft"", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override(""System"", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty(""Target"", (object p) => p.ToString() == ""Validation""))
        .WriteTo.File(""Logs/Validation/validation.log"", rollingInterval: RollingInterval.Day,
            outputTemplate: ""{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}""))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty(""Target"", (object p) => p.ToString() == ""Application""))
        .WriteTo.File(""Logs/Application/application.log"", rollingInterval: RollingInterval.Day,
            outputTemplate: ""{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}""))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty(""Target"", (object p) => p.ToString() == ""Business""))
        .WriteTo.File(""Logs/Business/business.log"", rollingInterval: RollingInterval.Day,
            outputTemplate: ""{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}""))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty(""Target"", (object p) => p.ToString() == ""DataAccess""))
        .WriteTo.File(""Logs/DataAccess/dataAccess.log"", rollingInterval: RollingInterval.Day,
            outputTemplate: ""{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}""))
    .WriteTo.Logger(lc => lc
        .Filter.ByExcluding(Matching.WithProperty<string>(""Target"", _ => true))
        .WriteTo.File(""Logs/Other/others.log"", rollingInterval: RollingInterval.Day,
            outputTemplate: ""{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level}] {Message}{NewLine}{Exception}""))
    .CreateLogger();

builder.Host.UseSerilog();
// ------- Logger Implementation -------
");
    }


    private void AddLayerRegistrations(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- Layer Registrations -------
builder.Services.AddModelServices();
builder.Services.AddCoreServices(builder.Configuration);
builder.Services.AddDataAccessServices(builder.Configuration);
builder.Services.AddBusinessServices(builder.Configuration);
// ------- Layer Registrations -------
");
    }

    private void AddAutofacModules(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- Autofac Modules -------
builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory())
    .ConfigureContainer<ContainerBuilder>(builder =>
    {
        builder.RegisterModule(new Core.AutofacModule());
        builder.RegisterModule(new DataAccess.AutofacModule());
        builder.RegisterModule(new Business.AutofacModule());
    });
// ------- Autofac Modules -------
");
    }

    private void AddIdentityImplemantation(ref StringBuilder sb, string identityUserType, string identityRoleType)
    {
        sb.AppendLine("");
        sb.AppendLine("// ------- IDENTITY -------");
        sb.AppendLine("builder.Services.AddSingleton<TokenSettings>(new TokenSettings());");
        sb.AppendLine();
        sb.AppendLine("builder.Services");
        sb.AppendLine($"\t.AddIdentity<{identityUserType}, {identityRoleType}>(options =>");
        sb.AppendLine("\t{");
        sb.AppendLine("\t\t// Default Lockout settings.");
        sb.AppendLine("\t\toptions.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);");
        sb.AppendLine("\t\toptions.Lockout.MaxFailedAccessAttempts = 5;");
        sb.AppendLine("\t\toptions.Lockout.AllowedForNewUsers = true;");
        sb.AppendLine("");
        sb.AppendLine("\t\toptions.SignIn.RequireConfirmedEmail = false;");
        sb.AppendLine("");
        sb.AppendLine("\t\toptions.Password.RequiredLength = 4;");
        sb.AppendLine("\t\toptions.Password.RequireDigit = false;");
        sb.AppendLine("\t\toptions.Password.RequireNonAlphanumeric = false;");
        sb.AppendLine("\t\toptions.Password.RequireLowercase = false;");
        sb.AppendLine("\t\toptions.Password.RequireUppercase = false;");
        sb.AppendLine("");
        sb.AppendLine("\t\toptions.User.RequireUniqueEmail = false;");
        sb.AppendLine("\t\toptions.User.AllowedUserNameCharacters = \"abcçdefgğhiıjklmnoöpqrsştuüvwxyzABCÇDEFGĞHIİJKLMNOÖPQRSŞTUÜVWXYZ0123456789-._@+/*|!,;:()&#?[] \";");
        sb.AppendLine("\t})");
        sb.AppendLine("\t.AddEntityFrameworkStores<AppDbContext>()");
        sb.AppendLine("\t.AddDefaultTokenProviders();");
        sb.AppendLine();
        sb.AppendLine("builder.Services.AddAuthorization();");
        sb.AppendLine("// ------- IDENTITY -------");
        sb.AppendLine("");
    }

    private void AddCookieOptions(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- Cookie Options -------
builder.Services.ConfigureApplicationCookie(options =>
{
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.SlidingExpiration = true;
    options.AccessDeniedPath = ""/Error/Forbidden"";
    options.LogoutPath = ""/Account/Logout"";
    options.LoginPath = ""/Account/Login"";
    options.Cookie = new()
    {
        Name = ""IdentityCookie"",
        HttpOnly = true,
        SameSite = SameSiteMode.Lax,
        SecurePolicy = CookieSecurePolicy.Always
    };
});
// ------- Cookie Options -------
");
    }

    private void AddActionFilters(ref StringBuilder sb)
    {
        sb.AppendLine(@"
// ------- Action Filters -------
builder.Services.AddScoped(typeof(WebUI.Utils.ActionFilters.ValidationFilter<>));
// ------- Action Filters -------
");
    }
    #endregion
}
