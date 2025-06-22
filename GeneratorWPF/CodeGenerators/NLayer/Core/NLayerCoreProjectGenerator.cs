using System.Diagnostics;
using System.IO;
using System.Xml.Linq;
using Path = System.IO.Path;

namespace GeneratorWPF.CodeGenerators.NLayer.Core;

public class NLayerCoreService
{
    public string CreateProject(string path, string solutionName)
    {
        try
        {
            string projectPath = Path.Combine(path, "Core");
            string csprojPath = Path.Combine(projectPath, "Core.csproj");

            if (Directory.Exists(projectPath) && File.Exists(csprojPath))
                return "INFO: Core layer project already exists.";

            RunCommand(path, "dotnet", "new classlib -n Core");
            RunCommand(path, "dotnet", $"sln {solutionName}.sln add Core/Core.csproj");

            return "OK: Core Project Created Successfully";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while creating the Core project. \n\t Details:{ex.Message}");
        }
    }

    #region Package-Methods
    public string AddPackage(string path, string packageName)
    {
        try
        {
            string projectPath = Path.Combine(path, "Core");
            string csprojPath = Path.Combine(projectPath, "Core.csproj");

            if (!File.Exists(csprojPath))
                throw new FileNotFoundException($"Core.csproj not found for adding package({packageName}).");

            var doc = XDocument.Load(csprojPath);

            var packageAlreadyAdded = doc.Descendants("PackageReference").Any(p => p.Attribute("Include")?.Value == packageName);

            if (packageAlreadyAdded)
                return $"INFO: Package {packageName} already exists in Core project.";

            RunCommand(projectPath, "dotnet", $"add package {packageName}");

            return $"OK: Package {packageName} added to Core project.";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding pacgace to Core project. \n\t Details:{ex.Message}");
        }
    }
    public string Restore(string path)
    {
        try
        {
            string projectPath = Path.Combine(path, "Core");

            RunCommand(projectPath, "dotnet", "restore");
            return "OK: Restored Core project.";
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while restoring Core project. \n Details:{ex.Message}");
        }
    }
    #endregion

    public string GenerateBaseRequestModels(string path)
    {
        string code = @"
using Core.Utils.Datatable;
using Core.Utils.DynamicQuery;
using Core.Utils.Pagination;

namespace Core.BaseRequestModels;

public class DynamicRequest
{
    public Filter? Filter { get; set; }
    public IEnumerable<Sort>? Sorts { get; set; }
}

public class DynamicPaginationRequest
{
    public PaginationRequest PaginationRequest { get; set; } = new PaginationRequest();
    public Filter? Filter { get; set; }
    public IEnumerable<Sort>? Sorts { get; set; }
}

public class DynamicDatatableServerSideRequest
{
    public DatatableRequest DatatableRequest { get; set; } = null!;
    public Filter? Filter { get; set; }
}";

        string folderPath = Path.Combine(path, "Core", "BaseRequestModels");
        return AddFile(folderPath, "BaseRequestModels", code);
    }

    public string GenerateEnums(string path)
    {
        string code_AuthenticationTypes = @"
using System.ComponentModel;

namespace Core.Enums;

public enum AuthenticationTypes
{
    [Description(""None"")]
    None = 0,
    [Description(""Email"")]
    Email = 1,
    [Description(""Google"")]
    Google = 2,
    [Description(""Facebook"")]
    Facebook = 3,
}";

        string code_CrudTypes = @"
using System.ComponentModel;

namespace Core.Enums;

public enum CrudTypes
{
    [Description(""Read"")]
    Read = 1,
    [Description(""Create"")]
    Create = 2,
    [Description(""Update"")]
    Update = 3,
    [Description(""Delete"")]
    Delete = 4,
    [Description(""Undefined"")]
    Undefined = 5,
}";

        string code_ProblemDetailTypes = @"
using System.ComponentModel;

namespace Core.Enums;

public enum ProblemDetailTypes
{
    [Description(""Unknown Error"")]
    General = 1,
    [Description(""Validation Error"")]
    Validation = 2,
    [Description(""Business Logic"")]
    Business = 3,
    [Description(""Data Access"")]
    DataAccess = 4,
}";

        string code_RoleTypes = @"
using System.ComponentModel;

namespace Core.Enums;

public enum RoleTypes
{
    [Description(""User"")]
    User = 0,
    [Description(""Manager"")]
    Manager = 1,
    [Description(""Admin"")]
    Admin = 2,
    [Description(""Owner"")]
    Owner = 3,
}";


        string folderPath = Path.Combine(path, "Core", "Enums");

        var results = new List<string>
        {
            AddFile(folderPath, "AuthenticationTypes", code_AuthenticationTypes),
            AddFile(folderPath, "CrudTypes", code_CrudTypes),
            AddFile(folderPath, "ProblemDetailTypes", code_ProblemDetailTypes),
            AddFile(folderPath, "RoleTypes", code_RoleTypes)
        };

        return string.Join("\n", results);
    }

    public string GenerateModels(string path)
    {
        string code_IAppException = @"
namespace Core.Model;

public interface IAppException
{
    string? LocationName { get; set; }
    string? Parameters { get; set; }
    string? Description { get; set; }
}";

        string code_IDto = @"
namespace Core.Model;

public abstract class IDto
{
    // ... signature class
}";

        string code_IEntity = @"
namespace Core.Model;

/// <summary>
/// Base interface for all entities.
/// </summary>
public interface IEntity
{
}

/// <summary>
/// Interface for entities voiding to handle interceptors.
/// </summary>

public interface IProjectEntity
{
}

/// <summary>
/// Interface for entities that support soft deletion.
/// </summary>
public interface ISoftDeletableEntity
{
    string? DeletedBy { get; set; }
    bool IsDeleted { get; set; }
    DateTime? DeletedDateUtc { get; set; }
}

/// <summary>
/// Interface for entities that support auditing.
/// </summary>
public interface IAuditableEntity
{
    string? CreatedBy { get; set; }
    string? UpdatedBy { get; set; }
    DateTime? CreateDateUtc { get; set; }
    DateTime? UpdateDateUtc { get; set; }
}

/// <summary>
/// Interface for entities that support logging.
/// </summary>
public interface ILoggableEntity
{
}

/// <summary>
/// Interface for entities that support archiving.
/// </summary>
public interface IArchivableEntity
{
}";


        string folderPath = Path.Combine(path, "Core", "Model");

        var results = new List<string>
        {
            AddFile(folderPath, "IAppException", code_IAppException),
            AddFile(folderPath, "IDto", code_IDto),
            AddFile(folderPath, "IEntity", code_IEntity)
        };

        return string.Join("\n", results);
    }

