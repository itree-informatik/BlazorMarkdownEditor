using System.Text;
using System.Text.RegularExpressions;

namespace BlazorMarkdownEditor;

/// <summary>
/// Converts a small, GitHub-flavoured subset of Markdown to HTML — headings,
/// bold, italic, bulleted/numbered lists, links and pipe tables.
/// </summary>
/// <remarks>
/// The input is HTML-encoded <em>before</em> any Markdown is interpreted, so raw
/// HTML (and script) embedded in the source is rendered as plain text rather than
/// executed. The converter has no third-party dependencies.
/// </remarks>
public static partial class MarkdownConverter
{
    /// <summary>
    /// Converts the given Markdown <paramref name="markdown"/> to an HTML fragment.
    /// Returns an empty string for <see langword="null"/> or whitespace input.
    /// </summary>
    public static string ToHtml(string? markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        // Encode first so embedded HTML cannot break out — Markdown syntax characters
        // (#, *, [, |, ...) are left untouched by the encoder.
        var encoded = Encode(markdown.Replace("\r\n", "\n").Replace('\r', '\n'));
        var lines = encoded.Split('\n');

        var html = new StringBuilder();
        var i = 0;
        while (i < lines.Length)
        {
            var line = lines[i];

            // Blank line — nothing to emit.
            if (line.Trim().Length == 0)
            {
                i++;
                continue;
            }

            // Fenced code block: ``` ... ``` (content kept verbatim).
            if (IsFence(line))
            {
                i = AppendFencedCode(html, lines, i);
                continue;
            }

            // Heading: 1-6 leading '#' followed by a space.
            var heading = HeadingRegex().Match(line);
            if (heading.Success)
            {
                var level = heading.Groups[1].Value.Length;
                html.Append("<h").Append(level).Append('>')
                    .Append(Inline(heading.Groups[2].Value))
                    .Append("</h").Append(level).Append(">\n");
                i++;
                continue;
            }

            // Horizontal rule: 3+ of '-', '*' or '_' (checked before lists so that
            // "* * *" / "- - -" are not mistaken for list items).
            if (HorizontalRuleRegex().IsMatch(line))
            {
                html.Append("<hr />\n");
                i++;
                continue;
            }

            // Blockquote: consecutive lines starting with '>'.
            if (BlockquoteRegex().IsMatch(line))
            {
                i = AppendBlockquote(html, lines, i);
                continue;
            }

            // Table: a pipe row immediately followed by a separator row.
            if (IsTableRow(line) && i + 1 < lines.Length && IsTableSeparator(lines[i + 1]))
            {
                i = AppendTable(html, lines, i);
                continue;
            }

            // Unordered list.
            if (UnorderedItemRegex().IsMatch(line))
            {
                i = AppendList(html, lines, i, ordered: false);
                continue;
            }

            // Ordered list.
            if (OrderedItemRegex().IsMatch(line))
            {
                i = AppendList(html, lines, i, ordered: true);
                continue;
            }

            // Paragraph: consecutive plain lines, joined with hard line breaks.
            i = AppendParagraph(html, lines, i);
        }

        return html.ToString().TrimEnd('\n');
    }

    private static int AppendParagraph(StringBuilder html, string[] lines, int i)
    {
        var content = new StringBuilder();
        while (i < lines.Length)
        {
            var line = lines[i];
            if (line.Trim().Length == 0
                || IsFence(line)
                || HeadingRegex().IsMatch(line)
                || HorizontalRuleRegex().IsMatch(line)
                || BlockquoteRegex().IsMatch(line)
                || UnorderedItemRegex().IsMatch(line)
                || OrderedItemRegex().IsMatch(line)
                || (IsTableRow(line) && i + 1 < lines.Length && IsTableSeparator(lines[i + 1])))
            {
                break;
            }

            if (content.Length > 0)
                content.Append("<br />\n");
            content.Append(Inline(line.Trim()));
            i++;
        }

        html.Append("<p>").Append(content).Append("</p>\n");
        return i;
    }

