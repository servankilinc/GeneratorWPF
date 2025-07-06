using GeneratorWPF.CodeGenerators.NLayer.API;
using GeneratorWPF.CodeGenerators.NLayer.Base;
using GeneratorWPF.CodeGenerators.NLayer.Business;
using GeneratorWPF.CodeGenerators.NLayer.Core;
using GeneratorWPF.CodeGenerators.NLayer.DataAccess;
using GeneratorWPF.CodeGenerators.NLayer.Model;
using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using System.IO;

namespace GeneratorWPF.CodeGenerators.NLayer;

public class NLayerGeneratorService
{
    private readonly AppSettingsRepository _appSettingsRepository;
    private readonly AppSetting _appSetting;
    public NLayerGeneratorService()
    {
        _appSettingsRepository = new AppSettingsRepository();
        _appSetting = _appSettingsRepository.Get(f => f.Id == 1);
    }

    public bool GenerateSolution(Action<string> log)
    {
        try
        {
            if (_appSetting == null || string.IsNullOrEmpty(_appSetting.Path) || string.IsNullOrEmpty(_appSetting.SolutionName) || string.IsNullOrEmpty(_appSetting.ProjectName))
                throw new Exception("App Settings Not Completted To Generate!");

            var NLayerBaseService = new NLayerBaseService();

            log(NLayerBaseService.CreateSolution(_appSetting.Path, _appSetting.ProjectName));

            return true;
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    public bool GenerateCoreLayer(Action<string> log)
    {
        try
        {
            if (_appSetting == null || string.IsNullOrEmpty(_appSetting.Path) || string.IsNullOrEmpty(_appSetting.SolutionName))
                throw new Exception("App Settings Not Completted To Generate!");

            var NLayerCoreService = new NLayerCoreService();

            string solutionPath = Path.Combine(_appSetting.Path, _appSetting.SolutionName);

            // 1. Create Core Class Library if not exists
            log(NLayerCoreService.CreateProject(solutionPath, _appSetting.SolutionName));

            // 2. Add Packages
            log(NLayerCoreService.AddPackage(solutionPath, "Autofac"));
            log(NLayerCoreService.AddPackage(solutionPath, "Autofac.Extensions.DependencyInjection"));
            log(NLayerCoreService.AddPackage(solutionPath, "Autofac.Extras.DynamicProxy"));
            log(NLayerCoreService.AddPackage(solutionPath, "AutoMapper"));
            log(NLayerCoreService.AddPackage(solutionPath, "Castle.Core.AsyncInterceptor"));
            log(NLayerCoreService.AddPackage(solutionPath, "FluentValidation"));
            log(NLayerCoreService.AddPackage(solutionPath, "FluentValidation.DependencyInjectionExtensions"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.AspNetCore.Identity.EntityFrameworkCore"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.EntityFrameworkCore"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.EntityFrameworkCore.Design"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.EntityFrameworkCore.SqlServer"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.EntityFrameworkCore.Tools"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.Extensions.Caching.StackExchangeRedis"));
            log(NLayerCoreService.AddPackage(solutionPath, "Microsoft.Extensions.Configuration.Binder"));
            log(NLayerCoreService.AddPackage(solutionPath, "Newtonsoft.Json"));
            log(NLayerCoreService.AddPackage(solutionPath, "Serilog.AspNetCore"));
            log(NLayerCoreService.AddPackage(solutionPath, "Serilog.Sinks.File"));
            log(NLayerCoreService.AddPackage(solutionPath, "System.ComponentModel.Composition"));
            log(NLayerCoreService.AddPackage(solutionPath, "System.Linq.Dynamic.Core"));

            log(NLayerCoreService.Restore(solutionPath));

            // 3. BaseRequestModels
            log(NLayerCoreService.GenerateBaseRequestModels(solutionPath));

            // 4. Enums
            log(NLayerCoreService.GenerateEnums(solutionPath));

            // 5. Models
            log(NLayerCoreService.GenerateModels(solutionPath));

            // 6. Utils
            log(NLayerCoreService.GenerateUtilsAuth(solutionPath));
            log(NLayerCoreService.GenerateUtilsCaching(solutionPath));
            log(NLayerCoreService.GenerateUtilsCriticalData(solutionPath));
            log(NLayerCoreService.GenerateUtilsCrossCuttingConcerns(solutionPath));
            log(NLayerCoreService.GenerateUtilsDatatable(solutionPath));
            log(NLayerCoreService.GenerateUtilsDynamicQuery(solutionPath));
            log(NLayerCoreService.GenerateUtilsExceptionHandle(solutionPath));
            log(NLayerCoreService.GenerateUtilsHttpContextManager(solutionPath));
            log(NLayerCoreService.GenerateUtilsPagination(solutionPath));

            // 7. Service Registration
            log(NLayerCoreService.GenerateServiceRegistrations(solutionPath));

            return true;
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    public bool GenerateModelLayer(Action<string> log)
    {
        try
        {
            if (_appSetting == null || string.IsNullOrEmpty(_appSetting.Path) || string.IsNullOrEmpty(_appSetting.SolutionName))
                throw new Exception("App Settings Not Completted To Generate!");

            var NLayerModelService = new NLayerModelService(_appSetting);

            string solutionPath = Path.Combine(_appSetting.Path, _appSetting.SolutionName);

            // 1. Create Core Class Library if not exists
            log(NLayerModelService.CreateProject(solutionPath, _appSetting.SolutionName));

            // 2. Auth
            if (_appSetting.IsThereIdentiy)
            {
                log(NLayerModelService.GenerateAuthLogin(solutionPath));
                log(NLayerModelService.GenerateAuthRefreshAuth(solutionPath));
                log(NLayerModelService.GenerateAuthSignUp(solutionPath));
            }

            // 3. Dtos
            log(NLayerModelService.GenerateDtos(solutionPath));

            // 4. Entities
            log(NLayerModelService.GenerateEntities(solutionPath));

            // 5. ProjectEntities
            log(NLayerModelService.GenerateProjectEntities(solutionPath));

            // 6. Service Registration
            log(NLayerModelService.GenerateServiceRegistrations(solutionPath));

            return true;
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    public bool GenerateDataAccessLayer(Action<string> log)
    {
        try
        {
            if (_appSetting == null || string.IsNullOrEmpty(_appSetting.Path) || string.IsNullOrEmpty(_appSetting.SolutionName))
                throw new Exception("App Settings Not Completted To Generate!");

            var nLayerDataAccessService = new NLayerDataAccessService(_appSetting);

            string solutionPath = Path.Combine(_appSetting.Path, _appSetting.SolutionName);

            // 1. Create Core Class Library if not exists
            log(nLayerDataAccessService.CreateProject(solutionPath, _appSetting.SolutionName));

            // 2. Repository Base
            log(nLayerDataAccessService.GenerateRepositoryBase(solutionPath));

            // 3. Interceptors
            log(nLayerDataAccessService.GenerateInterceptors(solutionPath));

            // 4. Servises
            log(nLayerDataAccessService.GenerateServices(solutionPath));

            // 5. UOW
            log(nLayerDataAccessService.GenerateUOW(solutionPath));

            // 6. Context Fiel
            log(nLayerDataAccessService.GenerateContext(solutionPath));

            // 7. Service Registrations
            log(nLayerDataAccessService.GenerateServiceRegistrations(solutionPath));

            return true;
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    public bool GenerateBusinessLayer(Action<string> log)
    {
        try
        {
            if (_appSetting == null || string.IsNullOrEmpty(_appSetting.Path) || string.IsNullOrEmpty(_appSetting.SolutionName))
                throw new Exception("App Settings Not Completted To Generate!");

            var nLayerBusinessService = new NLayerBusinessService(_appSetting);

            string solutionPath = Path.Combine(_appSetting.Path, _appSetting.SolutionName);

            // 1. Create Core Class Library if not exists
            log(nLayerBusinessService.CreateProject(solutionPath, _appSetting.SolutionName));

            // 2. Add Packages
            log(nLayerBusinessService.AddPackage(solutionPath, "AutoMapper"));

            // 3. Service Base
            log(nLayerBusinessService.GenerateServiceBase(solutionPath));

            // 4. Utils
            log(nLayerBusinessService.GenerateUtils(solutionPath));

            // 5. Mappings
            log(nLayerBusinessService.GenerateMappings(solutionPath));

            // 6. Concretes
            log(nLayerBusinessService.GeneraterService(solutionPath));

            // 7. Service Registrations
            log(nLayerBusinessService.GenerateServiceRegistrations(solutionPath));

            return true;
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }

    public bool GenerateAPILayer(Action<string> log)
    {
        try
        {
            if (_appSetting == null || string.IsNullOrEmpty(_appSetting.Path) || string.IsNullOrEmpty(_appSetting.SolutionName))
                throw new Exception("App Settings Not Completted To Generate!");

            var nLayerAPIService = new NLayerAPIService(_appSetting);

            string solutionPath = Path.Combine(_appSetting.Path, _appSetting.SolutionName);

            // 1. Create Core Class Library if not exists
            log(nLayerAPIService.CreateProject(solutionPath, _appSetting.SolutionName));

            // 2. Add Packages
            log(nLayerAPIService.AddPackage(solutionPath, "Microsoft.AspNetCore.Authentication.JwtBearer"));
            log(nLayerAPIService.AddPackage(solutionPath, "Microsoft.AspNetCore.OpenApi"));
            log(nLayerAPIService.AddPackage(solutionPath, "Microsoft.EntityFrameworkCore.Design"));
            log(nLayerAPIService.AddPackage(solutionPath, "Scalar.AspNetCore"));

            // 3. Exception Handler
            log(nLayerAPIService.GenerateExceptionHandler(solutionPath));

            // 4. Scalar Security Scheme Transformer
            log(nLayerAPIService.GenerateScalarSecuritySchemeTransformer(solutionPath));

            // 5. Program.cs
            log(nLayerAPIService.GenerateProgramCs(solutionPath));

            // 6. Mappings
            log(nLayerAPIService.GenerateAppSettings(solutionPath));

            // 7. Controllers
            log(nLayerAPIService.GenerateControllers(solutionPath));

            return true;
        }
        catch (Exception ex)
        {
            log(ex.Message);
            return false;
        }
    }
}
