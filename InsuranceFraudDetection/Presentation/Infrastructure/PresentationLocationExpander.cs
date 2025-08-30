using Microsoft.AspNetCore.Mvc.Razor;

namespace InsuranceFraudDetection.Presentation.Infrastructure
{
    public class PresentationLocationExpander : IViewLocationExpander
    {
        public void PopulateValues(ViewLocationExpanderContext context)
        {
        }

        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            var cleanArchitectureViewLocations = new[]
            {
                "/Presentation/Views/{1}/{0}.cshtml",
                "/Presentation/Views/Shared/{0}.cshtml"
            };

            return cleanArchitectureViewLocations.Concat(viewLocations);
        }
    }
}