    #region Utils
    public string GenerateUtilsAuth(string path)
    {
        string code_AccessToken = @"
namespace Core.Utils.Auth;

public record AccessToken(string Token, DateTime Expiration);";

        string code_TokenSettings = @"
namespace Core.Utils.Auth;

public class TokenSettings
{
    public string Audience { get; set; } = null!;
    public string Issuer { get; set; } = null!;
    public string SecurityKey { get; set; } = null!;
    public int AccessTokenExpiration { get; set; }
    public int RefreshTokenExpiration { get; set; }
    public int RefreshTokenTTL { get; set; }
}";


        string folderPath = Path.Combine(path, "Core", "Utils", "Auth");

        var results = new List<string>
        {
            AddFile(folderPath, "AccessToken", code_AccessToken),
            AddFile(folderPath, "TokenSettings", code_TokenSettings)
        };

        return string.Join("\n", results);
    }

    public string GenerateUtilsCaching(string path)
    {
        string code_CacheResponse = @"
namespace Core.Utils.Caching;

public record CacheResponse(bool IsSuccess, string? Source = default);";

        string code_CacheService = @"
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System.Text;

namespace Core.Utils.Caching;

public class CacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    public CacheService(IDistributedCache distributedCache) => _distributedCache = distributedCache;

    public CacheResponse GetFromCache(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(nameof(cacheKey));

        byte[]? cachedData = _distributedCache.Get(cacheKey);
        if (cachedData != null)
        {
            var response = Encoding.UTF8.GetString(cachedData);
            if (string.IsNullOrEmpty(response)) return new CacheResponse(IsSuccess: false);

            return new CacheResponse(IsSuccess: true, Source: response);
        }
        else
        {
            return new CacheResponse(IsSuccess: false);
        }
    }

    public void AddToCache<TData>(string cacheKey, string[] cacheGroupKeys, TData data)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(nameof(cacheKey));

        DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions()
        {
            SlidingExpiration = TimeSpan.FromDays(1),
            AbsoluteExpiration = DateTime.UtcNow.AddDays(2)
        };

        string serializedData = JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 7
        });
        byte[]? bytedData = Encoding.UTF8.GetBytes(serializedData);

        _distributedCache.Set(cacheKey, bytedData, cacheEntryOptions);

        if (cacheGroupKeys != null && cacheGroupKeys.Any()) AddCacheKeyToGroups(cacheKey, cacheGroupKeys, cacheEntryOptions);
    }

    public void RemoveFromCache(string cacheKey)
    {
        if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(nameof(cacheKey));

        _distributedCache.Remove(cacheKey);
    }

    public void RemoveCacheGroupKeys(string[] cacheGroupKeyList)
    {
        if (cacheGroupKeyList == null) throw new ArgumentNullException(nameof(cacheGroupKeyList));

        foreach (string cacheGroupKey in cacheGroupKeyList)
        {
            byte[]? keyListFromCache = _distributedCache.Get(cacheGroupKey);
            _distributedCache.Remove(cacheGroupKey);

            if (keyListFromCache == null) continue;

            string stringKeyList = Encoding.UTF8.GetString(keyListFromCache);
            HashSet<string>? keyListInGroup = JsonConvert.DeserializeObject<HashSet<string>>(stringKeyList);
            if (keyListInGroup != null)
            {
                foreach (var key in keyListInGroup)
                {
                    _distributedCache.Remove(key);
                }
            }
        }
    }

    private void AddCacheKeyToGroups(string cacheKey, string[] cacheGroupKeys, DistributedCacheEntryOptions groupCacheEntryOptions)
    {
        foreach (string cacheGroupKey in cacheGroupKeys)
        {
            HashSet<string>? keyListInGroup;
            byte[]? cachedGroupData = _distributedCache.Get(cacheGroupKey);
            if (cachedGroupData != null)
            {
                keyListInGroup = JsonConvert.DeserializeObject<HashSet<string>>(Encoding.UTF8.GetString(cachedGroupData));
                if (keyListInGroup != null && !keyListInGroup.Contains(cacheKey))
                {
                    keyListInGroup.Add(cacheKey);
                }
            }
            else
            {
                keyListInGroup = new HashSet<string>(new[] { cacheKey });
            }
            string serializedData = JsonConvert.SerializeObject(keyListInGroup, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                MaxDepth = 7
            });
            byte[]? bytedKeyList = Encoding.UTF8.GetBytes(serializedData);
            _distributedCache.Set(cacheGroupKey, bytedKeyList, groupCacheEntryOptions);
        }
    }
}";

        string code_ICacheService = @"
namespace Core.Utils.Caching;

public interface ICacheService
{
    CacheResponse GetFromCache(string cacheKey);
    void AddToCache<TData>(string CacheKey, string[] CacheGroupKeys, TData data);
    void RemoveFromCache(string CacheKey);
    void RemoveCacheGroupKeys(string[] cacheGroupKeys);
}";


        string folderPath = Path.Combine(path, "Core", "Utils", "Caching");

        var results = new List<string>
        {
            AddFile(folderPath, "CacheResponse", code_CacheResponse),
            AddFile(folderPath, "CacheService", code_CacheService),
            AddFile(folderPath, "ICacheService", code_ICacheService)
        };

        return string.Join("\n", results);
    }

    public string GenerateUtilsCriticalData(string path)
    {
        string code = @"
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace Core.Utils.CriticalData
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class CriticalDataAttribute : Attribute
    {
    }

    /// <summary>
    /// Json Serilaze Ignore Critical Properties for logs ...
    /// </summary>
    public class IgnoreCriticalDataResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var props = base.CreateProperties(type, memberSerialization);

            return props.Where(p =>
            {
                if (string.IsNullOrEmpty(p.PropertyName)) return false;

                PropertyInfo propertyInfo = type.GetProperty(p.PropertyName);
                if (propertyInfo == null) return true;

                return !Attribute.IsDefined(propertyInfo, typeof(CriticalDataAttribute));
            }).ToList();
        }
    }
}";

        string folderPath = Path.Combine(path, "Core", "Utils", "CriticalData");
        return AddFile(folderPath, "CriticalData", code);
    }

    public string GenerateUtilsCrossCuttingConcerns(string path)
    {
        string code_Extensions = @"
using Castle.DynamicProxy;
using Core.Utils.CriticalData;
using Newtonsoft.Json;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns.Helpers;

public static class Extensions
{
public static TAttribute? GetAttribute<TAttribute>(this IInvocation invocation) where TAttribute : Attribute
{
    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

    return methodInfo.GetCustomAttributes<TAttribute>(true).FirstOrDefault();
}

public static bool HasAttribute<TAttribute>(this IInvocation invocation) where TAttribute : Attribute
{
    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

    var methodAttribute = methodInfo.GetCustomAttributes<TAttribute>(true);
    var classAttribute = invocation.TargetType.GetCustomAttributes<TAttribute>(true);

    return methodAttribute.Any() || classAttribute.Any();
}

public static bool IsAsync(this MethodInfo methodInfo)
{
    return typeof(Task).IsAssignableFrom(methodInfo.ReturnType);
}

public static bool IsGenericAsync(this MethodInfo methodInfo)
{
    var isWithResult =
        methodInfo.ReturnType.IsGenericType &&
        methodInfo.ReturnType.GetGenericTypeDefinition() == typeof(Task<>) &&
        methodInfo.ReturnType.GetGenericArguments().Any();

    return isWithResult;
}

public static bool IsVoid(this MethodInfo methodInfo)
{
    return methodInfo.ReturnType == typeof(void);
}

public static string GetLocation(this IInvocation invocation)
{
    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

    var className = methodInfo.DeclaringType?.FullName ?? ""<UnknownClass>"";
    var methodName = methodInfo.Name;
    return $""{className}.{methodName}"";
}

public static string GetParameters(this IInvocation invocation)
{
    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

    var parameters = methodInfo.GetParameters();
    var arguments = invocation.Arguments;
        
    if (parameters.Length == 0) return ""not found any parameter."";

    var paramsByArgs = parameters.Select((p, i) =>
    {
        var arg = arguments[i];
        if (arg == null) return ""null"";

        Type type = arg.GetType();
            
        if (type == typeof(CancellationToken)) return $""CancellationToken = ..."";

        if (type.IsSimpleType()) return $""{p.Name} = {arg.ToString()}"";

        string serialized = JsonConvert.SerializeObject(arg, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 7,
            ContractResolver = new IgnoreCriticalDataResolver()
        });
        return $""{p.Name} = {serialized}"";
    }).ToArray();

    return ""\n"" + string.Join("", \n\t"", paramsByArgs);
}

public static bool IsSimpleType(this Type type)
{
    if (type.IsPrimitive || type.IsEnum) return true;

    Type[] simpleTypes =
    [
        typeof(string),
        typeof(decimal),
        typeof(DateTime),
        typeof(DateTimeOffset),
        typeof(TimeSpan),
        typeof(Guid),
        typeof(Uri),
        typeof(DateOnly),
        typeof(TimeOnly)
    ];

    return simpleTypes.Contains(type);
}
}";

        string code_CacheInterceptor = @"
