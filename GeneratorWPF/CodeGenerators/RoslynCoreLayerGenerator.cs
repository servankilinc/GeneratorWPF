using System.Diagnostics;
using System.IO;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace GeneratorWPF.CodeGenerators;

public static class RoslynCoreLayerGenerator
{
    public static string GenerateProject(string path)
    {
        string corePath = Path.Combine(path, "Core");

        if (!Directory.Exists(corePath))
        {
            Directory.CreateDirectory(corePath);
            return RunCommand(corePath, "dotnet", "new classlib");
        }
        return "Core layer project already exists.";
    }

    #region Model
    public static void GenerateIDto(string path)
    {
        string code = @"
            namespace Core.Model;

            public abstract class IDto
            {
                // ... signature class
            }";

        string folderPath = Path.Combine(path, "Core", "Model");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "IDto.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateIEntity(string path)
    {
        string code = @"
            namespace Core.Model;

            public abstract class IEntity
            {
                // ... signature class
            }";

        string folderPath = Path.Combine(path, "Core", "Model");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "IEntity.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    #endregion

    #region Utils/Auth
    public static void GenerateHashHelper(string path)
    {
        string code = @"
            using System.Security.Cryptography;
            using System.Text;

            namespace Core.Utils.Auth.Hashing;

            public static class HashingHelper
            {
                public static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
                {
                    using (HMACSHA512 hmac = new())
                    {
                        passwordSalt = hmac.Key;
                        passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                    }
                }

                public static bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
                {
                    using (HMACSHA512 hmac = new(passwordSalt))
                    {
                        byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                        for (int i = 0; i < computedHash.Length; i++)
                            if (computedHash[i] != passwordHash[i])
                                return false;
                    }

                    return true;
                }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Auth", "Hashing");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "HashingHelper.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateAccessToken(string path)
    {
        string code = @"
            namespace Core.Utils.Auth;

            public record AccessToken(string Token, DateTime Expiration);";

        string folderPath = Path.Combine(path, "Core", "Utils", "Auth");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "AccessToken.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateAutheticatorType(string path)
    {
        string code = @"
            namespace Core.Utils.Auth;

            public enum AutheticatorType
            {
                None = 0,
                Email = 1,
                Google = 2,
                Facebook = 3,
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Auth");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "AutheticatorType.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateTokenOptions(string path)
    {
        string code = @"
            namespace Core.Utils.Auth;

            public class TokenOptions
            {
                public string Audience { get; set; } = null!;
                public string Issuer { get; set; } = null!;
                public string SecurityKey { get; set; } = null!;
                public int AccessTokenExpiration { get; set; }
                public int RefreshTokenTTL { get; set; }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Auth");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "TokenOptions.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    #endregion

    #region Utils/Cache
    public static void GenerateCacheResponse(string path)
    {
        string code = @"
            namespace Core.Utils.Caching;

            public record CacheResponse(bool IsSuccess, string? Source = default);";

        string folderPath = Path.Combine(path, "Core", "Utils", "Caching");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "CacheResponse.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateICacheService(string path)
    {
        string code = @"
            namespace Core.Utils.Caching;

            public interface ICacheService
            {
                CacheResponse GetFromCache(string cacheKey);
                void AddToCache<TData>(string CacheKey, string[] CacheGroupKeys, TData data);
                void RemoveFromCache(string CacheKey);
                void RemoveCacheGroupKeys(string[] cacheGroupKeys);
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Caching");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "ICacheService.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateCacheService(string path)
    {
        string code = @"
            using Microsoft.Extensions.Caching.Distributed;
            using Microsoft.Extensions.Logging;
            using System.Text;
            using System.Text.Json;

            namespace Core.Utils.Caching;

            public class CacheService : ICacheService
            {
                private readonly IDistributedCache _distributedCache;
                private readonly ILogger<CacheService> _logger;
                public CacheService(IDistributedCache distributedCache, ILogger<CacheService> logger)
                {
                    _distributedCache = distributedCache;
                    _logger = logger;
                }


                public CacheResponse GetFromCache(string cacheKey)
                {
                    if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(cacheKey);

                    byte[]? cachedData = _distributedCache.Get(cacheKey);
                    if (cachedData != null)
                    {
                        var response = Encoding.UTF8.GetString(cachedData);
                        if (string.IsNullOrEmpty(response)) return new CacheResponse(IsSuccess: false);

                        _logger.LogInformation($""CacheService Get, Successfully => key: ({cacheKey}), data: {response}"");
                        return new CacheResponse(IsSuccess: true, Source: response);
                    }
                    else
                    {
                        _logger.LogInformation($""CacheService Get, Couldnt Found => key: ({cacheKey})"");
                        return new CacheResponse(IsSuccess: false);
                    }
                }


                public void AddToCache<TData>(string cacheKey, string[] cacheGroupKeys, TData data)
                {
                    if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(cacheKey);

                    DistributedCacheEntryOptions cacheEntryOptions = new DistributedCacheEntryOptions()
                    {
                        SlidingExpiration = TimeSpan.FromDays(1),
                        AbsoluteExpiration = DateTime.Now.AddDays(5)
                    };

                    string serializedData = JsonSerializer.Serialize(data);
                    byte[]? bytedData = Encoding.UTF8.GetBytes(serializedData);

                    _distributedCache.Set(cacheKey, bytedData, cacheEntryOptions);
                    _logger.LogInformation($""CacheService Add, Successfully => key: ({cacheKey}), data: {serializedData}"");

                    if (cacheGroupKeys.Length > 0) AddCacheKeyToGroups(cacheKey, cacheGroupKeys, cacheEntryOptions);
                }


                public void RemoveFromCache(string cacheKey)
                {
                    if (string.IsNullOrWhiteSpace(cacheKey)) throw new ArgumentNullException(cacheKey);

                    _distributedCache.Remove(cacheKey);
                    _logger.LogInformation($""CacheService Remove, Successfully => key: ({cacheKey})"");
                }


                public void RemoveCacheGroupKeys(string[] cacheGroupKeyList)
                {
                    if (cacheGroupKeyList.Length == 0) throw new ArgumentNullException(nameof(cacheGroupKeyList));

                    foreach (string cacheGroupKey in cacheGroupKeyList)
                    {
                        byte[]? keyListFromCache = _distributedCache.Get(cacheGroupKey);
                        _distributedCache.Remove(cacheGroupKey);

                        if (keyListFromCache == null)
                        {
                            _logger.LogInformation($""CacheService Group Remove, Successfully (but not exist any key !!!) => groupKey: ({cacheGroupKey})"");
                            continue;
                        }

                        string stringKeyList = Encoding.Default.GetString(keyListFromCache);
                        HashSet<string>? keyListInGroup = JsonSerializer.Deserialize<HashSet<string>>(stringKeyList);
                        if (keyListInGroup != null)
                        {
                            foreach (var key in keyListInGroup)
                            {
                                _distributedCache.Remove(key);
                            }
                        }

                        _logger.LogInformation($""CacheService Group Remove, Successfully => groupKey: ({cacheGroupKey}) keyList: ({stringKeyList})"");
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
                            keyListInGroup = JsonSerializer.Deserialize<HashSet<string>>(Encoding.Default.GetString(cachedGroupData));
                            if (keyListInGroup != null && !keyListInGroup.Contains(cacheKey))
                            {
                                keyListInGroup.Add(cacheKey);
                            }
                        }
                        else
                        {
                            keyListInGroup = new HashSet<string>(new[] { cacheKey });
                        }
                        string serializedData = JsonSerializer.Serialize(keyListInGroup);
                        byte[]? bytedKeyList = Encoding.UTF8.GetBytes(serializedData); 
                        //byte[]? bytedKeyList = JsonSerializer.SerializeToUtf8Bytes(keyListInGroup);

                        _distributedCache.Set(cacheGroupKey, bytedKeyList, groupCacheEntryOptions);
                        _logger.LogInformation($""CacheService Keylist to Group, Successfully group: ({cacheGroupKey}) new keyList : ({serializedData})"");
                    }
                }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Caching");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "CacheService.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    #endregion

    #region Utils/DynmaicQuery
    public static void GenerateDynamicQuery(string path)
    {
        string code = @"
            namespace Core.Utils.DynamicQuery;

            public class DynamicQuery
            {
                public IEnumerable<Sort>? Sort { get; set; }
                public Filter? Filter { get; set; }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "DynamicQuery");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "DynamicQuery.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateFilter(string path)
    {
        string code = @"
            namespace Core.Utils.DynamicQuery;

            public class Filter
            {
                public string? Field { get; set; }
                public string? Operator { get; set; }
                public string? Value { get; set; }
                public string? Logic { get; set; }
                public List<Filter>? Filters { get; set; }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "DynamicQuery");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "Filter.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateQueryableDynamicQueryExtension(string path)
    {
        string code = @"
            using System.Linq.Dynamic.Core;
            using System.Text;

            namespace Core.Utils.DynamicQuery;

            public static class QueryableDynamicQueryExtension
            {
                private static readonly string[] _orderDirs = { ""asc"", ""desc"" };
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

                public static IQueryable<T> ToDynamic<T>(this IQueryable<T> query, DynamicQuery dynamicQuery)
                {
                    if (dynamicQuery == null) return query;
                    if (dynamicQuery.Filter is not null) query = Filter(query, dynamicQuery.Filter);
                    if (dynamicQuery.Sort is not null && dynamicQuery.Sort.Any()) query = Sort(query, dynamicQuery.Sort);
                    return query;
                }

                private static IQueryable<T> Filter<T>(IQueryable<T> queryable, Filter filter)
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

                private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
                {
                    if (sort.Any())
                    {
                        foreach (Sort item in sort)
                        {
                            if (string.IsNullOrEmpty(item.Field))
                                throw new ArgumentException(""Empty Field For Sorting Process"");
                            if (string.IsNullOrEmpty(item.Dir) || !_orderDirs.Contains(item.Dir))
                                throw new ArgumentException(""Invalid Order Type For Sorting Process"");
                        }

                        string ordering = string.Join(separator: "","", values: sort.Select(s => $""{s.Field} {s.Dir}""));
                        return queryable.OrderBy(ordering);
                    }

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

        string folderPath = Path.Combine(path, "Core", "Utils", "DynamicQuery");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "QueryableDynamicQueryExtension.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateSort(string path)
    {
        string code = @"
            namespace Core.Utils.DynamicQuery;

            public class Sort
            {
                public string? Field { get; set; }
                public string? Dir { get; set; }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "DynamicQuery");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "Sort.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    #endregion

    #region Utils/Pagination
    public static void GeneratePagination(string path)
    {
        string code = @"
            namespace Core.Utils.Pagination;

            public class BasePageableModel
            {
                public int Index { get; set; }
                public int Size { get; set; }
                public int Count { get; set; }
                public int Pages { get; set; }
                public bool HasPrevious { get; set; }
                public bool HasNext { get; set; }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Pagination");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "BasePageableModel.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GeneratePaginate(string path)
    {
        string code = @"
            namespace Core.Utils.Pagination;

            public class Paginate<TData>
            {
                public Paginate()
                {
                    Items = Array.Empty<TData>();
                }

                public int Index { get; set; }
                public int Size { get; set; }
                public int Count { get; set; }
                public int Pages { get; set; }
                public IList<TData> Items { get; set; }
                public bool HasPrevious => Index > 0;
                public bool HasNext => Index + 1 < Pages;
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Pagination");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "Paginate.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GeneratePagingRequest(string path)
    {
        string code = @"
            namespace Core.Utils.Pagination;

            public class PagingRequest
            {
                public int Page { get; set; } = 0;
                public int PageSize { get; set; } = 20;

                public PagingRequest()
                {
                }

                public PagingRequest(int page, int pageSize)
                {
                    Page = page;
                    PageSize = pageSize;
                }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Pagination");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "PagingRequest.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateQueryablePaginateExtension(string path)
    {
        string code = @"
            using Microsoft.EntityFrameworkCore;

            namespace Core.Utils.Pagination;

            public static class QueryablePaginateExtension
            {
                public static Paginate<TData> ToPaginate<TData>(this IQueryable<TData> data, int index = default, int size = default)
                {
                    int count = data.Count();

                    if (index == default || index < 0) index = 0;
                    if (size == default || size <= 0) size = count;

                    List<TData> items = data.Skip(index * size).Take(size).ToList();
                    Paginate<TData> list = new()
                    {
                        Index = index,
                        Size = size,
                        Count = count,
                        Items = items,
                        Pages = (count <= 0 || size <= 0) ? 0 : (int)Math.Ceiling(count / (double)size)
                    };
                    return list;
                }


                public static async Task<Paginate<TData>> ToPaginateAsync<TData>(this IQueryable<TData> data, int index = default, int size = default, CancellationToken cancellationToken = default)
                {
                    int count = await data.CountAsync(cancellationToken).ConfigureAwait(false);

                    if (index == default || index < 0) index = 0;
                    if (size == default || size <= 0) size = count;

                    List<TData> items = await data.Skip(index * size).Take(size).ToListAsync(cancellationToken).ConfigureAwait(false);
                    Paginate<TData> list = new()
                    {
                        Index = index,
                        Size = size,
                        Count = count,
                        Items = items,
                        Pages = (count <= 0 || size <= 0) ? 0 : (int)Math.Ceiling(count / (double)size)
                    };
                    return list;
                }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils", "Pagination");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "QueryablePaginateExtension.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateFSPModel(string path)
    {
        string code = @"
            using Core.Utils.Pagination;

            namespace Core.Utils;

            public class FSPModel
            {
                public PagingRequest? PagingRequest { get; set; }
                public DynamicQuery.DynamicQuery? DynamicQuery { get; set; }
            }";

        string folderPath = Path.Combine(path, "Core", "Utils");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "FSPModel.cs");

        
        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    #endregion

    #region CrossCuttingConcerns
    public static void GenerateBusinessExceptionHandlerInspector(string path)
    {
        string code = @"
            using Castle.DynamicProxy;
            using Core.Exceptions;
            using System.Reflection;

            namespace Core.CrossCuttingConcerns;
            public class BusinessExceptionHandlerInspector : IInterceptor
            {
                public void Intercept(IInvocation invocation)
                {
                    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                    var attribute = methodInfo.GetCustomAttributes(typeof(BusinessExceptionHandlerAttribute), true).FirstOrDefault();
                    var classAttribute = methodInfo.DeclaringType?.GetCustomAttributes(typeof(BusinessExceptionHandlerAttribute), true).FirstOrDefault();
                    if (attribute == null && classAttribute == null)
                    {
                        invocation.Proceed();
                        return;
                    }
     

                    try
                    {
                        if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                        {
                            invocation.Proceed();
                        }
                        else if (!methodInfo.ReturnType.IsGenericType)
                        {
                            invocation.ReturnValue = InterceptAsync(invocation);
                        }
                        else
                        {
                            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault(); ;
                            if (returnType == null) { invocation.ReturnValue = InterceptAsync(invocation); return; }
                            var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric method not found."");
                            var genericMethod = method.MakeGenericMethod(returnType);
                            if (genericMethod == null) throw new InvalidOperationException(""InterceptAsyncGeneric has not been created."");

                            invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation });
                        }
                    }
                    catch (Exception exception)
                    {
                        if (exception.InnerException != null) throw new BusinessException(exception.Message + exception.InnerException.Message);
                        throw new BusinessException(exception.Message);
                    }
                } 

                private async Task InterceptAsync(IInvocation invocation)
                {
                    try
                    {
                        invocation.Proceed();
                        await (Task)invocation.ReturnValue;
                    }
                    catch (Exception exception)
                    { 
                        if (exception.InnerException != null) throw new BusinessException(exception.Message + exception.InnerException.Message);
                        throw new BusinessException(exception.Message);
                    }
                }
     
                private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation)
                {
                    try
                    {
                        invocation.Proceed();
                        var result = await (Task<TResult>)invocation.ReturnValue;
                        return result;
                    }
                    catch (Exception exception)
                    {
                        if (exception.InnerException != null) throw new BusinessException(exception.Message + exception.InnerException.Message);
                        throw new BusinessException(exception.Message);
                    } 
                }
            }


            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
            public class BusinessExceptionHandlerAttribute : Attribute
            {
            }";

        string folderPath = Path.Combine(path, "Core", "CrossCuttingConcerns");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "BusinessExceptionHandlerInspector.cs");


        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateCacheInterceptor(string path)
    {
        string code = @"
            using Castle.DynamicProxy;
            using Core.Utils.Caching;
            using Newtonsoft.Json;
            using System.Reflection;

            namespace Core.CrossCuttingConcerns;
            public class CacheInterceptor : IInterceptor
            {
                private readonly ICacheService _cacheService;
                public CacheInterceptor(ICacheService cacheService) => _cacheService = cacheService;
    
    
                public void Intercept(IInvocation invocation)
                {
                    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                    var attribute = methodInfo.GetCustomAttributes(typeof(CacheAttribute), true).FirstOrDefault() as CacheAttribute;
                    if (attribute == null || string.IsNullOrEmpty(attribute.BaseCacheKey)) {
                        invocation.Proceed();
                        return;
                    }

                    if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        if(methodInfo.ReturnType == typeof(void))
                        {
                            InterceptVoidSync(invocation, attribute);
                        }
                        else
                        {
                            InterceptSync(invocation, attribute);
                        }
                    }
                    else 
                    {
                        if (!methodInfo.ReturnType.IsGenericType)
                        {
                            invocation.ReturnValue = InterceptAsync(invocation);
                        }
                        else
                        {
                            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault(); ;
                            if (returnType == null) { invocation.ReturnValue = InterceptAsync(invocation); return; }
                            var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric method not found."");
                            var genericMethod = method.MakeGenericMethod(returnType);
                            if (genericMethod == null) throw new InvalidOperationException(""InterceptAsyncGeneric has not been created."");

                            invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation, attribute });
                        }
                    }
                }

                private void InterceptVoidSync(IInvocation invocation, CacheAttribute attribute)
                {
                    // on before...
                    invocation.Proceed();
                    // on success...
                    // this method for void methods, already void methods must not use cache attribute
                }

                private void InterceptSync(IInvocation invocation, CacheAttribute attribute)
                {
                    // on before...
                    var cacheKey = GenerateCacheKey(attribute.BaseCacheKey, invocation.Arguments);
                    var resultCache = _cacheService.GetFromCache(cacheKey);
                    if (resultCache.IsSuccess)
                    {
                        var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                        var source = JsonConvert.DeserializeObject(resultCache.Source!, methodInfo.ReturnType);
                        if (source != null)
                        { 
                            invocation.ReturnValue = source;
                            return;
                        }
                    }

                    invocation.Proceed();
        
                    // on success...
                    if (invocation.ReturnValue != null)
                    {
                        _cacheService.AddToCache(cacheKey, attribute.CacheGroupKeys, invocation.ReturnValue);
                    }
                }

                private async Task InterceptAsync(IInvocation invocation)
                {
                    // on before...
                    invocation.Proceed();
                    await (Task)invocation.ReturnValue;
                    // on success...
                    // this method for void async methods, already void methods must not use cache attribute
                }

                private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, CacheAttribute attribute)
                {
                    // on before...
                    var cacheKey = GenerateCacheKey(attribute.BaseCacheKey, invocation.Arguments);
                    var resultCache = _cacheService.GetFromCache(cacheKey);
                    if (resultCache.IsSuccess)
                    {
                        var source = JsonConvert.DeserializeObject<TResult>(resultCache.Source!); 
                        if (source != null) return source;
                    }

                    invocation.Proceed();
                    var result = await (Task<TResult>)invocation.ReturnValue;

                    // on success...
                    if (result != null)
                    { 
                        _cacheService.AddToCache(cacheKey, attribute.CacheGroupKeys, result);
                    }
                    return result; 
                }
     

                private string GenerateCacheKey(string baseCacheKey, object[] args)
                {
                    if (args.Length > 0) return $""{baseCacheKey}-{string.Join(""-"", args)}"";
                    return $""{baseCacheKey}"";
                }
            }


            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
            public class CacheAttribute : Attribute
            {
                public string BaseCacheKey { get; }
                public string[] CacheGroupKeys { get; }
                public CacheAttribute(string baseCacheKey, string[] cacheGroupKeys)
                {
                    BaseCacheKey = baseCacheKey;
                    CacheGroupKeys = cacheGroupKeys;
                }
            }";

        string folderPath = Path.Combine(path, "Core", "CrossCuttingConcerns");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "CacheInterceptor.cs");


        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateCacheRemoveGroupInterceptor(string path)
    {
        string code = @"
            using Castle.DynamicProxy;
            using Core.Utils.Caching;
            using System.Reflection;

            namespace Core.CrossCuttingConcerns;
            public class CacheRemoveGroupInterceptor : IInterceptor
            {
                private readonly ICacheService _cacheService;
                public CacheRemoveGroupInterceptor(ICacheService cacheService) => _cacheService = cacheService;


                public void Intercept(IInvocation invocation)
                {
                    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                    var attribute = methodInfo.GetCustomAttributes(typeof(CacheRemoveGroupAttribute), true).FirstOrDefault() as CacheRemoveGroupAttribute;
                    if (attribute == null || attribute.CacheGroupKeys.Length == 0)
                    {
                        invocation.Proceed();
                        return;
                    }

                    if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        if (methodInfo.ReturnType == typeof(void))
                        {
                            InterceptVoidSync(invocation, attribute.CacheGroupKeys);
                        }
                        else
                        {
                            InterceptSync(invocation, attribute.CacheGroupKeys);
                        }
                    }
                    else
                    {
                        if (!methodInfo.ReturnType.IsGenericType)
                        {
                            invocation.ReturnValue = InterceptAsync(invocation, attribute.CacheGroupKeys);
                        }
                        else
                        {
                            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault(); ;
                            if (returnType == null) { invocation.ReturnValue = InterceptAsync(invocation, attribute.CacheGroupKeys); return; }
                            var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric method not found."");
                            var genericMethod = method.MakeGenericMethod(returnType);
                            if (genericMethod == null) throw new InvalidOperationException(""InterceptAsyncGeneric has not been created."");

                            invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation, attribute.CacheGroupKeys });
                        }
                    }
                }

                private void InterceptVoidSync(IInvocation invocation, string[] cacheGroupKeys)
                {
                    // on before...
                    invocation.Proceed();
                    // on success...
                    _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
                }

                private void InterceptSync(IInvocation invocation, string[] cacheGroupKeys)
                {
                    // on before...
                    invocation.Proceed();
                    // on success...
                    _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
                }

                private async Task InterceptAsync(IInvocation invocation, string[] cacheGroupKeys)
                {
                    // on before...
                    invocation.Proceed();
                    await (Task)invocation.ReturnValue;
                    // on success...
                    _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
                }

                private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, string[] cacheGroupKeys)
                {
                    // on before...
                    invocation.Proceed();
                    var result = await (Task<TResult>)invocation.ReturnValue;
                    // on success...
                    _cacheService.RemoveCacheGroupKeys(cacheGroupKeys);
                    return result;
                }
            }


            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
            public class CacheRemoveGroupAttribute : Attribute
            {
                public string[] CacheGroupKeys { get; }
                public CacheRemoveGroupAttribute(string[] cacheGroupKeys) => CacheGroupKeys = cacheGroupKeys;
            }";

        string folderPath = Path.Combine(path, "Core", "CrossCuttingConcerns");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "CacheRemoveGroupInterceptor.cs");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateCacheRemoveInterceptor(string path)
    {
        string code = @"
            using Castle.DynamicProxy;
            using Core.Utils.Caching;
            using System.Reflection;

            namespace Core.CrossCuttingConcerns;
            public class CacheRemoveInterceptor : IInterceptor 
            {
                private readonly ICacheService _cacheService;
                public CacheRemoveInterceptor(ICacheService cacheService) => _cacheService = cacheService;


                public void Intercept(IInvocation invocation)
                {
                    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                    var attribute = methodInfo.GetCustomAttributes(typeof(CacheRemoveAttribute), true).FirstOrDefault() as CacheRemoveAttribute;
                    if (attribute == null || string.IsNullOrEmpty(attribute.CacheKey))
                    {
                        invocation.Proceed();
                        return;
                    }

                    if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        if (methodInfo.ReturnType == typeof(void))
                        {
                            InterceptVoidSync(invocation, attribute.CacheKey);
                        }
                        else
                        {
                            InterceptSync(invocation, attribute.CacheKey);
                        }
                    }
                    else
                    {
                        if (!methodInfo.ReturnType.IsGenericType)
                        {
                            invocation.ReturnValue = InterceptAsync(invocation, attribute.CacheKey);
                        }
                        else
                        {
                            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault(); ;
                            if (returnType == null) { invocation.ReturnValue = InterceptAsync(invocation, attribute.CacheKey); return; }
                            var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric method not found."");
                            var genericMethod = method.MakeGenericMethod(returnType);
                            if (genericMethod == null) throw new InvalidOperationException(""InterceptAsyncGeneric has not been created."");

                            invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation, attribute.CacheKey });
                        }
                    }
                }

                private void InterceptVoidSync(IInvocation invocation, string cacheKey)
                {
                    // on before...
                    invocation.Proceed();
                    // on success...
                    _cacheService.RemoveFromCache(cacheKey);
                }

                private void InterceptSync(IInvocation invocation, string cacheKey)
                {
                    // on before...
                    invocation.Proceed();
                    // on success...
                    _cacheService.RemoveFromCache(cacheKey);
                }

                private async Task InterceptAsync(IInvocation invocation, string cacheKey)
                {
                    // on before...
                    invocation.Proceed();
                    await (Task)invocation.ReturnValue;
                    // on success...
                    _cacheService.RemoveFromCache(cacheKey);
                }

                private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, string cacheKey)
                {
                    // on before...
                    invocation.Proceed();
                    var result = await (Task<TResult>)invocation.ReturnValue;
                    // on success...
                    _cacheService.RemoveFromCache(cacheKey);
                    return result;
                }
            }

            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
            public class CacheRemoveAttribute : Attribute
            {
                public string CacheKey { get; }
                public CacheRemoveAttribute(string cacheKey) => CacheKey = cacheKey;
            }";

        string folderPath = Path.Combine(path, "Core", "CrossCuttingConcerns");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "CacheRemoveInterceptor.cs");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateDataAccessExceptionHandlerInspector(string path)
    {
        string code = @"
            using Castle.DynamicProxy;
            using Core.Exceptions;
            using System.Reflection;

            namespace Core.CrossCuttingConcerns;
            public class DataAccessExceptionHandlerInspector : IInterceptor
            {
                public void Intercept(IInvocation invocation)
                {
                    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                    var attribute = methodInfo.GetCustomAttributes(typeof(DataAccessExceptionHandlerAttribute), true).FirstOrDefault();
                    var classAttribute = methodInfo.DeclaringType?.GetCustomAttributes(typeof(DataAccessExceptionHandlerAttribute), true).FirstOrDefault();
                    if (attribute == null && classAttribute == null)
                    {
                        invocation.Proceed();
                        return;
                    }

                    try
                    {
                        if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                        {
                            invocation.Proceed();
                        }
                        else if (!methodInfo.ReturnType.IsGenericType)
                        {
                            invocation.ReturnValue = InterceptAsync(invocation);
                        }
                        else
                        {
                            var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault(); ;
                            if (returnType == null) { invocation.ReturnValue = InterceptAsync(invocation); return; }
                            var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                            if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric method not found."");
                            var genericMethod = method.MakeGenericMethod(returnType);
                            if (genericMethod == null) throw new InvalidOperationException(""InterceptAsyncGeneric has not been created."");

                            invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation });
                        }
                    }
                    catch (Exception exception)
                    {
                        if (exception.InnerException != null) throw new DataAccessException(exception.Message + exception.InnerException.Message);
                        throw new DataAccessException(exception.Message);
                    }
                }

                private async Task InterceptAsync(IInvocation invocation)
                {
                    try
                    {
                        invocation.Proceed();
                        await (Task)invocation.ReturnValue;
                    }
                    catch (Exception exception)
                    {
                        if (exception.InnerException != null) throw new DataAccessException(exception.Message + exception.InnerException.Message);
                        throw new DataAccessException(exception.Message);
                    }
                }

                private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation)
                {
                    try
                    {
                        invocation.Proceed();
                        var result = await (Task<TResult>)invocation.ReturnValue;
                        return result;
                    }
                    catch (Exception exception)
                    {
                        if (exception.InnerException != null) throw new DataAccessException(exception.Message + exception.InnerException.Message);
                        throw new DataAccessException(exception.Message);
                    }
                }
            }


            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
            public class DataAccessExceptionHandlerAttribute : Attribute
            {
            }";

        string folderPath = Path.Combine(path, "Core", "CrossCuttingConcerns");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "DataAccessExceptionHandlerInspector.cs");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    public static void GenerateValidationInterceptor(string path)
    {
        string code = @"
            using Castle.DynamicProxy;
            using Core.Exceptions;
            using FluentValidation;
            using FluentValidation.Results;
            using System.Reflection;

            namespace Core.CrossCuttingConcerns;

            public class ValidationInterceptor : IInterceptor
            {
                private readonly IServiceProvider _serviceProvider;
                public ValidationInterceptor(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;


                public void Intercept(IInvocation invocation)
                {
                    var methodInfo = invocation.MethodInvocationTarget ?? invocation.Method;
                    var attribute = methodInfo.GetCustomAttributes(typeof(ValidationAttribute), true).FirstOrDefault() as ValidationAttribute;
                    if (attribute == null || attribute.TargetType == null )
                    {
                        invocation.Proceed();
                        return;
                    }
 

                    if (!typeof(Task).IsAssignableFrom(methodInfo.ReturnType))
                    {
                        // On before...
                        CheckValidation(invocation, attribute.TargetType);
                        invocation.Proceed();
                        // On success...
                    }
                    else if (!methodInfo.ReturnType.IsGenericType)
                    {
                        invocation.ReturnValue = InterceptAsync(invocation, attribute);
                    }
                    else
                    {
                        var returnType = methodInfo.ReturnType.GetGenericArguments().FirstOrDefault(); ;
                        if (returnType == null) { invocation.ReturnValue = InterceptAsync(invocation, attribute); return; }
                        var method = GetType().GetMethod(nameof(InterceptAsyncGeneric), BindingFlags.NonPublic | BindingFlags.Instance);
                        if (method == null) throw new InvalidOperationException(""InterceptAsyncGeneric method not found."");
                        var genericMethod = method.MakeGenericMethod(returnType);
                        if (genericMethod == null) throw new InvalidOperationException(""InterceptAsyncGeneric has not been created."");

                        invocation.ReturnValue = genericMethod.Invoke(this, new object[] { invocation, attribute });
                    }
                }
 

                private async Task InterceptAsync(IInvocation invocation, ValidationAttribute attribute)
                {
                    // on before...
                    CheckValidation(invocation, attribute.TargetType);

                    invocation.Proceed();
                    await (Task)invocation.ReturnValue;
                    // on success...
                }

                private async Task<TResult> InterceptAsyncGeneric<TResult>(IInvocation invocation, ValidationAttribute attribute)
                {
                    // on before...
                    CheckValidation(invocation, attribute.TargetType);

                    invocation.Proceed();
                    var result = await (Task<TResult>)invocation.ReturnValue;
                    // on success...
                    return result;
                }


                private void CheckValidation(IInvocation invocation, Type targetType)
                {
                    var request = invocation.Arguments.FirstOrDefault(arg => arg?.GetType() == targetType);
                    if (request == null) throw new InvalidOperationException(""Request object to validation could not read."");

                    var validatorsType = typeof(IEnumerable<>).MakeGenericType(typeof(IValidator<>).MakeGenericType(targetType));
                    if (validatorsType == null) throw new InvalidOperationException(""ValidatorsType has not been created."");
                    var validators = (IEnumerable<IValidator>)_serviceProvider.GetService(validatorsType)!;
                    if (validators == null || !validators.Any()) return;


                    var contextType = typeof(ValidationContext<>).MakeGenericType(targetType);
                    if (contextType == null) throw new InvalidOperationException(""contextType has not been created."");

                    var context = (IValidationContext)Activator.CreateInstance(contextType, request)!;
                    if (context == null) throw new InvalidOperationException(""context has not been created."");

                    IEnumerable<ValidationFailure> failures = validators
                        .Select(validator => validator.Validate(context))
                        .Where(result => result.IsValid == false)
                        .SelectMany(result => result.Errors)
                        .ToList();

                    if (failures.Any()) throw new ValidationException(failures);
                }
            }


            [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class | AttributeTargets.Assembly, AllowMultiple = true, Inherited = true)]
            public class ValidationAttribute : Attribute
            {
                public Type TargetType { get; }
                public ValidationAttribute(Type targetType) => TargetType = targetType;
            }";

        string folderPath = Path.Combine(path, "Core", "CrossCuttingConcerns");
        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string filePath = Path.Combine(folderPath, "ValidationInterceptor.cs");

        if (!File.Exists(filePath))
        {
            File.WriteAllText(filePath, code);
        }
    }
    #endregion


    public static void GenerateServiceRegistration()
    {


    }



    public static void AddPackage(string path, string packageName)
    {
        string corePath = Path.Combine(path, "Core");
        string csprojPath = Path.Combine(corePath, "Core.csproj");

        RunCommand(Path.GetDirectoryName(csprojPath)!, "dotnet", $"add \"{csprojPath}\" package {packageName}");
    }


    private static string RunCommand(string workingDirectory, string fileName, string arguments)
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
            process!.WaitForExit();

            if (process.ExitCode != 0)
            {
                string error = process.StandardError.ReadToEnd();
                throw new Exception($"Command failed: {error}");
            }
            else
            {
                string output = process.StandardOutput.ReadToEnd();
                return output;
            }
        }
    }
}
