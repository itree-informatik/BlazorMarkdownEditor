using System.Linq.Expressions;
using Bunit;
using Xunit;

namespace BlazorMarkdownEditor.Tests;

public class MarkdownEditorTests : BunitContext
{
    private string _bound = "value";

    public MarkdownEditorTests()
    {
        // The component imports a JS module on first render; loose mode satisfies that.
        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    [Fact]
    public void RendersWriteAndPreviewTabs_WithEnglishDefaults()
    {
        var cut = Render<MarkdownEditor>();

        var tabs = cut.FindAll(".bme__tab");
        Assert.Equal(2, tabs.Count);
        Assert.Equal("Write", tabs[0].TextContent.Trim());
        Assert.Equal("Preview", tabs[1].TextContent.Trim());
    }

    [Fact]
    public void WriteTab_ShowsToolbarAndTextarea()
    {
        var cut = Render<MarkdownEditor>();

        Assert.NotEmpty(cut.FindAll(".bme__tool"));
        Assert.NotNull(cut.Find("textarea.bme__textarea"));
    }

    [Fact]
    public void PreviewTab_RendersMarkdownAsHtml()
    {
        var cut = Render<MarkdownEditor>(ps => ps.Add(p => p.Value, "# Hello"));

        cut.FindAll(".bme__tab")[1].Click();

        var preview = cut.Find(".bme__preview");
        Assert.Contains("<h1>Hello</h1>", preview.InnerHtml);
    }

    [Fact]
    public void PreviewTab_WithEmptyValue_ShowsPlaceholderMessage()
    {
        var cut = Render<MarkdownEditor>(ps => ps
            .Add(p => p.Labels, new MarkdownEditorLabels { NothingToPreview = "Nichts" }));

        cut.FindAll(".bme__tab")[1].Click();

        Assert.Equal("Nichts", cut.Find(".bme__preview-empty").TextContent.Trim());
    }

    [Fact]
    public void OutsideEditForm_WithValueExpression_DoesNotThrow()
    {
        // @bind-Value sets ValueExpression automatically; ValidationMessage must not
        // be rendered without an ambient EditContext, otherwise it throws.
        Expression<Func<string>> expression = () => _bound;

        var cut = Render<MarkdownEditor>(ps => ps
            .Add(p => p.Value, _bound)
            .Add(p => p.ValueExpression, expression));

        Assert.NotNull(cut.Find(".bme"));
    }

    [Fact]
    public void Readonly_RendersOnlyPreview_NoTabsOrToolbar()
    {
        var cut = Render<MarkdownEditor>(ps => ps
            .Add(p => p.Value, "# Hello")
            .Add(p => p.Readonly, true));

        Assert.Empty(cut.FindAll(".bme__tabs"));
        Assert.Empty(cut.FindAll(".bme__toolbar"));
        Assert.Empty(cut.FindAll("textarea"));
        Assert.Contains("<h1>Hello</h1>", cut.Find(".bme__preview").InnerHtml);
    }

    [Fact]
    public void Readonly_WithEmptyValue_ShowsNoPlaceholder()
    {
        var cut = Render<MarkdownEditor>(ps => ps
            .Add(p => p.Value, string.Empty)
            .Add(p => p.Readonly, true));

        Assert.Empty(cut.FindAll(".bme__preview-empty"));
    }

    [Fact]
    public void Labels_AreOverridable()
    {
        var cut = Render<MarkdownEditor>(ps => ps
            .Add(p => p.Labels, new MarkdownEditorLabels { WriteTab = "Schreiben", PreviewTab = "Vorschau" }));

        var tabs = cut.FindAll(".bme__tab");
        Assert.Equal("Schreiben", tabs[0].TextContent.Trim());
        Assert.Equal("Vorschau", tabs[1].TextContent.Trim());
    }
}