using Castle.DynamicProxy;
using Core.Utils.Caching;
using Core.Utils.CrossCuttingConcerns.Helpers;
using Newtonsoft.Json;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns;

public class CacheInterceptor : IInterceptor
{
    private readonly ICacheService _cacheService;
    public CacheInterceptor(ICacheService cacheService) => _cacheService = cacheService;

    public void Intercept(IInvocation invocation)
    {
        if (invocation.HasAttribute<CacheAttribute>())
        {
            HandleIntercept(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void HandleIntercept(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        var attribute = invocation.GetAttribute<CacheAttribute>();
        if (attribute == null) throw new InvalidOperationException(""CacheAttribute not found."");

        if (!methodInfo.IsAsync())
        {
            if (methodInfo.IsVoid())
            {
                InterceptSyncVoid(invocation);
            }
            else
            {
                InterceptSync(invocation, methodInfo, attribute);
            }
        }
        else
        {
            if (!methodInfo.IsGenericAsync())
            {
                invocation.ReturnValue = InterceptAsync(invocation);
            }
            else
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(returnType);
                if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric could not be resolved."");

                invocation.ReturnValue = method.Invoke(this, new object[] { invocation, attribute });
            }
        }
    }

    private void InterceptSyncVoid(IInvocation invocation)
    {
        invocation.Proceed();
    }

    private void InterceptSync(IInvocation invocation, MethodInfo methodInfo, CacheAttribute attribute)
    {
        var cacheKey = GenerateCacheKey(attribute.CacheKey, invocation.Arguments);
        var resultCache = _cacheService.GetFromCache(cacheKey);
        if (resultCache.IsSuccess)
        {
            var source = JsonConvert.DeserializeObject(resultCache.Source!, methodInfo.ReturnType);
            if (source != null)
            {
                invocation.ReturnValue = source;
                return;
            }
        }

        invocation.Proceed();

        if (invocation.ReturnValue != null)
        {
            _cacheService.AddToCache(cacheKey, attribute.CacheGroupKeys, invocation.ReturnValue);
        }
    }

    private async Task InterceptAsync(IInvocation invocation)
    {
        invocation.Proceed();
        var task = (Task)invocation.ReturnValue;
        await task.ConfigureAwait(false);
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, CacheAttribute attribute)
    {
        var cacheKey = GenerateCacheKey(attribute.CacheKey, invocation.Arguments);
        var resultCache = _cacheService.GetFromCache(cacheKey);
        if (resultCache.IsSuccess)
        {
            var source = JsonConvert.DeserializeObject<TResult>(resultCache.Source!);
            if (source != null) return source;
        }

        invocation.Proceed();
        var task = (Task<TResult>)invocation.ReturnValue;
        var result = await task.ConfigureAwait(false);

        if (result != null)
        {
            _cacheService.AddToCache(cacheKey, attribute.CacheGroupKeys, result);
        }
        return result;
    }

    private string GenerateCacheKey(string cacheKey, object[] args)
    {
        if (args.Length > 0)
        {
            var serializedArgs = JsonConvert.SerializeObject(args, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            });
            return $""{cacheKey}-{serializedArgs}"";
        }
        return $""{cacheKey}"";
    }
}


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class CacheAttribute : Attribute
{
    public string CacheKey { get; }
    public string[] CacheGroupKeys { get; }
    public CacheAttribute(string cacheKey, string[] cacheGroupKeys)
    {
        CacheKey = cacheKey;
        CacheGroupKeys = cacheGroupKeys;
    }
}";

        string code_CacheRemoveGroupInterceptor = @"
using Castle.DynamicProxy;
using Core.Utils.Caching;
using Core.Utils.CrossCuttingConcerns.Helpers;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns;

public class CacheRemoveGroupInterceptor : IInterceptor
{
    private readonly ICacheService _cacheService;
    public CacheRemoveGroupInterceptor(ICacheService cacheService) => _cacheService = cacheService;

    public void Intercept(IInvocation invocation)
    {
        if (invocation.HasAttribute<CacheRemoveGroupAttribute>())
        {
            HandleIntercept(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void HandleIntercept(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        var attribute = invocation.GetAttribute<CacheRemoveGroupAttribute>();
        if (attribute == null) throw new InvalidOperationException(""CacheRemoveGroupAttribute not found."");

        if (!methodInfo.IsAsync())
        {
            InterceptSync(invocation, attribute.CacheGroupKeys);
        }
        else
        {
            if (!methodInfo.IsGenericAsync())
            {
                invocation.ReturnValue = InterceptAsync(invocation, attribute.CacheGroupKeys);
            }
            else
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(returnType);
                if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric could not be resolved."");

                invocation.ReturnValue = method.Invoke(this, new object[] { invocation, attribute.CacheGroupKeys });
            }
        }
    }

    private void InterceptSync(IInvocation invocation, string[] cacheGroupKeys)
    {
        _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
        invocation.Proceed();
    }

    private async Task InterceptAsync(IInvocation invocation, string[] cacheGroupKeys)
    {
        _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
        invocation.Proceed();
        var task = (Task)invocation.ReturnValue;
        await task.ConfigureAwait(false);
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, string[] cacheGroupKeys)
    {
        _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
        invocation.Proceed();
        var task = (Task<TResult>)invocation.ReturnValue;
        return await task.ConfigureAwait(false);
    }
}


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public class CacheRemoveGroupAttribute : Attribute
{
    public string[] CacheGroupKeys { get; }
    public CacheRemoveGroupAttribute(string[] cacheGroupKeys) => CacheGroupKeys = cacheGroupKeys;
}";

