using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.IO;
using System.Xml.Linq;

namespace GeneratorWPF.CodeGenerators.NLayer.Model;

public class NLayerModelService
{
    private readonly EntityRepository _entityRepository;
    private readonly DtoRepository _dtoRepository;
    private readonly AppSetting _appSetting;
    public NLayerModelService(AppSetting appSetting)
    {
        _appSetting = appSetting;
        _entityRepository = new();
        _dtoRepository = new();
    }

    public string CreateProject(string path, string solutionName)
    {
        try
        {
            string projectPath = Path.Combine(path, "Model");
            string csprojPath = Path.Combine(projectPath, "Model.csproj");

            if (Directory.Exists(projectPath) && File.Exists(csprojPath))
                return "INFO: Model layer project already exists.";

            RunCommand(path, "dotnet", "new classlib -n Model");
            RunCommand(path, "dotnet", $"sln {solutionName}.sln add Model/Model.csproj");
            RunCommand(projectPath, "dotnet", $"dotnet add reference ../Core/Core.csproj");

            return "OK: Model Project Created Successfully";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while creating the Model project. \n\t Details:{ex.Message}");
        }
    }

    #region Package Methods
    public string AddPackage(string path, string packageName)
    {
        try
        {
            string projectPath = Path.Combine(path, "Model");
            string csprojPath = Path.Combine(projectPath, "Model.csproj");

            if (!File.Exists(csprojPath))
                throw new FileNotFoundException($"Model.csproj not found for adding package({packageName}).");

            var doc = XDocument.Load(csprojPath);

            var packageAlreadyAdded = doc.Descendants("PackageReference").Any(p => p.Attribute("Include")?.Value == packageName);

            if (packageAlreadyAdded)
                return $"INFO: Package {packageName} already exists in Model project.";

            RunCommand(projectPath, "dotnet", $"add package {packageName}");

            return $"OK: Package {packageName} added to Model project.";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding pacgace to Model project. \n\t Details:{ex.Message}");
        }
    }
    public string Restore(string path)
    {
        try
        {
            string projectPath = Path.Combine(path, "Model");

            RunCommand(projectPath, "dotnet", "restore");
            return "OK: Restored Model project.";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while restoring Model project. \n Details:{ex.Message}");
        }
    }
    #endregion

    #region Static Codes
    public string GenerateAuthLogin(string solutionPath)
    {
        string code_LoginRequest = @"
using Core.Utils.CriticalData;
using FluentValidation;

namespace Model.Auth.Login;

public class LoginRequest
{
    public string Email { get; set; } = null!;

    [CriticalData]
    public string Password { get; set; } = null!;
}


public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(b => b.Email).NotNull().EmailAddress().NotEmpty().EmailAddress();
        RuleFor(b => b.Password).NotNull().MinimumLength(6).NotEmpty();
    }
}";

        string code_LoginResponse = @"
using Core.Utils.Auth;
using Model.Dtos.User_;

namespace Model.Auth.Login;

public class LoginResponse
{
    public UserBasicResponseDto User { get; set; } = null!;
    public AccessToken AccessToken { get; set; } = null!;
    public IList<string>? Roles { get; set; }
}

public class LoginTrustedResponse : LoginResponse
{
    public string RefreshToken { get; set; } = null!;
}";


        string folderPath = Path.Combine(solutionPath, "Model", "Auth", "Login");

        var results = new List<string>
        {
            AddFile(folderPath, "LoginRequest", code_LoginRequest),
            AddFile(folderPath, "LoginResponse", code_LoginResponse)
        };

        return string.Join("\n", results);
    }

    public string GenerateAuthRefreshAuth(string solutionPath)
    {
        string code_RefreshAuthRequest = @"
using FluentValidation;

namespace Model.Auth.RefreshAuth;

public class RefreshAuthRequest
{
    public Guid UserId { get; set; }
    public bool IsTrusted { get; set; }
    public string RefreshToken { get; set; } = null!;
}

public class RefreshAuthRequestValidator : AbstractValidator<RefreshAuthRequest>
{
    public RefreshAuthRequestValidator()
    {
        RuleFor(b => b.UserId).NotNull().NotEqual(Guid.Empty).NotEmpty();
        When(b => b.IsTrusted, () =>
        {
            RuleFor(b => b.RefreshToken)
                .NotNull()
                .NotEmpty();
        });
    }
}";

        string code_RefreshAuthResponse = @"
using Core.Utils.Auth;
using Model.Dtos.User_;

namespace Model.Auth.RefreshAuth;

public class RefreshAuthResponse
{
    public UserBasicResponseDto User { get; set; } = null!;
    public AccessToken AccessToken { get; set; } = null!;
}

public class RefreshAuthTrustedResponse : RefreshAuthResponse
{
    public string RefreshToken { get; set; } = null!;
}";


        string folderPath = Path.Combine(solutionPath, "Model", "Auth", "RefreshAuth");

        var results = new List<string>
        {
            AddFile(folderPath, "RefreshAuthRequest", code_RefreshAuthRequest),
            AddFile(folderPath, "RefreshAuthResponse", code_RefreshAuthResponse)
        };

        return string.Join("\n", results);
    }

    public string GenerateAuthSignUp(string solutionPath)
    {
        string code_SignUpRequest = @"
using FluentValidation;

namespace Model.Auth.SignUp;

public class SignUpRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
}

