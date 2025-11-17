using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace API.Helpers;

[ModelBinder(BinderType = typeof(UserParamsModelBinder))]
public class UserParams
{
    private const int MaxPageSize = int.MaxValue;
    public int PageNumber { get; init; } = 1;
    private readonly int _pageSize = MaxPageSize;

    /// <summary>
    /// If set to 0, will set as MaxInt
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        init => _pageSize = (value == 0) ? MaxPageSize : value;
    }

    public static readonly UserParams Default = new()
    {
        PageSize = 20,
        PageNumber = 1,
    };
}

/// <summary>
/// A custom model binder for UserParams which assigns the null value when none of the fields are found. Fields are matched case-insensitive.
/// This is needed so we don't get int.MaxValue as pageSize by default everywhere
/// </summary>
public class UserParamsModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var query = bindingContext.HttpContext.Request.Query;

        var pageNumberKey = query.Keys.FirstOrDefault(k => k.Equals("PageNumber", StringComparison.OrdinalIgnoreCase));
        var pageSizeKey = query.Keys.FirstOrDefault(k => k.Equals("PageSize", StringComparison.OrdinalIgnoreCase));

        if (pageSizeKey == null && pageNumberKey == null)
        {
            bindingContext.Result = ModelBindingResult.Success(null);
            return Task.CompletedTask;
        }

        var pageNumber = 1;
        var pageSize = int.MaxValue;

        if (pageNumberKey != null && int.TryParse(query[pageNumberKey], out var pn))
        {
            pageNumber = pn;
        }

        if (pageSizeKey != null && int.TryParse(query[pageSizeKey], out var ps))
        {
            pageSize = ps;
        }

        var result = new UserParams
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
        };

        bindingContext.Result = ModelBindingResult.Success(result);
        return Task.CompletedTask;
    }
}