        string code_CacheRemoveInterceptor = @"
using Castle.DynamicProxy;
using Core.Utils.Caching;
using Core.Utils.CrossCuttingConcerns.Helpers;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns;

public class CacheRemoveInterceptor : IInterceptor
{
    private readonly ICacheService _cacheService;
    public CacheRemoveInterceptor(ICacheService cacheService) => _cacheService = cacheService;

    public void Intercept(IInvocation invocation)
    {
        if (invocation.HasAttribute<CacheRemoveAttribute>())
        {
            HandleIntercept(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void HandleIntercept(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        var attribute = invocation.GetAttribute<CacheRemoveAttribute>();
        if (attribute == null) throw new InvalidOperationException(""CacheRemoveAttribute not found."");

        if (!methodInfo.IsAsync())
        {
            InterceptSync(invocation, attribute.CacheKey);
        }
        else
        {
            if (!methodInfo.IsGenericAsync())
            {
                invocation.ReturnValue = InterceptAsync(invocation, attribute.CacheKey);
            }
            else
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(returnType);
                if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric could not be resolved."");

                invocation.ReturnValue = method.Invoke(this, new object[] { invocation, attribute.CacheKey });
            }
        }
    }

    private void InterceptSync(IInvocation invocation, string cacheKey)
    {
        _cacheService.RemoveFromCache(cacheKey);
        invocation.Proceed();
    }

    private async Task InterceptAsync(IInvocation invocation, string cacheKey)
    {
        _cacheService.RemoveFromCache(cacheKey);
        invocation.Proceed();
        var task = (Task)invocation.ReturnValue;
        await task.ConfigureAwait(false);
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, string cacheKey)
    {
        _cacheService.RemoveFromCache(cacheKey);
        invocation.Proceed();
        var task = (Task<TResult>)invocation.ReturnValue;
        return await task.ConfigureAwait(false);
    }
}

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public class CacheRemoveAttribute : Attribute
{
    public string CacheKey { get; }
    public CacheRemoveAttribute(string cacheKey) => CacheKey = cacheKey;
}";

        string code_DataAccessExceptionHandlerInterceptor = @"
using Castle.DynamicProxy;
using Core.Model;
using Core.Utils.CrossCuttingConcerns.Helpers;
using Core.Utils.ExceptionHandle.Exceptions;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns;

public class DataAccessExceptionHandlerInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        if (invocation.HasAttribute<DataAccessExceptionAttribute>())
        {
            HandleIntercept(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void HandleIntercept(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        if (!methodInfo.IsAsync())
        {
            InterceptSync(invocation);
        }
        else
        {
            if (!methodInfo.IsGenericAsync())
            {
                invocation.ReturnValue = InterceptAsync(invocation);
            }
            else
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(returnType);
                if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric could not be resolved."");

                invocation.ReturnValue = method.Invoke(this, new object[] { invocation });
            }
        }
    }

    private void InterceptSync(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
        }
        catch (Exception exception)
        {
            throw HandleException(exception, invocation);
        }
    }

    private async Task InterceptAsync(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw HandleException(exception, invocation);
        }
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            return await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw HandleException(exception, invocation);
        }
    }

    private Exception HandleException(Exception exception, IInvocation invocation)
    {
        if (exception is IAppException appException)
        {
            appException.LocationName ??= invocation.GetLocation();
            appException.Parameters ??= invocation.GetParameters();

            return exception;
        }

        var message = exception.InnerException != null ? $""Message: {exception.Message} \n InnerException Message: {exception.InnerException.Message})"" : $""Message: {exception.Message} \n "";

        return new DataAccessException(message, exception, invocation.GetLocation(), invocation.GetParameters());
    }
}


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class DataAccessExceptionAttribute : Attribute
{
}";

        string code_ExceptionHandlerInterceptor = @"
using Castle.DynamicProxy;
using Core.Model;
using Core.Utils.CrossCuttingConcerns.Helpers;
using Core.Utils.ExceptionHandle.Exceptions;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns;

public class ExceptionHandlerInterceptor : IInterceptor
{
    public void Intercept(IInvocation invocation)
    {
        if (invocation.HasAttribute<ExceptionHandlerAttribute>())
        {
            HandleIntercept(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    private void HandleIntercept(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        if (!methodInfo.IsAsync())
        {
            InterceptSync(invocation);
        }
        else
        {
            if (!methodInfo.IsGenericAsync())
            {
                invocation.ReturnValue = InterceptAsync(invocation);
            }
            else
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(returnType);
                if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric could not be resolved."");

                invocation.ReturnValue = method.Invoke(this, new object[] { invocation });
            }
        }
    }

    private void InterceptSync(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
        }
        catch (Exception exception)
        {
            throw HandleException(exception, invocation);
        }
    }

    private async Task InterceptAsync(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
            var task = (Task)invocation.ReturnValue;
            await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw HandleException(exception, invocation);
        }
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation)
    {
        try
        {
            invocation.Proceed();
            var task = (Task<TResult>)invocation.ReturnValue;
            return await task.ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            throw HandleException(exception, invocation);
        }
    }

    private Exception HandleException(Exception exception, IInvocation invocation)
    {
        if (exception is IAppException appException)
        {
            appException.LocationName ??= invocation.GetLocation();
            appException.Parameters ??= invocation.GetParameters();

            return exception;
        }

        var message = exception.InnerException != null ? $""Message: {exception.Message} \n InnerException Message: {exception.InnerException.Message})"" : $""Message: {exception.Message} \n "";

        return new GeneralException(message, exception, invocation.GetLocation(), invocation.GetParameters());
    }
}


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = false, Inherited = true)]
public class ExceptionHandlerAttribute : Attribute
{
}";

        string code_ValidationInterceptor = @"
using Castle.DynamicProxy;
using Core.Utils.CrossCuttingConcerns.Helpers;
using Core.Utils.ExceptionHandle.Exceptions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Core.Utils.CrossCuttingConcerns;

