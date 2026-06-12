// JavaScript module for BlazorMarkdownEditor. Loaded on demand via a relative
// import from the component: "./_content/BlazorMarkdownEditor/markdown-editor.js".

// Applies a toolbar action to the textarea identified by `textareaId`.
//
//   "inline": wraps the current selection (or `placeholder` when nothing is
//             selected) with `before` / `after`.
//   "line":   prepends `before` to every line covered by the selection
//             (headings, list items).
//   "insert": inserts `before` at the caret, replacing any selection (tables).
//
// Returns { value, start, end } so the Blazor component can sync its bound value
// and restore the caret after the re-render.
export function applyTool(textareaId, before, after, placeholder, mode) {
    const ta = document.getElementById(textareaId);
    if (!ta) {
        return null;
    }

    const value = ta.value;
    const start = ta.selectionStart;
    const end = ta.selectionEnd;
    const selected = value.substring(start, end);

    let newValue, selStart, selEnd;

    if (mode === 'line') {
        const lineStart = value.lastIndexOf('\n', start - 1) + 1;
        let lineEnd = value.indexOf('\n', end);
        if (lineEnd === -1) {
            lineEnd = value.length;
        }

        const block = value.substring(lineStart, lineEnd);
        const prefixed = block
            .split('\n')
            .map(line => before + (line.length ? line : placeholder))
            .join('\n');

        newValue = value.substring(0, lineStart) + prefixed + value.substring(lineEnd);
        selStart = lineStart + before.length;
        selEnd = lineStart + prefixed.length;
    } else if (mode === 'insert') {
        newValue = value.substring(0, start) + before + value.substring(end);
        selStart = start + before.length;
        selEnd = selStart;
    } else {
        const text = selected.length ? selected : placeholder;
        const insert = before + text + after;
        newValue = value.substring(0, start) + insert + value.substring(end);
        selStart = start + before.length;
        selEnd = selStart + text.length;
    }

    ta.value = newValue;
    ta.focus();
    ta.setSelectionRange(selStart, selEnd);
    return { value: newValue, start: selStart, end: selEnd };
}

// Re-applies the caret/selection after Blazor has re-rendered the textarea (which
// rewrites the value attribute and would otherwise drop the selection).
export function restoreSelection(textareaId, start, end) {
    const ta = document.getElementById(textareaId);
    if (!ta) {
        return;
    }
    ta.focus();
    ta.setSelectionRange(start, end);
}

const BASE = './_content/BlazorMarkdownEditor';
let hljsPromise = null;

// Loads the bundled highlight.js (and its theme) once, lazily.
function ensureHighlighter() {
    if (hljsPromise) {
        return hljsPromise;
    }

    hljsPromise = new Promise(resolve => {
        if (!document.getElementById('bme-hljs-theme')) {
            const link = document.createElement('link');
            link.id = 'bme-hljs-theme';
            link.rel = 'stylesheet';
            link.href = `${BASE}/highlight/github.min.css`;
            document.head.appendChild(link);
        }

        if (window.hljs) {
            resolve(window.hljs);
            return;
        }

        const script = document.createElement('script');
        script.src = `${BASE}/highlight/highlight.min.js`;
        script.onload = () => resolve(window.hljs ?? null);
        script.onerror = () => resolve(null);
        document.head.appendChild(script);
    });

    return hljsPromise;
}

// Highlights every <pre><code> inside the given preview container. Blocks that were
// already highlighted are skipped (fresh markup has no such marker).
export async function highlight(containerId) {
    const container = document.getElementById(containerId);
    if (!container) {
        return;
    }

    const blocks = container.querySelectorAll('pre code');
    if (blocks.length === 0) {
        return;
    }

    const hljs = await ensureHighlighter();
    if (!hljs) {
        return;
    }

    blocks.forEach(block => {
        if (block.dataset.highlighted === 'yes') {
            return;
        }
        hljs.highlightElement(block);
    });
}
