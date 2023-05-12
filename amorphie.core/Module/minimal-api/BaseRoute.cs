namespace amorphie.core.Module.minimal_api
{
    public abstract class BaseRoute : IBaseRoute
    {
        public BaseRoute(WebApplication app)
        {
            RouteGroupBuilder routeGroupBuilder = app.MapGroup($"/{UrlFragment}").WithTags(UrlFragment != null ? UrlFragment : "Default");
            AddRoutes(routeGroupBuilder);
        }
        public abstract string? UrlFragment { get; }
        public abstract void AddRoutes(RouteGroupBuilder routeGroupBuilder);
    }
}