public class ValidationInterceptor : IInterceptor
{
    private readonly IServiceProvider _serviceProvider;
    public ValidationInterceptor(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;

    public void Intercept(IInvocation invocation)
    {
        if (invocation.HasAttribute<ValidationAttribute>())
        {
            HandleIntercept(invocation);
        }
        else
        {
            invocation.Proceed();
        }
    }

    public void HandleIntercept(IInvocation invocation)
    {
        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;

        var attribute = invocation.GetAttribute<ValidationAttribute>();
        if (attribute == null) throw new InvalidOperationException(""ValidationAttribute not found."");

        if (!methodInfo.IsAsync())
        {
            InterceptSync(invocation, attribute);
        }
        else
        {
            if (!methodInfo.IsGenericAsync())
            {
                invocation.ReturnValue = InterceptAsync(invocation, attribute);
            }
            else
            {
                var returnType = methodInfo.ReturnType.GetGenericArguments()[0];

                var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance)?.MakeGenericMethod(returnType);
                if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric could not be resolved."");

                invocation.ReturnValue = method.Invoke(this, new object[] { invocation, attribute });
            }
        }
    }

    private void InterceptSync(IInvocation invocation, ValidationAttribute attribute)
    {
        CheckValidation(invocation, attribute.TargetType);
        invocation.Proceed();
    }

    private async Task InterceptAsync(IInvocation invocation, ValidationAttribute attribute)
    {
        CheckValidation(invocation, attribute.TargetType);
        invocation.Proceed();
        var task = (Task)invocation.ReturnValue;
        await task.ConfigureAwait(false);
    }

    private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, ValidationAttribute attribute)
    {
        CheckValidation(invocation, attribute.TargetType);
        invocation.Proceed();
        var task = (Task<TResult>)invocation.ReturnValue;
        return await task.ConfigureAwait(false);
    }

    private void CheckValidation(IInvocation invocation, Type targetType)
    {
        var arg = invocation.Arguments.FirstOrDefault(arg => arg?.GetType() == targetType);
        if (arg == null) throw new InvalidOperationException(""Arg object to validation could not be determined."");

        var validatorsType = typeof(IEnumerable<>).MakeGenericType(typeof(IValidator<>).MakeGenericType(targetType));

        var validators = (IEnumerable<IValidator>)_serviceProvider.GetRequiredService(validatorsType);
        if (!validators.Any()) return;

        var context = (IValidationContext)Activator.CreateInstance(typeof(ValidationContext<>).MakeGenericType(targetType), arg)!;
        if (context == null) throw new InvalidOperationException(""ValidationContext could not be created."");

        IEnumerable<ValidationFailure> failures = validators
            .Select(validator => validator.Validate(context))
            .Where(result => !result.IsValid)
            .SelectMany(result => result.Errors)
            .ToList();

        
        if (failures.Any()){
            string message = ""Validation Error(s):\n\t"" + string.Join("",\n\t"", failures.Select(f => $""{f.PropertyName}: {f.ErrorMessage}""));
            throw new ValidationRuleException(message, failures, invocation.GetLocation(), invocation.GetParameters());
        }
    }
}


