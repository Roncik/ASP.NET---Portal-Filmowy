using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace PortalFilmowy.Helpers
{
    [HtmlTargetElement("star-rating")]
    public class StarRatingTagHelper : TagHelper
    {
        public double Value { get; set; } // Odbiera wartość oceny (1-10)

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "span";
            output.Attributes.SetAttribute("class", "star-rating-container");
            output.Attributes.SetAttribute("style", "font-size: 1.5rem; line-height: 1; display: inline-block;");

            var sb = new StringBuilder();
            int fullStars = (int)Math.Floor(Value);
            bool hasHalfStar = Value - fullStars > 0;
            double fillPercentage = (Value - fullStars) * 100;

            // 1. Pełne gwiazdki
            for (int i = 0; i < fullStars; i++)
            {
                sb.Append("<span style='color: #ffc107;'>★</span>");
            }

            // 2. Gwiazdka niepełna
            if (hasHalfStar)
            {
                // Trik CSS - na pustą gwiazdkę "nakładamy" uciętą pełną gwiazdkę aby otrzymać niepełną gwiazdkę
                sb.Append("<span style='position: relative; display: inline-block; color: #ccc;'>");
                sb.Append("☆"); // Tło (pusta gwiazdka)
                sb.Append("<span style='position: absolute; left: 0; top: 0; width: " + (int)Math.Round(fillPercentage) + "%; overflow: hidden; color: #ffc107;'>★</span>");
                sb.Append("</span>");
            }

            // 3. Puste gwiazdki (uzupełnienie do 10)
            int currentStars = fullStars + (hasHalfStar ? 1 : 0);
            for (int i = currentStars; i < 10; i++)
            {
                sb.Append("<span style='color: #ccc;'>☆</span>");
            }

            output.Content.SetHtmlContent(sb.ToString());
        }
    }
}