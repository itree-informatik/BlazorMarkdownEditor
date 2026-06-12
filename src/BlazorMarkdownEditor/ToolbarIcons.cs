namespace BlazorMarkdownEditor;

/// <summary>
/// Inline SVG icons (16×16, <c>currentColor</c>) used by the editor toolbar.
/// Kept self-contained so the component needs no external icon font or library.
/// </summary>
internal static class ToolbarIcons
{
    public const string Heading1 = "<span class=\"bme__tool-text\">H1</span>";

    public const string Heading2 = "<span class=\"bme__tool-text\">H2</span>";

    public const string Heading3 = "<span class=\"bme__tool-text\">H3</span>";

    public const string Blockquote =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M1.75 2.5h10.5a.75.75 0 0 1 0 1.5H1.75a.75.75 0 0 1 0-1.5Zm4 5h6.5a.75.75 0 0 1 0 1.5h-6.5a.75.75 0 0 1 0-1.5Zm0 5h6.5a.75.75 0 0 1 0 1.5h-6.5a.75.75 0 0 1 0-1.5ZM2.5 7.75v6a.75.75 0 0 1-1.5 0v-6a.75.75 0 0 1 1.5 0Z\"/></svg>";

    public const string InlineCode =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M11.28 3.22a.75.75 0 0 1 1.06 0l4.25 4.25a.75.75 0 0 1 0 1.06l-4.25 4.25a.75.75 0 0 1-1.06-1.06L14.94 8l-3.66-3.72a.75.75 0 0 1 0-1.06Zm-6.56 0a.75.75 0 0 1 0 1.06L1.06 8l3.66 3.72a.75.75 0 1 1-1.06 1.06L-.59 8.53a.75.75 0 0 1 0-1.06l4.25-4.25a.75.75 0 0 1 1.06 0Z\"/></svg>";

    public const string CodeBlock =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" fill-rule=\"evenodd\" d=\"M1.5 1h13a.5.5 0 0 1 .5.5v13a.5.5 0 0 1-.5.5h-13a.5.5 0 0 1-.5-.5v-13a.5.5 0 0 1 .5-.5Zm1 1.5v11h11v-11Zm7.22 2.97a.75.75 0 0 1 1.06 0l2 2a.75.75 0 0 1 0 1.06l-2 2a.75.75 0 1 1-1.06-1.06L11.19 8 9.72 6.53a.75.75 0 0 1 0-1.06ZM6.28 5.47a.75.75 0 0 1 0 1.06L4.81 8l1.47 1.47a.75.75 0 1 1-1.06 1.06l-2-2a.75.75 0 0 1 0-1.06l2-2a.75.75 0 0 1 1.06 0Z\"/></svg>";

    public const string Bold =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M4 2h5a3 3 0 0 1 2.1 5.1A3.2 3.2 0 0 1 9.5 14H4V2Zm2 2v3h3a1.5 1.5 0 0 0 0-3H6Zm0 5v3h3.5a1.5 1.5 0 0 0 0-3H6Z\"/></svg>";

    public const string Italic =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M6 2h6v2H9.8l-2.4 8H10v2H4v-2h2.2l2.4-8H6V2Z\"/></svg>";

    public const string UnorderedList =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M2 3.5a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3Zm0 6a1.5 1.5 0 1 1 0 3 1.5 1.5 0 0 1 0-3ZM6 4h9v2H6V4Zm0 6h9v2H6v-2Z\"/></svg>";

    public const string OrderedList =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M6 4h9v2H6V4Zm0 6h9v2H6v-2ZM2 2h2v4H3V3H2V2Zm0 7h2v1l-1 1h1v1H1v-1l1-1H1V9h1Z\"/></svg>";

    public const string Link =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" d=\"M7.775 3.275a.75.75 0 0 0 1.06 1.06l1.25-1.25a2 2 0 1 1 2.83 2.83l-2.5 2.5a2 2 0 0 1-2.83 0 .75.75 0 0 0-1.06 1.06 3.5 3.5 0 0 0 4.95 0l2.5-2.5a3.5 3.5 0 0 0-4.95-4.95l-1.25 1.25Zm-4.69 9.64a2 2 0 0 1 0-2.83l2.5-2.5a2 2 0 0 1 2.83 0 .75.75 0 0 0 1.06-1.06 3.5 3.5 0 0 0-4.95 0l-2.5 2.5a3.5 3.5 0 0 0 4.95 4.95l1.25-1.25a.75.75 0 0 0-1.06-1.06l-1.25 1.25a2 2 0 0 1-2.83 0Z\"/></svg>";

    public const string Table =
        "<svg viewBox=\"0 0 16 16\" width=\"16\" height=\"16\" aria-hidden=\"true\"><path fill=\"currentColor\" fill-rule=\"evenodd\" d=\"M1.5 1h13a.5.5 0 0 1 .5.5v13a.5.5 0 0 1-.5.5h-13a.5.5 0 0 1-.5-.5v-13a.5.5 0 0 1 .5-.5ZM2.5 2.5v3H7v-3H2.5Zm6.5 0v3h4.5v-3H9Zm-6.5 4.5v3H7V7H2.5Zm6.5 0v3h4.5V7H9Zm-6.5 4.5v2H7v-2H2.5Zm6.5 0v2h4.5v-2H9Z\"/></svg>";
}