[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
public class ValidationAttribute : Attribute
{
    public Type TargetType { get; }
    public ValidationAttribute(Type targetType) => TargetType = targetType;
}";


        string folderPath = Path.Combine(path, "Core", "Utils", "CrossCuttingConcerns");
        string folderPathExtensions = Path.Combine(path, "Core", "Utils", "CrossCuttingConcerns", "Helpers");

        var results = new List<string>
        {
            AddFile(folderPathExtensions, "Extensions", code_Extensions),
            AddFile(folderPath, "CacheInterceptor", code_CacheInterceptor ),
            AddFile(folderPath, "CacheRemoveGroupInterceptor", code_CacheRemoveGroupInterceptor ),
            AddFile(folderPath, "CacheRemoveInterceptor", code_CacheRemoveInterceptor ),
            AddFile(folderPath, "DataAccessExceptionHandlerInterceptor", code_DataAccessExceptionHandlerInterceptor ),
            AddFile(folderPath, "ExceptionHandlerInterceptor", code_ExceptionHandlerInterceptor),
            AddFile(folderPath, "ValidationInterceptor", code_ValidationInterceptor ),
        };

        return string.Join("\n", results);
    }

    public string GenerateUtilsDatatable(string path)
    {
        string code_DatatableRequest = @"
namespace Core.Utils.Datatable;

public class DatatableRequest
{
    public int Draw { get; set; }
    public int Start { get; set; }
    public int Length { get; set; }
    public Search? Search { get; set; }
    public List<Order>? Order { get; set; }
    public List<Column>? Columns { get; set; }

    public DatatableRequest GetDatatableRequest()
    {
        return this;
    }
}


public class Search
{
    public string? Value { get; set; }
    public bool Regex { get; set; }
}

public class Order
{
    public int Column { get; set; }
    public string? Dir { get; set; }
}

public class Column
{
    public string? Data { get; set; }
    public string? Name { get; set; }
    public bool Searchable { get; set; } = false;
    public bool Orderable { get; set; }
    public Search? Search { get; set; }
}";

        string code_DatatableResponseClientSide = @"
namespace Core.Utils.Datatable;

public class DatatableResponseClientSide<TData>
{
    public List<TData>? Data { get; set; }
}";

        string code_DatatableResponseServerSide = @"
namespace Core.Utils.Datatable;

public class DatatableResponseServerSide<TData>
{
    public int Draw { get; set; }
    public int RecordsTotal { get; set; }
    public int RecordsFiltered { get; set; }
    public List<TData>? Data { get; set; }
}";

        string code_QueryableDatatableExtension = @"
using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Core.Utils.Datatable;

public static class QueryableDatatableExtension
{
    #region Server-Side Extension Methods
    // ***************** Sync Version *****************
    public static DatatableResponseServerSide<TData> ToDatatableServerSide<TData>(this IQueryable<TData> query, DatatableRequest dataTableRequest)
    {
        if (dataTableRequest == null) throw new ArgumentNullException(nameof(dataTableRequest));

        // 1. Count of Total Records
        int recordsTotal = query.Count();

        // 2. Filter by search parameter
        string? searchPredicate = GenerateSearchPredicate<TData>(dataTableRequest);
        if (searchPredicate != null) query = query.Where(searchPredicate, dataTableRequest.Search!.Value!.ToLower());

        // 3. Count of Filtered Records
        int recordsFiltered = query.Count();

        // 4. Ordering 
        string? orderPredicate = GenerateOrderPredicate<TData>(dataTableRequest);
        if (orderPredicate != null) query = query.OrderBy(orderPredicate);

        // 5. Pagination
        query = query.Skip(dataTableRequest.Start).Take(dataTableRequest.Length);

        return new DatatableResponseServerSide<TData>
        {
            Data = query.ToList(),
            Draw = dataTableRequest.Draw,
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
        };
    }

    // ***************** Async Version *****************
    public static async Task<DatatableResponseServerSide<TData>> ToDatatableServerSideAsync<TData>(this IQueryable<TData> query, DatatableRequest dataTableRequest, CancellationToken cancellationToken = default)
    {
        if (dataTableRequest == null) throw new ArgumentNullException(nameof(dataTableRequest));

        // 1. Count of Total Records
        int recordsTotal = await query.CountAsync();

        // 2. Filter by search parameter
        string? searchPredicate = GenerateSearchPredicate<TData>(dataTableRequest);
        if (searchPredicate != null) query = query.Where(searchPredicate, dataTableRequest.Search!.Value!.ToLower());

        // 3. Count of Filtered Records
        int recordsFiltered = await query.CountAsync();

        // 4. Ordering 
        string? orderPredicate = GenerateOrderPredicate<TData>(dataTableRequest);
        if (orderPredicate != null) query = query.OrderBy(orderPredicate);

        // 5. Pagination
        query = query.Skip(dataTableRequest.Start).Take(dataTableRequest.Length);

        var data = await query.ToListAsync(cancellationToken);

        return new DatatableResponseServerSide<TData>
        {
            Data = data,
            Draw = dataTableRequest.Draw,
            RecordsTotal = recordsTotal,
            RecordsFiltered = recordsFiltered,
        };
    }
    #endregion


    #region Client-Side Extension Methods
    // ***************** Sync Version *****************
    public static DatatableResponseClientSide<TData> ToDatatableClientSide<TData>(this IQueryable<TData> query)
    {
        return new DatatableResponseClientSide<TData>()
        {
            Data = query.ToList(),
        };
    }

    // ***************** Async Version *****************
    public static async Task<DatatableResponseClientSide<TData>> ToDatatableClientSideAsync<TData>(this IQueryable<TData> query, CancellationToken cancellationToken = default)
    {
        var data = await query.ToListAsync(cancellationToken);
        return new DatatableResponseClientSide<TData>()
        {
            Data = data,
        };
    }
    #endregion


    // ################# Helper Methods #################
    private static string? GenerateSearchPredicate<TData>(DatatableRequest dataTableRequest)
    {
        if (dataTableRequest.Search == null || string.IsNullOrEmpty(dataTableRequest.Search.Value) || dataTableRequest.Columns == null) return null;

        var props = typeof(TData).GetProperties().Select(p => p.Name).ToDictionary(p => p.ToLower(), p => p);

        IEnumerable<Column>? searchableColumns = dataTableRequest.Columns!.Where(c => c.Searchable && !string.IsNullOrEmpty(c.Data));

        foreach (var column in searchableColumns) // c.Data is column name
        {
            var key = column.Data!.ToLower();
            if (props.TryGetValue(key, out var actualPropName))
            {
                column.Data = actualPropName;
            }
        }
        var filters = searchableColumns.Select(c => $""{c.Data}.Contains(@0)"");

        var searchPredicate = string.Join("" OR "", filters);
        return searchPredicate;
    }

    private static string? GenerateOrderPredicate<TData>(DatatableRequest dataTableRequest)
    {
        if (dataTableRequest.Order == null || dataTableRequest.Columns == null) return null;
        
        var props = typeof(TData).GetProperties().Select(p => p.Name).ToDictionary(p => p.ToLower(), p => p);

        List<string> orderList = new List<string>();
        foreach (var orderItem in dataTableRequest.Order)
        {
            var column = dataTableRequest.Columns[orderItem.Column];
            if (column == null || !column.Orderable || string.IsNullOrEmpty(column.Data)) continue;

            var key = column.Data.ToLower();
            if (props.TryGetValue(key, out var actualPropName))
            {
                orderList.Add($""{actualPropName} {orderItem.Dir}"");
            }
        }
        string orderPredicate = string.Join("","", orderList);

        if (orderList.Any()) return orderPredicate;
        return null;
    }
}";

        string folderPath = Path.Combine(path, "Core", "Utils", "Datatable");

        var results = new List<string>
        {
            AddFile(folderPath, "DatatableRequest", code_DatatableRequest),
            AddFile(folderPath, "DatatableResponseClientSide", code_DatatableResponseClientSide),
            AddFile(folderPath, "DatatableResponseServerSide", code_DatatableResponseServerSide ),
            AddFile(folderPath, "QueryableDatatableExtension", code_QueryableDatatableExtension )
        };

        return string.Join("\n", results);
    }

    public string GenerateUtilsDynamicQuery(string path)
    {
        string code_Filter = @"
namespace Core.Utils.DynamicQuery;

public class Filter
{
    public string? Field { get; set; }
    public string? Operator { get; set; }
    public string? Value { get; set; }
    public string? Logic { get; set; }
    public List<Filter>? Filters { get; set; }
}";

        string code_QueryableFilterExtension = @"
using System.Linq.Dynamic.Core;
using System.Text;

namespace Core.Utils.DynamicQuery;

public static class QueryableFilterExtension
{
    private static readonly string[] _logics = { ""and"", ""or"" };
    private static readonly IDictionary<string, string> _operators = new Dictionary<string, string>
    {
        { ""base"", "" "" },
        { ""eq"", ""="" },
        { ""neq"", ""!="" },
        { ""lt"", ""<"" },
        { ""lte"", ""<="" },
        { ""gt"", "">"" },
        { ""gte"", "">="" },
        { ""isnull"", ""== null"" },
        { ""isnotnull"", ""!= null"" },
        { ""startswith"", ""StartsWith"" },
        { ""endswith"", ""EndsWith"" },
        { ""contains"", ""Contains"" },
        { ""doesnotcontain"", ""Contains"" }
    };

    public static IQueryable<T> ToFilter<T>(this IQueryable<T> queryable, Filter filter)
    {
        List<Filter> filterList = new();
        GetFilters(filterList, filter);

        foreach (Filter item in filterList) // Validation before processes
        {
            if (item.Operator == ""base"" && _logics.Contains(item.Logic)) continue;
            if (string.IsNullOrEmpty(item.Field))
                throw new ArgumentException(""Empty Field For Filter Process"");
            if (string.IsNullOrEmpty(item.Operator) || !_operators.ContainsKey(item.Operator))
                throw new ArgumentException(""Invalid Opreator Type For Filter Process"");
            if (string.IsNullOrEmpty(item.Value) && (item.Operator == ""isnull"" || item.Operator == ""isnotnull"")) // those operators do not need value
                throw new ArgumentException(""Invalid Value For Filter Process"");
            if (string.IsNullOrEmpty(item.Logic) == false && _logics.Contains(item.Logic) == false)
                throw new ArgumentException(""Invalid Logic Type For Filter Process"");
        }

        string?[] values = filterList.Select(f => f.Value).ToArray();
        string where = Transform(filter, filterList);

        if (!string.IsNullOrWhiteSpace(where))
            queryable = queryable.Where(where, values);
        return queryable;
    }

    private static void GetFilters(IList<Filter> filterList, Filter filter)
    {
        filterList.Add(filter);
        if (filter.Filters is not null && filter.Filters.Any())
            foreach (Filter item in filter.Filters)
                GetFilters(filterList, item);
    }

    public static string Transform(Filter filter, IList<Filter> filters)
    {
        int index = filters.IndexOf(filter);
        string comparison = _operators[filter.Operator!];
        StringBuilder where = new();

        switch (filter.Operator)
        {
            case ""base"":
                where.Append($"" "");
                break;
            case ""eq"":
                where.Append($""np({filter.Field}) == @{index}"");
                break;
            case ""neq"":
                where.Append($""np({filter.Field}) != @{index}"");
                break;
            case ""lt"":
                where.Append($""np({filter.Field}) < @{index}"");
                break;
            case ""lte"":
                where.Append($""np({filter.Field}) <= @{index}"");
                break;
            case ""gt"":
                where.Append($""np({filter.Field}) > @{index}"");
                break;
            case ""gte"":
                where.Append($""np({filter.Field}) >= @{index}"");
                break;
            case ""isnull"":
                where.Append($""np({filter.Field}) == null"");
                break;
            case ""isnotnull"":
                where.Append($""np({filter.Field}) != null"");
                break;
            case ""startswith"":
                where.Append($""np({filter.Field}).StartsWith(@{index})"");
                break;
            case ""endswith"":
                where.Append($""np({filter.Field}).EndsWith(@{index})"");
                break;
            case ""contains"":
                where.Append($""np({filter.Field}).Contains(@{index})"");
                break;
            case ""doesnotcontain"":
                where.Append($""!np({filter.Field}).Contains(@{index})"");
                break;
            default:
                throw new ArgumentException($""Invalid Operator Type For Filter Process ({filter.Operator})"");
        }



        if (filter.Logic is not null && filter.Filters is not null && filter.Filters.Any())
        {
            string baseLogic = filter.Operator == ""base"" ? """" : filter.Logic;
            return $""({where} {baseLogic} {string.Join(separator: $"" {filter.Logic} "", value: filter.Filters.Select(f => Transform(f, filters)).ToArray())})"";
        }

        return where.ToString();
    }
}";

        string code_QueryableSortExtension = @"
using System.Linq.Dynamic.Core;

namespace Core.Utils.DynamicQuery;

public static class QueryableSortExtension
{
    private static readonly string[] _orderDirs = { ""asc"", ""desc"" };
    
    public static IQueryable<T> ToSort<T>(this IQueryable<T> queryable, IEnumerable<Sort> sorts)
    {
        if (sorts is not null)
        {
            foreach (Sort item in sorts)
            {
                if (string.IsNullOrEmpty(item.Field)) throw new ArgumentException(""Empty Field For Sorting Process"");
                if (string.IsNullOrEmpty(item.Dir) || !_orderDirs.Contains(item.Dir)) throw new ArgumentException(""Invalid Order Type For Sorting Process"");
            }

            string ordering = string.Join(separator: "","", values: sorts.Select(s => $""{s.Field} {s.Dir}""));
            return queryable.OrderBy(ordering);
        }

        return queryable;
    }
}";

        string code_Sort = @"
namespace Core.Utils.DynamicQuery;

public class Sort
{
    public string? Field { get; set; }
    public string? Dir { get; set; }
}";

        string folderPath = Path.Combine(path, "Core", "Utils", "DynamicQuery");

        var results = new List<string>
        {
            AddFile(folderPath, "Filter", code_Filter),
            AddFile(folderPath, "QueryableFilterExtension", code_QueryableFilterExtension),
            AddFile(folderPath, "QueryableSortExtension", code_QueryableSortExtension),
            AddFile(folderPath, "Sort", code_Sort)
        };

        return string.Join("\n", results);
    }

    public string GenerateUtilsExceptionHandle(string path)
    {
        string code_BusinessException = @"
using Core.Model;

namespace Core.Utils.ExceptionHandle.Exceptions;

public class BusinessException : Exception, IAppException
{
    public string? LocationName { get; set; }
    public string? Parameters { get; set; }
    public string? Description { get; set; }

    public BusinessException(string message, string? locationName = default, string? parameters = default, string? description = default) : base(message)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }

    public BusinessException(string message, Exception innerException, string? locationName = default, string? parameters = default, string? description = default) : base(message, innerException)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }
}";

        string code_DataAccessException = @"
using Core.Model;

namespace Core.Utils.ExceptionHandle.Exceptions;

public class DataAccessException : Exception, IAppException
{
    public string? LocationName { get; set; }
    public string? Parameters { get; set; }
    public string? Description { get; set; }

    public DataAccessException(string message, string? locationName = default, string? parameters = default, string? description = default) : base(message)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }

    public DataAccessException(string message, Exception innerException, string? locationName = default, string? parameters = default, string? description = default) : base(message, innerException)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }
}";

        string code_GeneralException = @"
using Core.Model;

namespace Core.Utils.ExceptionHandle.Exceptions;

public class GeneralException : Exception, IAppException
{
    public string? LocationName { get; set; }
    public string? Parameters { get; set; }
    public string? Description { get; set; }

    public GeneralException(string message, string? locationName = default, string? parameters = default, string? description = default) : base(message)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }

    public GeneralException(string message, Exception innerException, string? locationName = default, string? parameters = default, string? description = default) : base(message, innerException)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }
}";

        string code_ValidationRuleException = @"
using Core.Model;
using FluentValidation;
using FluentValidation.Results;

namespace Core.Utils.ExceptionHandle.Exceptions;

public class ValidationRuleException : ValidationException, IAppException
{
    public string? LocationName { get; set; }
    public string? Parameters { get; set; }
    public string? Description { get; set; }

    public ValidationRuleException(string message, string? locationName = default, string? parameters = default, string? description = default) : base(message)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }

    public ValidationRuleException(IEnumerable<ValidationFailure> errors, string? locationName = default, string? parameters = default, string? description = default) : base(errors)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }

    public ValidationRuleException(string message, IEnumerable<ValidationFailure> errors, string? locationName = default, string? parameters = default, string? description = default) : base(message, errors)
    {
        LocationName = locationName;
        Parameters = parameters;
        Description = description;
    }
}";

        string code_BusinessProblemDetails = @"
using Microsoft.AspNetCore.Mvc;

namespace Core.Utils.ExceptionHandle.ProblemDetailModels;

public class BusinessProblemDetails : ProblemDetails
{
}";

        string code_DataAccessProblemDetails = @"
using Microsoft.AspNetCore.Mvc;

namespace Core.Utils.ExceptionHandle.ProblemDetailModels;

public class DataAccessProblemDetails : ProblemDetails
{
}";

        string code_ValidationProblemDetails = @"
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;

namespace Core.Utils.ExceptionHandle.ProblemDetailModels;

public class ValidationProblemDetails : ProblemDetails
{
    public IEnumerable<ValidationFailure>? Errors { get; set; }
}";

        string folderPath_exceptions = Path.Combine(path, "Core", "Utils", "ExceptionHandle", "Exceptions");
        string folderPath_problemDetails = Path.Combine(path, "Core", "Utils", "ExceptionHandle", "ProblemDetailModels");

        var results = new List<string>
        {
            AddFile(folderPath_exceptions, "BusinessException", code_BusinessException),
            AddFile(folderPath_exceptions, "DataAccessException", code_DataAccessException ),
            AddFile(folderPath_exceptions, "GeneralException", code_GeneralException ),
            AddFile(folderPath_exceptions, "ValidationRuleException", code_ValidationRuleException ),
            AddFile(folderPath_problemDetails, "BusinessProblemDetails", code_BusinessProblemDetails),
            AddFile(folderPath_problemDetails, "DataAccessProblemDetails", code_DataAccessProblemDetails ),
            AddFile(folderPath_problemDetails, "ValidationProblemDetails", code_ValidationProblemDetails )
        };

        return string.Join("\n", results);
    }

    public string GenerateUtilsHttpContextManager(string path)
    {
        string code = @"
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Core.Utils.HttpContextManager;

public class HttpContextManager
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public HttpContextManager(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    public string? GetUserId()
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.GetUserId!"");

        return _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
    public string? GetUserAgent()
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.GetUserAgent!"");

        return _httpContextAccessor.HttpContext.Request.Headers.UserAgent.ToString();
    }
    public string? GetClientIp()
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.GetClientIp!"");

        if (_httpContextAccessor.HttpContext.Request.Headers.ContainsKey(""X-Forwarded-For""))
            return _httpContextAccessor.HttpContext.Request.Headers[""X-Forwarded-For""];
        return _httpContextAccessor.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
    public bool IsMobile()
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.IsMobile!"");

        var appPlatform = _httpContextAccessor.HttpContext.Request.Headers[""X-App-Platform""];
        return appPlatform.ToString().ToLowerInvariant() == ""mobile"".ToLowerInvariant();
    }
    public void AddRefreshTokenToCookie(string refreshToken, DateTime expirationUtc)
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.AddRefreshTokenToCookie!"");

        _httpContextAccessor.HttpContext.Response.Cookies.Append(""RefreshToken"", refreshToken, new CookieOptions
        {
            Secure = true,
            HttpOnly = true,
            Expires = expirationUtc,
            SameSite = SameSiteMode.Strict,
            //Path = ""/Account/RefreshAuth""
        });
    }
    public string GetRefreshTokenFromCookie()
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.GetRefreshTokenFromCookie!"");

        string? refreshToken = _httpContextAccessor.HttpContext.Request.Cookies[""RefreshToken""];
        if (string.IsNullOrEmpty(refreshToken)) throw new Exception(""Not exist refresh token inside cookie!"");

        return refreshToken;
    }
    public void DeletetRefreshTokenFromCookie()
    {
        if (_httpContextAccessor.HttpContext == null) throw new Exception(""Not exist HttpContext inside HttpContextManager.DeletetRefreshTokenFromCookie!"");

        _httpContextAccessor.HttpContext?.Response.Cookies.Delete(""Key_RefreshToken"");
    }
}";

        string folderPath = Path.Combine(path, "Core", "Utils", "HttpContextManager");
        return AddFile(folderPath, "HttpContextManager", code);
    }

    public string GenerateUtilsPagination(string path)
    {
        string code_PaginationRequest = @"
namespace Core.Utils.Pagination;

public class PaginationRequest
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public PaginationRequest()
    {
    }

    public PaginationRequest(int page, int pageSize)
    {
        Page = page;
        PageSize = pageSize;
    }
}";

        string code_PaginationResponse = @"
namespace Core.Utils.Pagination;

public class PaginationResponse<TData>
{
    public PaginationResponse()
    {
        Data = Array.Empty<TData>();
    }

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int DataCount { get; set; }
    public int PageCount { get; set; }
    public IList<TData> Data { get; set; }
    public bool HasPrevious => Page > 0;
    public bool HasNext => Page + 1 < PageCount;
}";

        string code_QueryablePaginationExtension = @"
using Microsoft.EntityFrameworkCore;

namespace Core.Utils.Pagination;

public static class QueryablePaginationExtension
{
    public static PaginationResponse<TData> ToPaginate<TData>(this IQueryable<TData> queryable, PaginationRequest request)
    {
        int count = queryable.Count();

        if (request.Page == default || request.Page <= 0) request.Page = 1;
        if (request.PageSize == default || request.PageSize <= 0) request.PageSize = count;

        List<TData> items = queryable.Skip((request.Page -1) * request.PageSize).Take(request.PageSize).ToList();
        PaginationResponse<TData> list = new()
        {
            Page = request.Page,
            PageSize = request.PageSize,
            DataCount = count,
            Data = items,
            PageCount = (int)Math.Ceiling(count / (double)request.PageSize)
        };
        return list;
    }
 
    public static async Task<PaginationResponse<TData>> ToPaginateAsync<TData>(this IQueryable<TData> queryable, PaginationRequest request, CancellationToken cancellationToken = default)
    {
        int count = await queryable.CountAsync(cancellationToken);

        if (request.Page == default || request.Page <= 0) request.Page = 1;
        if (request.PageSize == default || request.PageSize <= 0) request.PageSize = count;

        List<TData> items = await queryable.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        return new PaginationResponse<TData>
        {
            Page = request.Page,
            PageSize = request.PageSize,
            DataCount = count,
            Data = items,
            PageCount = (int)Math.Ceiling(count / (double)request.PageSize)
        };
    }
}";

        string folderPath = Path.Combine(path, "Core", "Utils", "Pagination");

        var results = new List<string>
        {
            AddFile(folderPath, "PaginationRequest", code_PaginationRequest ),
            AddFile(folderPath, "PaginationResponse", code_PaginationResponse ),
            AddFile(folderPath, "QueryablePaginationExtension", code_QueryablePaginationExtension)
        };

        return string.Join("\n", results);
    }
    #endregion

    public string GenerateServiceRegistrations(string path)
    {
        string code_AutofacModule = @"
using Autofac;
using Core.Utils.CrossCuttingConcerns;

namespace Core;

public class AutofacModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        // ******** Interceptors **********
        builder.RegisterType<ValidationInterceptor>();
        builder.RegisterType<CacheInterceptor>();
        builder.RegisterType<CacheRemoveInterceptor>();
        builder.RegisterType<CacheRemoveGroupInterceptor>();
        builder.RegisterType<ExceptionHandlerInterceptor>();
        builder.RegisterType<DataAccessExceptionHandlerInterceptor>();
    }
}";

        string code_ServiceRegistration = @"
using Core.Utils.Caching;
using Core.Utils.CriticalData;
using Core.Utils.HttpContextManager;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;

namespace Core;

public static class ServiceRegistration
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddSingleton<HttpContextManager>();

        JsonConvert.DefaultSettings = () => new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            MaxDepth = 7,
            ContractResolver = new IgnoreCriticalDataResolver()
        };

        #region Distributed Cache In Memory
        services.AddDistributedMemoryCache();
        // services.AddStackExchangeRedisCache(options =>
        // {
        //     options.Configuration = configuration[""Redis:ConnectionString""];
        // });
        services.AddSingleton<ICacheService, CacheService>();
        #endregion

        return services;
    }
}";

        string folderPath = Path.Combine(path, "Core");

        var results = new List<string>
        {
            AddFile(folderPath, "AutofacModule", code_AutofacModule ),
            AddFile(folderPath, "ServiceRegistration", code_ServiceRegistration)
        };

        return string.Join("\n", results);
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
                return $"OK: File {fileName} added to Core project.";
            }
            else
            {
                return $"INFO: File {fileName} already exists in Core project.";
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"ERROR: An error occurred while adding file({fileName}) to Core project. \n Details:{ex.Message}");
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
