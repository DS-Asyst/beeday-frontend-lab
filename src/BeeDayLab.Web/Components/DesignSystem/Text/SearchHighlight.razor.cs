using Microsoft.AspNetCore.Components;

namespace BeeDayLab.Web.Components.DesignSystem.Text;

public partial class SearchHighlight
{
    [Parameter] public string Text { get; set; } = string.Empty;
    [Parameter] public string SearchTerm { get; set; } = string.Empty;

    private IReadOnlyList<TextSegment> Segments => BuildSegments(Text, SearchTerm);

    internal static IReadOnlyList<TextSegment> BuildSegments(string text, string searchTerm)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var term = searchTerm.Trim();
        var segments = new List<TextSegment>();
        var currentIndex = 0;

        while (currentIndex < text.Length)
        {
            var matchIndex = text.IndexOf(term, currentIndex, StringComparison.OrdinalIgnoreCase);
            if (matchIndex < 0)
            {
                segments.Add(new TextSegment(text[currentIndex..], false));
                break;
            }

            if (matchIndex > currentIndex)
            {
                segments.Add(new TextSegment(text[currentIndex..matchIndex], false));
            }

            segments.Add(new TextSegment(text.Substring(matchIndex, term.Length), true));
            currentIndex = matchIndex + term.Length;
        }

        return segments;
    }

    internal sealed record TextSegment(string Value, bool IsMatch);
}