    private static int AppendList(StringBuilder html, string[] lines, int i, bool ordered)
    {
        var tag = ordered ? "ol" : "ul";
        var itemRegex = ordered ? OrderedItemRegex() : UnorderedItemRegex();

        html.Append('<').Append(tag).Append(">\n");
        while (i < lines.Length)
        {
            var match = itemRegex.Match(lines[i]);
            if (!match.Success)
                break;

            html.Append("<li>").Append(Inline(match.Groups[1].Value.Trim())).Append("</li>\n");
            i++;
        }

        html.Append("</").Append(tag).Append(">\n");
        return i;
    }

    private static int AppendFencedCode(StringBuilder html, string[] lines, int i)
    {
        var language = ExtractFenceLanguage(lines[i]);
        i++; // skip the opening fence (and its optional language identifier)

        var code = new StringBuilder();
        while (i < lines.Length && !IsFence(lines[i]))
        {
            if (code.Length > 0)
                code.Append('\n');
            code.Append(lines[i]); // already HTML-encoded; kept verbatim
            i++;
        }

        if (i < lines.Length)
            i++; // skip the closing fence

        // Emit a "language-xxx" class so a highlighter (e.g. highlight.js) can colour it.
        html.Append("<pre><code");
        if (language.Length > 0)
            html.Append(" class=\"language-").Append(language).Append('"');
        html.Append('>').Append(code).Append("</code></pre>\n");
        return i;
    }

    /// <summary>Reads and sanitizes the language identifier from a fence line (e.g. ```sql).</summary>
    private static string ExtractFenceLanguage(string fenceLine)
    {
        var info = fenceLine.TrimStart().TrimStart('`').Trim();

        var space = info.IndexOf(' ');
        if (space >= 0)
            info = info[..space];

        return new string(info.Where(c => char.IsLetterOrDigit(c) || c is '#' or '+' or '-').ToArray());
    }

    private static int AppendBlockquote(StringBuilder html, string[] lines, int i)
    {
        var content = new StringBuilder();
        while (i < lines.Length)
        {
            var match = BlockquoteRegex().Match(lines[i]);
            if (!match.Success)
                break;

            if (content.Length > 0)
                content.Append("<br />\n");
            content.Append(Inline(match.Groups[1].Value.Trim()));
            i++;
        }

        html.Append("<blockquote>").Append(content).Append("</blockquote>\n");
        return i;
    }

    private static int AppendTable(StringBuilder html, string[] lines, int i)
    {
        var headers = SplitRow(lines[i]);
        i += 2; // skip header + separator

        html.Append("<table>\n<thead>\n<tr>");
        foreach (var header in headers)
            html.Append("<th>").Append(Inline(header)).Append("</th>");
        html.Append("</tr>\n</thead>\n<tbody>\n");

        while (i < lines.Length && IsTableRow(lines[i]))
        {
            var cells = SplitRow(lines[i]);
            html.Append("<tr>");
            for (var c = 0; c < headers.Count; c++)
                html.Append("<td>").Append(Inline(c < cells.Count ? cells[c] : string.Empty)).Append("</td>");
            html.Append("</tr>\n");
            i++;
        }

        html.Append("</tbody>\n</table>\n");
        return i;
    }