public class SignUpRequestValidator : AbstractValidator<SignUpRequest>
{
    public SignUpRequestValidator()
    {
        RuleFor(b => b.Email).NotNull().EmailAddress().NotEmpty().EmailAddress();
        RuleFor(b => b.Password).NotNull().MinimumLength(6).NotEmpty();
        RuleFor(b => b.FirstName).NotNull().MinimumLength(2).NotEmpty();
        RuleFor(b => b.LastName).NotNull().MinimumLength(2).NotEmpty().NotEqual(s => s.FirstName);
    }
}";

        string code_SignUpResponse = @"
using Core.Utils.Auth;
using Model.Dtos.User_;

namespace Model.Auth.SignUp;

public class SignUpResponse
{
    public UserBasicResponseDto User { get; set; } = null!;
    public AccessToken AccessToken { get; set; } = null!;
    public IList<string>? Roles { get; set; }
}

public class SignUpTrustedResponse : SignUpResponse
{
    public string RefreshToken { get; set; } = null!;
}";


        string folderPath = Path.Combine(solutionPath, "Model", "Auth", "SignUp");

        var results = new List<string>
        {
            AddFile(folderPath, "SignUpRequest", code_SignUpRequest),
            AddFile(folderPath, "SignUpResponse", code_SignUpResponse)
        };

        return string.Join("\n", results);
    }

    public string GenerateProjectEntities(string solutionPath)
    {
        string code_Archive = @"
using Core.Enums;
using Core.Model;

namespace Model.ProjectEntities;

public class Archive: IEntity, IProjectEntity
{
    public int Id { get; set; }
    public string? EntityId { get; set; }
    public string? TableName { get; set; }
    public string? RequesterId { get; set; }
    public CrudTypes Action { get; set; }
    public string? Data { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime DateUtc { get; set; }
}";

        string code_Log = @"
using Core.Enums;
using Core.Model;

namespace Model.ProjectEntities;

public class Log: IEntity, IProjectEntity
{
    public int Id { get; set; }
    public string? EntityId { get; set; }
    public string? TableName { get; set; }
    public string? RequesterId { get; set; }
    public CrudTypes Action { get; set; }
    public string? Data { get; set; }
    public string? NewData { get; set; }
    public string? OldData { get; set; }
    public string? ClientIp { get; set; }
    public string? UserAgent { get; set; }
    public DateTime DateUtc { get; set; }
}";


        string folderPath = Path.Combine(solutionPath, "Model", "ProjectEntities");

        var results = new List<string>
        {
            AddFile(folderPath, "Archive", code_Archive),
            AddFile(folderPath, "Log", code_Log)
        };

        return string.Join("\n", results);
    }
    #endregion

    public string GenerateDtos(string solutionPath)
    {
        var results = new List<string>();

        var dtos = _dtoRepository.GetAll(f => f.Control == false, include: i => i.Include(x => x.RelatedEntity));

        var roslynDtoGenerator = new RoslynDtoGenerator();

        foreach (var dto in dtos)
        {
            string code = roslynDtoGenerator.GeneraterDto(dto, _appSetting);
            string folderPath = Path.Combine(solutionPath, "Model", "Dtos", $"{dto.RelatedEntity.Name}_");

            results.Add(AddFile(folderPath, dto.Name, code));
        }

        return string.Join("\n", results);
    }

    public string GenerateEntities(string solutionPath)
    {
        var results = new List<string>();

        var entities = _entityRepository.GetAll(f => f.Control == false);

        var roslynEntityGenerator = new RoslynEntityGenerator(_appSetting);

        foreach (var entity in entities)
        {
            string code = roslynEntityGenerator.GeneraterEntity(entity);

            string folderPath = Path.Combine(solutionPath, "Model", "Entities");
            results.Add(AddFile(folderPath, entity.Name, code));
        }

        if (_appSetting.IsThereIdentiy)
        {
            string code_refreshToken = @"
using Core.Model;

namespace Model.Entities;

public class RefreshToken : IEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string IpAddress { get; set; } = null!;
    public string Token { get; set; } = null!;
    public DateTime ExpirationUtc { get; set; }
    public DateTime CreateDateUtc { get; set; }
    public int TTL { get; set; }

    public virtual User? User { get; set; }
}";

            string folderPath = Path.Combine(solutionPath, "Model", "Entities");
            results.Add(AddFile(folderPath, "RefreshToken", code_refreshToken));
        }

        return string.Join("\n", results);
    }

    public string GenerateServiceRegistrations(string solutionPath)
    {
        string code = @"
            using FluentValidation;
            using Microsoft.Extensions.DependencyInjection;
            using System.Reflection;

            namespace Model;

            public static class ServiceRegistration
            {
                public static IServiceCollection AddModelServices(this IServiceCollection services)
                {
                    services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

                    return services;
                }
            }";

        string folderPath = Path.Combine(solutionPath, "Model");

        return AddFile(folderPath, "ServiceRegistration", code);
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
                return $"OK: File {fileName} added to Model project.";
            }
            else
            {
                return $"INFO: File {fileName} already exists in Model project.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding file({fileName}) to Model project. \n Details:{ex.Message}");
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
}
