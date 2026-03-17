using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Fancy.ResourceLinker.Models;

namespace Fancy.ResourceLinker.Hateoas;

/// <summary>
/// A result type to use if you want to add hypermedia links to the resource(s) before returning them.
/// </summary>
/// <typeparam name="TResource"></typeparam>
public class HypermediaResult<TResource> : IActionResult where TResource : IResource
{
    private TResource? _resource { get; }
    private IEnumerable<TResource>? _resources { get; }

    public HypermediaResult(TResource resource)
    {
        _resource = resource;
    }

    public HypermediaResult(IEnumerable<TResource> resources)
    {
        _resources = resources;
    }

    public async Task ExecuteResultAsync(ActionContext context)
    {
        IResourceLinker resourceLinker = context.HttpContext.RequestServices.GetRequiredService<IResourceLinker>();
        IUrlHelperFactory urlHelperFactory = context.HttpContext.RequestServices.GetRequiredService<IUrlHelperFactory>();
        IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(context);

        if(_resource != null)
        {
            resourceLinker.AddLinks(_resource, urlHelper);
            ObjectResult result = new ObjectResult(_resource);
            await result.ExecuteResultAsync(context);
        } 
        else if(_resources != null)
        {
            resourceLinker.AddLinks(_resources, urlHelper);
            ObjectResult result = new ObjectResult(_resources);
            await result.ExecuteResultAsync(context);
        } 
        else
        {
            NoContentResult result = new NoContentResult();
            await result.ExecuteResultAsync(context);
        }
    }
}
