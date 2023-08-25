using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace amorphie.core.Extension;

public static class HttpContextExtensions
{
    public static async Task<TModel> Deserialize<TModel>(this HttpContext httpContext)
    {
        TModel model = default;

        using (MemoryStream str = new MemoryStream())
        {
            await httpContext.Request.Body.CopyToAsync(str);
            var jsonString = System.Text.Encoding.UTF8.GetString(str.ToArray());
            model = System.Text.Json.JsonSerializer.Deserialize<TModel>(jsonString);
        }
        return model;
    }
}
