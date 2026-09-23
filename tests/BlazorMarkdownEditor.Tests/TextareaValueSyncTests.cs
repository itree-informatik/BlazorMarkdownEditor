using System.Runtime.ExceptionServices;
using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

// Inspecting the render diffs is the point of these tests.
#pragma warning disable BL0006

namespace BlazorMarkdownEditor.Tests;

/// <summary>
/// Regression tests for the textarea losing its caret while typing (issue #1).
/// bUnit rebuilds markup from the final render tree, so it cannot tell whether a
/// re-render writes <c>textarea.value</c> back to the browser. These tests inspect
/// the render diffs instead — exactly what the browser applies to the DOM.
/// </summary>
public class TextareaValueSyncTests
{
    [Fact]
    public async Task Typing_DoesNotWriteValueBackToTextarea()
    {
        await using var renderer = CreateRenderer();
        var componentId = await renderer.RenderAsync<MarkdownEditor>(Parameters("Hello"));
        renderer.ValueWrites.Clear();

        // The browser reports the typed text along with the event (EventFieldInfo).
        // Echoing it back must not produce a DOM write: if that render arrives late
        // (Blazor Server latency, async ValueChanged handlers) it overwrites newer
        // keystrokes and the caret jumps to the end.
        await renderer.TypeAsync(componentId, "Hello!");

        Assert.Empty(renderer.ValueWrites);
    }

    [Fact]
    public async Task ExternalValueChange_IsWrittenToTextarea()
    {
        await using var renderer = CreateRenderer();
        var componentId = await renderer.RenderAsync<MarkdownEditor>(Parameters("Hello"));
        renderer.ValueWrites.Clear();

        await renderer.SetParametersAsync(componentId, Parameters("Replaced"));

        Assert.Equal(["Replaced"], renderer.ValueWrites);
    }

    private static ParameterView Parameters(string value) =>
        ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(MarkdownEditor.Value)] = value });

    private static DiffRecordingRenderer CreateRenderer()
    {
        // The component imports a JS module on first render; loose mode satisfies that.
        var services = new ServiceCollection()
            .AddSingleton(new BunitJSInterop { Mode = JSRuntimeMode.Loose }.JSRuntime)
            .BuildServiceProvider();
        return new DiffRecordingRenderer(services);
    }

    /// <summary>Records every <c>value</c> attribute write (only the textarea has one).</summary>
    private sealed class DiffRecordingRenderer(IServiceProvider services)
        : Renderer(services, NullLoggerFactory.Instance)
    {
        public List<string?> ValueWrites { get; } = [];

        public override Dispatcher Dispatcher { get; } = Dispatcher.CreateDefault();

        public async Task<int> RenderAsync<TComponent>(ParameterView parameters)
            where TComponent : IComponent
        {
            var componentId = AssignRootComponentId(InstantiateComponent(typeof(TComponent)));
            await SetParametersAsync(componentId, parameters);
            return componentId;
        }

        public Task SetParametersAsync(int componentId, ParameterView parameters) =>
            Dispatcher.InvokeAsync(() => RenderRootComponentAsync(componentId, parameters));

        public Task TypeAsync(int componentId, string text)
        {
            var frames = GetCurrentRenderTreeFrames(componentId);
            var onInput = frames.Array.Take(frames.Count)
                .First(f => f.FrameType == RenderTreeFrameType.Attribute && f.AttributeName == "oninput");

            return Dispatcher.InvokeAsync(() => DispatchEventAsync(
                onInput.AttributeEventHandlerId,
                new EventFieldInfo { ComponentId = componentId, FieldValue = text },
                new ChangeEventArgs { Value = text }));
        }

        protected override Task UpdateDisplayAsync(in RenderBatch renderBatch)
        {
            for (var i = 0; i < renderBatch.UpdatedComponents.Count; i++)
            {
                foreach (var edit in renderBatch.UpdatedComponents.Array[i].Edits)
                {
                    if (edit.Type != RenderTreeEditType.SetAttribute)
                        continue;

                    var attribute = renderBatch.ReferenceFrames.Array[edit.ReferenceFrameIndex];
                    if (attribute.AttributeName == "value")
                        ValueWrites.Add(attribute.AttributeValue as string);
                }
            }

            return Task.CompletedTask;
        }

        protected override void HandleException(Exception exception) =>
            ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
