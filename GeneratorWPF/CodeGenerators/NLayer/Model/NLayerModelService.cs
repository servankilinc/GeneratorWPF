using GeneratorWPF.Models;
using GeneratorWPF.Models.Enums;
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

            RemoveFile(projectPath, "Class1.cs");

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
    public string GenerateAuthModels(string solutionPath)
    {
        var results = new List<string>();
        var roslynAuthModelGenerator = new RoslynAuthModelsGenerator(_appSetting);

        // 1. Login Models
        string code_LoginRequest = roslynAuthModelGenerator.GeneraterLoginRequest();
        string code_LoginResponse = roslynAuthModelGenerator.GeneraterLoginResponse();

        string folderPathLogin = Path.Combine(solutionPath, "Model", "Auth", "Login");
        results.Add(AddFile(folderPathLogin, "LoginRequest", code_LoginRequest));
        results.Add(AddFile(folderPathLogin, "LoginResponse", code_LoginResponse));
       
        // 2. Refresh Auth Models
        string code_RefreshAuthRequest = roslynAuthModelGenerator.GeneraterRefreshAuthRequest();
        string code_RefreshAuthResponse = roslynAuthModelGenerator.GeneraterRefreshAuthResponse();
        
        string folderPathRefreshAuth = Path.Combine(solutionPath, "Model", "Auth", "RefreshAuth");
        results.Add(AddFile(folderPathRefreshAuth, "RefreshAuthRequest", code_RefreshAuthRequest));
        results.Add(AddFile(folderPathRefreshAuth, "RefreshAuthResponse", code_RefreshAuthResponse));

        // 3. Signup Models
        string code_SignUpRequest = roslynAuthModelGenerator.GeneraterSignUpRequest();
        string code_SignUpResponse = roslynAuthModelGenerator.GeneraterSignUpResponse();

        string folderPathSignup = Path.Combine(solutionPath, "Model", "Auth", "SignUp");
        results.Add(AddFile(folderPathSignup, "SignUpRequest", code_SignUpRequest));
        results.Add(AddFile(folderPathSignup, "SignUpResponse", code_SignUpResponse));

        return string.Join("\n", results);
    }

    public string GenerateProjectEntities(string solutionPath)
    {
        string code_Archive = @"using Core.Enums;
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

        string code_Log = @"using Core.Enums;
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

        var dtos = _dtoRepository.GetAll(f => f.Control == false, include: i => i.Include(x => x.RelatedEntity).ThenInclude(x => x.Fields));

        var roslynDtoGenerator = new RoslynDtoGenerator();

        // Report Dto
        foreach (var dto in dtos)
        {
            string code = dto.RelatedEntity.ReportDtoId == dto.Id ? 
                roslynDtoGenerator.GeneraterReportDto(dto, _appSetting) :
                roslynDtoGenerator.GeneraterDto(dto, _appSetting);
            string folderPath = Path.Combine(solutionPath, "Model", "Dtos", $"{dto.RelatedEntity.Name}_");

            results.Add(AddFile(folderPath, dto.Name, code));
        }

        return string.Join("\n", results);
    }

    public string GenerateEntities(string solutionPath)
    {
        var results = new List<string>();

        var entities = _entityRepository.GetAll(f => f.Control == false, include: i => i.Include(x => x.Fields));

        var roslynEntityGenerator = new RoslynEntityGenerator(_appSetting);

        foreach (var entity in entities)
        {
            string code = roslynEntityGenerator.GeneraterEntity(entity);

            string folderPath = Path.Combine(solutionPath, "Model", "Entities");
            results.Add(AddFile(folderPath, entity.Name, code));
        }

        if (_appSetting.IsThereIdentiy)
        {
            var roslynAuthModelsGenerator = new RoslynAuthModelsGenerator(_appSetting);

            string code_refreshToken = roslynAuthModelsGenerator.GeneraterRefreshTokenEntity();

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

    private string RemoveFile(string folderPath, string fileName)
    {
        try
        {
            string filePath = Path.Combine(folderPath, fileName);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return $"OK: File {fileName} removed from Model project.";
            }
            else
            {
                return $"INFO: File {fileName} does not exist in Model project.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while removing file ({fileName}) from Model project. \n Details: {ex.Message}");
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
