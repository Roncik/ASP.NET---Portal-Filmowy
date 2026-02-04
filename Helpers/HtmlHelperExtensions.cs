using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PortalFilmowy.Helpers
{
    public static class HtmlHelperExtensions
    {
        public static IHtmlContent MovieStatusBadge(this IHtmlHelper htmlHelper, DateTime releaseDate)
        {
            string colorClass = releaseDate.Year == DateTime.Now.Year ? "bg-success" : "bg-secondary";
            string text = releaseDate.Year == DateTime.Now.Year ? "NOWOŚĆ" : "ARCHIWUM";

            return new HtmlString($"<span class='badge {colorClass}'>{text}</span>");
        }
    }
}