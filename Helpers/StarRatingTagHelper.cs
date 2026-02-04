using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace PortalFilmowy.Helpers
{
    [HtmlTargetElement("star-rating")]
    public class StarRatingTagHelper : TagHelper
    {
        public int Value { get; set; } // Odbiera wartość oceny (1-10)

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", "text-warning");

            var sb = new StringBuilder();
            for (int i = 0; i < Value; i++) sb.Append("★");
            for (int i = Value; i < 10; i++) sb.Append("☆");

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}