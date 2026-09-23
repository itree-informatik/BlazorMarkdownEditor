using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BlazorMarkdownEditor;

/// <summary>
/// A lightweight, dependency-free Markdown editor with "Write" and "Preview" tabs
/// and a formatting toolbar (headings, bold, italic, lists, links, tables) —
/// similar to the GitHub issue editor. The bound <see cref="Value"/> is the raw
/// Markdown source.
/// </summary>
public partial class MarkdownEditor : ComponentBase, IAsyncDisposable
{
    private const string ModulePath = "./_content/BlazorMarkdownEditor/markdown-editor.js";

    private const string TableTemplate =
        "| Column 1 | Column 2 |\n| --- | --- |\n| Cell | Cell |\n";

    private readonly string _textAreaId = $"bme-{Guid.NewGuid():N}";
    private readonly string _previewId = $"bme-{Guid.NewGuid():N}";

    private IJSObjectReference? _module;
    private IReadOnlyList<ToolDefinition> _tools = [];
    private Tab _activeTab = Tab.Write;
    private (int Start, int End)? _pendingSelection;

    [Inject] private IJSRuntime JS { get; set; } = default!;

    // Present only when the component is used inside an EditForm. ValidationMessage
    // requires it, so the validation output is only rendered when it exists.
    [CascadingParameter] private EditContext? CascadedEditContext { get; set; }

    /// <summary>The Markdown source text. Use with <c>@bind-Value</c>.</summary>
    [Parameter] public string Value { get; set; } = string.Empty;

    /// <summary>Raised when the Markdown source changes.</summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>
    /// Identifies the bound value for validation. Set automatically when using
    /// <c>@bind-Value</c> inside an <c>EditForm</c>.
    /// </summary>
    [Parameter] public Expression<Func<string>>? ValueExpression { get; set; }

    /// <summary>User-facing texts. Override to localize the component.</summary>
    [Parameter] public MarkdownEditorLabels Labels { get; set; } = new();

    /// <summary>Placeholder shown in the empty textarea.</summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>Number of visible text rows in the write area.</summary>
    [Parameter] public int Rows { get; set; } = 10;

    /// <summary>
    /// When <see langword="true"/>, the editor renders only the formatted preview of
    /// <see cref="Value"/> — no tabs, toolbar or textarea.
    /// </summary>
    [Parameter] public bool Readonly { get; set; }

    /// <summary>Additional CSS class(es) applied to the component root element.</summary>
    [Parameter] public string? Class { get; set; }

    private MarkdownEditorLabels? _toolLabels;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        // Rebuild the toolbar only when the labels actually change — not on every
        // re-render (e.g. each keystroke updating Value).
        if (ReferenceEquals(_toolLabels, Labels))
            return;

        _toolLabels = Labels;
        _tools =
        [
            new ToolDefinition(Labels.Heading1Tooltip, ToolbarIcons.Heading1, "# ", string.Empty, Labels.HeadingPlaceholder, ToolMode.Line),
            new ToolDefinition(Labels.Heading2Tooltip, ToolbarIcons.Heading2, "## ", string.Empty, Labels.HeadingPlaceholder, ToolMode.Line),
            new ToolDefinition(Labels.Heading3Tooltip, ToolbarIcons.Heading3, "### ", string.Empty, Labels.HeadingPlaceholder, ToolMode.Line),
            new ToolDefinition(Labels.BoldTooltip, ToolbarIcons.Bold, "**", "**", Labels.BoldPlaceholder, ToolMode.Inline),
            new ToolDefinition(Labels.ItalicTooltip, ToolbarIcons.Italic, "*", "*", Labels.ItalicPlaceholder, ToolMode.Inline),
            new ToolDefinition(Labels.BlockquoteTooltip, ToolbarIcons.Blockquote, "> ", string.Empty, Labels.QuotePlaceholder, ToolMode.Line),
            new ToolDefinition(Labels.InlineCodeTooltip, ToolbarIcons.InlineCode, "`", "`", Labels.CodePlaceholder, ToolMode.Inline),
            new ToolDefinition(Labels.CodeBlockTooltip, ToolbarIcons.CodeBlock, "```\n", "\n```", Labels.CodePlaceholder, ToolMode.Inline),
            new ToolDefinition(Labels.UnorderedListTooltip, ToolbarIcons.UnorderedList, "- ", string.Empty, Labels.ListItemPlaceholder, ToolMode.Line),
            new ToolDefinition(Labels.OrderedListTooltip, ToolbarIcons.OrderedList, "1. ", string.Empty, Labels.ListItemPlaceholder, ToolMode.Line),
            new ToolDefinition(Labels.LinkTooltip, ToolbarIcons.Link, "[", "](url)", Labels.LinkTextPlaceholder, ToolMode.Inline),
            new ToolDefinition(Labels.TableTooltip, ToolbarIcons.Table, TableTemplate, string.Empty, string.Empty, ToolMode.Insert),
        ];
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
            _module = await JS.InvokeAsync<IJSObjectReference>("import", ModulePath);

        if (_module is null)
            return;

        if (_pendingSelection is { } selection)
        {
            // The re-render rewrote the textarea value and dropped the caret;
            // restore the selection the toolbar action set.
            _pendingSelection = null;
            await _module.InvokeVoidAsync("restoreSelection", _textAreaId, selection.Start, selection.End);
        }

        // Syntax-highlight code blocks whenever the preview is on screen.
        if (Readonly || _activeTab == Tab.Preview)
            await _module.InvokeVoidAsync("highlight", _previewId);
    }

    private async Task OnInputAsync(string value)
    {
        Value = value ?? string.Empty;
        await ValueChanged.InvokeAsync(Value);
    }

    private void SwitchToWrite() => _activeTab = Tab.Write;

    private void SwitchToPreview() => _activeTab = Tab.Preview;

    private async Task ApplyToolAsync(ToolDefinition tool)
    {
        if (_module is null)
            return;

        var result = await _module.InvokeAsync<ToolResult?>(
            "applyTool", _textAreaId, tool.Before, tool.After, tool.Placeholder, tool.Mode.ToString().ToLowerInvariant());

        if (result is not null)
        {
            Value = result.Value;
            _pendingSelection = (result.Start, result.End);
            await ValueChanged.InvokeAsync(Value);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // The circuit is already gone (e.g. page closed) — nothing to dispose.
            }
        }

        GC.SuppressFinalize(this);
    }

    private enum Tab
    {
        Write,
        Preview,
    }

    private enum ToolMode
    {
        /// <summary>Wrap the selection with <c>Before</c>/<c>After</c>.</summary>
        Inline,

        /// <summary>Prepend <c>Before</c> to every selected line.</summary>
        Line,

        /// <summary>Insert <c>Before</c> at the caret, replacing any selection.</summary>
        Insert,
    }

    private sealed record ToolDefinition(
        string Tooltip,
        string Icon,
        string Before,
        string After,
        string Placeholder,
        ToolMode Mode);

    private sealed record ToolResult(string Value, int Start, int End);
}
