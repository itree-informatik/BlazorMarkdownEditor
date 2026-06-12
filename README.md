# BlazorMarkdownEditor

A lightweight, **dependency-free** Markdown editor component for Blazor — with
**Write** and **Preview** tabs and a formatting toolbar, similar to the GitHub
issue editor.

- No third-party UI dependencies (no Telerik, MudBlazor, Bootstrap, …).
- No external Markdown library — rendering is built in.
- English defaults out of the box, fully localizable via parameters.
- Safe preview: embedded HTML is encoded, so the preview cannot execute scripts.

## Features

- Tabs: **Write** (Markdown source) and **Preview** (rendered HTML).
- Toolbar: headings, **bold**, *italic*, bulleted & numbered lists, links, tables.
- Two-way binding of the raw Markdown source via `@bind-Value`.
- Validation support inside `EditForm` (`ValueExpression`).

## Installation

```bash
dotnet add package BlazorMarkdownEditor
```

No `<script>` or `<link>` tags are required — the JavaScript module and the
component styles are served automatically as static web assets. (Make sure your
app references the framework's scoped-CSS bundle, i.e. the
`<link href="<AppName>.styles.css" />` that Blazor projects include by default.)

## Usage

```razor
@using BlazorMarkdownEditor

<MarkdownEditor @bind-Value="_text" Placeholder="Leave a comment" />

@code {
    private string _text = string.Empty;
}
```

`Value` always holds the **raw Markdown source**. Render it to HTML yourself
when displaying it elsewhere:

```csharp
string html = MarkdownConverter.ToHtml(_text);
```

## Localization

All texts default to English and can be overridden through the `Labels`
parameter — for example by wiring them up to your own translation system:

```razor
<MarkdownEditor @bind-Value="_text"
                Labels="@(new MarkdownEditorLabels
                {
                    WriteTab    = Translate("MARKDOWN_WRITE"),
                    PreviewTab  = Translate("MARKDOWN_PREVIEW"),
                    BoldTooltip = Translate("MARKDOWN_BOLD"),
                    // ...
                })" />
```

## Parameters

| Parameter         | Type                    | Default     | Description                                  |
|-------------------|-------------------------|-------------|----------------------------------------------|
| `Value`           | `string`                | `""`        | The Markdown source (use with `@bind-Value`).|
| `ValueChanged`    | `EventCallback<string>` | —           | Raised when the source changes.              |
| `ValueExpression` | `Expression<Func<…>>`   | —           | Set automatically inside an `EditForm`.      |
| `Labels`          | `MarkdownEditorLabels`  | English     | User-facing texts; override to localize.     |
| `Placeholder`     | `string?`               | `null`      | Placeholder for the empty textarea.          |
| `Rows`            | `int`                   | `10`        | Visible rows of the write area.              |
| `Readonly`        | `bool`                  | `false`     | Renders only the formatted preview — no tabs, toolbar or textarea. |
| `Class`           | `string?`               | `null`      | Extra CSS class(es) for the root element.    |

## Supported Markdown

Headings (`#`–`######`), `**bold**`, `*italic*`, blockquotes (`>`), inline code
(`` `code` ``), fenced code blocks (` ``` `), horizontal rules (`---`), bulleted
(`-`/`*`) and numbered (`1.`) lists, links `[text](url)` (safe schemes only), and
pipe tables.

The toolbar provides: H1, H2, H3, bold, italic, quote, inline code, code block,
bulleted list, numbered list, link and table.

### Syntax highlighting

Fenced code blocks with a language are rendered with a `language-xxx` class and
highlighted using a bundled copy of [highlight.js](https://highlightjs.org/) —
no extra setup required:

    ```sql
    SELECT * FROM Users WHERE Active = 1;
    ```

highlight.js and the GitHub theme ship under `wwwroot/highlight/` and are loaded
on demand the first time a code block is shown. To use a different theme, override
the `.hljs` styles in your app, or replace the bundled theme.

> highlight.js is included under the BSD-3-Clause license — see
> `wwwroot/highlight/LICENSE`.

## License

[MIT](LICENSE) © itree informatik GmbH
