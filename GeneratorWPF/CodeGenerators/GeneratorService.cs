using GeneratorWPF.Models;
using GeneratorWPF.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneratorWPF.CodeGenerators;

public class GeneratorService
{
    private AppSettingsRepository _appSettingsRepository;
    private AppSetting _appSetting;
    public GeneratorService(AppSettingsRepository appSettingsRepository)
    {
        _appSettingsRepository = appSettingsRepository;
        _appSetting = _appSettingsRepository.GetAll().Last();
    }

    public void GenerateCoreLayer()
    {
        // 1. Create Core Class Library if not exists
        RoslynCoreLayerGenerator.GenerateProject(_appSetting.Path);
        // 2. Add Packages
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Autofac");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Autofac.Extensions.DependencyInjection");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Autofac.Extras.DynamicProxy");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Castle.Core.AsyncInterceptor");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "FluentValidation");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "FluentValidation.DependencyInjectionExtensions");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Microsoft.EntityFrameworkCore");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Microsoft.EntityFrameworkCore.Design");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Microsoft.EntityFrameworkCore.SqlServer");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Microsoft.Extensions.Caching.StackExchangeRedis");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Microsoft.Extensions.Configuration.Binder");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Microsoft.Identity.Client");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "Newtonsoft.Json");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "System.ComponentModel.Composition");
        RoslynCoreLayerGenerator.AddPackage(_appSetting.Path, "System.Linq.Dynamic.Core");
        // 3. Utils 
        RoslynCoreLayerGenerator.GenerateHashHelper(_appSetting.Path);
        RoslynCoreLayerGenerator.GenerateAccessToken(_appSetting.Path);
        RoslynCoreLayerGenerator.GenerateAutheticatorType(_appSetting.Path);
        RoslynCoreLayerGenerator.GenerateTokenOptions(_appSetting.Path);
    }
}
