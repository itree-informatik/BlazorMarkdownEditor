using Xunit;

namespace BlazorMarkdownEditor.Tests;

public class MarkdownConverterTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ToHtml_EmptyInput_ReturnsEmpty(string? input)
    {
        Assert.Equal(string.Empty, MarkdownConverter.ToHtml(input));
    }

    [Theory]
    [InlineData("# Title", "<h1>Title</h1>")]
    [InlineData("### Sub", "<h3>Sub</h3>")]
    [InlineData("###### Deep", "<h6>Deep</h6>")]
    public void ToHtml_Headings(string input, string expected)
    {
        Assert.Equal(expected, MarkdownConverter.ToHtml(input));
    }

    [Fact]
    public void ToHtml_Bold()
    {
        Assert.Equal("<p><strong>bold</strong></p>", MarkdownConverter.ToHtml("**bold**"));
    }

    [Fact]
    public void ToHtml_Italic()
    {
        Assert.Equal("<p><em>italic</em></p>", MarkdownConverter.ToHtml("*italic*"));
    }

    [Fact]
    public void ToHtml_UnorderedList()
    {
        var html = MarkdownConverter.ToHtml("- one\n- two");

        Assert.Contains("<ul>", html);
        Assert.Contains("<li>one</li>", html);
        Assert.Contains("<li>two</li>", html);
    }

    [Fact]
    public void ToHtml_OrderedList()
    {
        var html = MarkdownConverter.ToHtml("1. one\n2. two");

        Assert.Contains("<ol>", html);
        Assert.Contains("<li>one</li>", html);
        Assert.Contains("<li>two</li>", html);
    }

    [Theory]
    [InlineData("# H1", "<h1>H1</h1>")]
    [InlineData("## H2", "<h2>H2</h2>")]
    public void ToHtml_HeadingLevels(string input, string expected)
    {
        Assert.Equal(expected, MarkdownConverter.ToHtml(input));
    }

    [Fact]
    public void ToHtml_Blockquote()
    {
        Assert.Equal("<blockquote>quoted</blockquote>", MarkdownConverter.ToHtml("> quoted"));
    }

    [Fact]
    public void ToHtml_InlineCode()
    {
        Assert.Equal("<p>run <code>dotnet build</code> now</p>", MarkdownConverter.ToHtml("run `dotnet build` now"));
    }

    [Fact]
    public void ToHtml_InlineCode_IsNotFurtherFormatted()
    {
        // '*' inside a code span must stay literal, not become <em>.
        Assert.Equal("<p><code>a*b*c</code></p>", MarkdownConverter.ToHtml("`a*b*c`"));
    }

    [Fact]
    public void ToHtml_FencedCodeBlock()
    {
        var html = MarkdownConverter.ToHtml("```\nline1\nline2\n```");

        Assert.Equal("<pre><code>line1\nline2</code></pre>", html);
    }

    [Fact]
    public void ToHtml_FencedCodeBlock_WithLanguage_EmitsLanguageClass()
    {
        var html = MarkdownConverter.ToHtml("```sql\nSELECT 1\n```");

        Assert.Equal("<pre><code class=\"language-sql\">SELECT 1</code></pre>", html);
    }

    [Fact]
    public void ToHtml_FencedCodeBlock_WithoutLanguage_HasNoClass()
    {
        var html = MarkdownConverter.ToHtml("```\nplain\n```");

        Assert.Equal("<pre><code>plain</code></pre>", html);
    }

    [Fact]
    public void ToHtml_FencedCodeBlock_KeepsMarkdownLiteral()
    {
        var html = MarkdownConverter.ToHtml("```\n# not a heading\n**not bold**\n```");

        Assert.Contains("# not a heading", html);
        Assert.Contains("**not bold**", html);
        Assert.DoesNotContain("<h1>", html);
        Assert.DoesNotContain("<strong>", html);
    }

    [Theory]
    [InlineData("---")]
    [InlineData("***")]
    [InlineData("___")]
    [InlineData("- - -")]
    public void ToHtml_HorizontalRule(string input)
    {
        Assert.Equal("<hr />", MarkdownConverter.ToHtml(input));
    }

    [Fact]
    public void ToHtml_HorizontalRule_BetweenParagraphs()
    {
        Assert.Equal("<p>a</p>\n<hr />\n<p>b</p>", MarkdownConverter.ToHtml("a\n\n---\n\nb"));
    }

    [Fact]
    public void ToHtml_Table()
    {
        var markdown = "| H1 | H2 |\n| --- | --- |\n| a | b |";

        var html = MarkdownConverter.ToHtml(markdown);

        Assert.Contains("<table>", html);
        Assert.Contains("<th>H1</th>", html);
        Assert.Contains("<th>H2</th>", html);
        Assert.Contains("<td>a</td>", html);
        Assert.Contains("<td>b</td>", html);
    }

    [Fact]
    public void ToHtml_SafeLink_RendersAnchor()
    {
        var html = MarkdownConverter.ToHtml("[itree](https://itree.ch)");

        Assert.Contains("<a href=\"https://itree.ch\"", html);
        Assert.Contains(">itree</a>", html);
    }

    [Theory]
    [InlineData("[x](javascript:alert(1))")]
    [InlineData("[x](javascript%3Aalert(1))")]
    [InlineData("[x](javascript%253Aalert(1))")]
    [InlineData("[x](javascript%25253Aalert(1))")]
    [InlineData("[x](data:text/html,hi)")]
    public void ToHtml_UnsafeLink_IsNotRenderedAsAnchor(string input)
    {
        var html = MarkdownConverter.ToHtml(input);

        Assert.DoesNotContain("<a", html);
    }

    [Fact]
    public void ToHtml_InlineCode_DoesNotCollideWithLiteralDigits()
    {
        // " 0 " in the surrounding text must not be mistaken for the
        // internal placeholder of the first code span.
        var html = MarkdownConverter.ToHtml("`foo` and 0 items");

        Assert.Equal("<p><code>foo</code> and 0 items</p>", html);
    }

    [Fact]
    public void ToHtml_LinkWithQueryString_PreservesAmpersandAsEntity()
    {
        var html = MarkdownConverter.ToHtml("[s](https://example.com/?a=1&b=2)");

        // &amp; is the correct HTML-attribute encoding; the browser decodes it to &.
        Assert.Contains("href=\"https://example.com/?a=1&amp;b=2\"", html);
    }

    [Fact]
    public void ToHtml_EncodesEmbeddedHtml()
    {
        var html = MarkdownConverter.ToHtml("<script>alert(1)</script>");

        Assert.DoesNotContain("<script>", html);
        Assert.Contains("&lt;script&gt;", html);
    }
}
