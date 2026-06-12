namespace BlazorMarkdownEditor;

/// <summary>
/// User-facing texts for the <see cref="MarkdownEditor"/> component.
/// All values default to English and can be overridden — for example by wiring
/// them up to the consuming application's translation/localization system.
/// </summary>
public sealed class MarkdownEditorLabels
{
    /// <summary>Caption of the "write" tab where the Markdown source is edited.</summary>
    public string WriteTab { get; set; } = "Write";

    /// <summary>Caption of the "preview" tab that renders the Markdown as HTML.</summary>
    public string PreviewTab { get; set; } = "Preview";

    /// <summary>Tooltip of the heading-1 toolbar button.</summary>
    public string Heading1Tooltip { get; set; } = "Heading 1";

    /// <summary>Tooltip of the heading-2 toolbar button.</summary>
    public string Heading2Tooltip { get; set; } = "Heading 2";

    /// <summary>Tooltip of the heading-3 toolbar button.</summary>
    public string Heading3Tooltip { get; set; } = "Heading 3";

    /// <summary>Tooltip of the bold toolbar button.</summary>
    public string BoldTooltip { get; set; } = "Bold";

    /// <summary>Tooltip of the italic toolbar button.</summary>
    public string ItalicTooltip { get; set; } = "Italic";

    /// <summary>Tooltip of the blockquote toolbar button.</summary>
    public string BlockquoteTooltip { get; set; } = "Quote";

    /// <summary>Tooltip of the inline-code toolbar button.</summary>
    public string InlineCodeTooltip { get; set; } = "Inline code";

    /// <summary>Tooltip of the fenced-code-block toolbar button.</summary>
    public string CodeBlockTooltip { get; set; } = "Code block";

    /// <summary>Tooltip of the bulleted (unordered) list toolbar button.</summary>
    public string UnorderedListTooltip { get; set; } = "Bulleted list";

    /// <summary>Tooltip of the numbered (ordered) list toolbar button.</summary>
    public string OrderedListTooltip { get; set; } = "Numbered list";

    /// <summary>Tooltip of the link toolbar button.</summary>
    public string LinkTooltip { get; set; } = "Link";

    /// <summary>Tooltip of the table toolbar button.</summary>
    public string TableTooltip { get; set; } = "Table";

    /// <summary>Message shown on the preview tab when there is nothing to render.</summary>
    public string NothingToPreview { get; set; } = "Nothing to preview";

    /// <summary>Placeholder text inserted by the heading button when no text is selected.</summary>
    public string HeadingPlaceholder { get; set; } = "Heading";

    /// <summary>Placeholder text inserted by the bold button when no text is selected.</summary>
    public string BoldPlaceholder { get; set; } = "bold text";

    /// <summary>Placeholder text inserted by the italic button when no text is selected.</summary>
    public string ItalicPlaceholder { get; set; } = "italic text";

    /// <summary>Placeholder text inserted by the list buttons when no text is selected.</summary>
    public string ListItemPlaceholder { get; set; } = "List item";

    /// <summary>Placeholder link text inserted by the link button when no text is selected.</summary>
    public string LinkTextPlaceholder { get; set; } = "link text";

    /// <summary>Placeholder text inserted by the blockquote button when no text is selected.</summary>
    public string QuotePlaceholder { get; set; } = "Quote";

    /// <summary>Placeholder text inserted by the code buttons when no text is selected.</summary>
    public string CodePlaceholder { get; set; } = "code";
}