    /// <summary>Applies inline formatting (code, links, bold, italic) to already-encoded text.</summary>
    private static string Inline(string text)
    {
        // Protect inline code spans (`code`) with sentinels so their contents are not
        // treated as links/bold/italic. The sentinel delimiter cannot appear in the
        // input because Encode() strips it.
        var codeSpans = new List<string>();
        text = InlineCodeRegex().Replace(text, m =>
        {
            codeSpans.Add(m.Groups[1].Value);
            return $"{Sentinel}{codeSpans.Count - 1}{Sentinel}";
        });

        // Links: [label](url) — only when the URL scheme is safe, otherwise left as text.
        text = LinkRegex().Replace(text, m =>
        {
            var label = m.Groups[1].Value;
            var url = m.Groups[2].Value;
            return IsSafeUrl(url)
                ? $"<a href=\"{url}\" target=\"_blank\" rel=\"noopener noreferrer\">{label}</a>"
                : m.Value;
        });

        // Bold before italic so '**' is not consumed by the single-'*' rule.
        text = BoldRegex().Replace(text, "<strong>$1</strong>");
        text = ItalicRegex().Replace(text, "<em>$1</em>");

        for (var j = 0; j < codeSpans.Count; j++)
            text = text.Replace($"{Sentinel}{j}{Sentinel}", $"<code>{codeSpans[j]}</code>");

        return text;
    }

    private static bool IsSafeUrl(string url)
    {
        var u = url.Trim();
        if (u.Length == 0)
            return false;

        // Undo percent-encoding until the value is stable, so multiply-encoded
        // colons (%3A, %253A, %25253A, …) cannot smuggle a scheme past the check.
        string previous;
        do
        {
            previous = u;
            u = Uri.UnescapeDataString(u);
        } while (u != previous);

        if (u.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || u.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || u.StartsWith("mailto:", StringComparison.OrdinalIgnoreCase)
            || u.StartsWith('/')
            || u.StartsWith('#'))
        {
            return true;
        }

        // Otherwise only allow scheme-less relative URLs (javascript:, data:, …
        // all require a colon, which after full decoding can no longer be hidden).
        return !u.Contains(':');
    }

    private static bool IsFence(string line) => line.TrimStart().StartsWith("```");

    private static bool IsTableRow(string line) => line.TrimStart().StartsWith('|');

    private static bool IsTableSeparator(string line)
    {
        var cells = SplitRow(line);
        return cells.Count > 0 && cells.All(c => TableSeparatorCellRegex().IsMatch(c));
    }

    private static List<string> SplitRow(string line)
    {
        var trimmed = line.Trim();
        if (trimmed.StartsWith('|'))
            trimmed = trimmed[1..];
        if (trimmed.EndsWith('|'))
            trimmed = trimmed[..^1];

        return trimmed.Split('|').Select(c => c.Trim()).ToList();
    }

    // Private-use character used to mark protected code spans during inline
    // processing; stripped from the input by Encode() so it can never collide.
    private const char Sentinel = '\uE000';

    private static string Encode(string text) => text
        .Replace(Sentinel.ToString(), string.Empty)
        .Replace("&", "&amp;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;")
        .Replace("\"", "&quot;");

    [GeneratedRegex(@"^(#{1,6})\s+(.*)$")]
    private static partial Regex HeadingRegex();

    [GeneratedRegex(@"^\s*[-*]\s+(.*)$")]
    private static partial Regex UnorderedItemRegex();

    [GeneratedRegex(@"^\s*\d+\.\s+(.*)$")]
    private static partial Regex OrderedItemRegex();

    // '>' has already been HTML-encoded to "&gt;" before block parsing runs.
    [GeneratedRegex(@"^\s*&gt;\s?(.*)$")]
    private static partial Regex BlockquoteRegex();

    // Thematic break: 3+ of the same marker, optionally separated by spaces/tabs.
    [GeneratedRegex(@"^\s*([-*_])([ \t]*\1){2,}[ \t]*$")]
    private static partial Regex HorizontalRuleRegex();

    [GeneratedRegex(@"`([^`]+)`")]
    private static partial Regex InlineCodeRegex();

    [GeneratedRegex(@"\[([^\]]+)\]\(([^)\s]+)\)")]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"\*\*([^*]+)\*\*")]
    private static partial Regex BoldRegex();

    [GeneratedRegex(@"\*([^*]+)\*")]
    private static partial Regex ItalicRegex();

    [GeneratedRegex(@"^:?-+:?$")]
    private static partial Regex TableSeparatorCellRegex();
}
