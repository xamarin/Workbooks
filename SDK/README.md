# Xamarin Interactive SDK

This document describes the integration SDK for Xamarin Workbooks & Inspector
(Xamarin Interactive).

## Architecture Overview

Xamarin Interactive features two main components which must work in
conjunction with each other: _Agent_ and _Client_.

### Interactive Agent

The Agent component is a small platform-specific assembly which runs in the
context of a .NET application.

During live inspection (Xamarin Inspector), the agent is injected via the
IDE debugger into an existing application as part of the regular development &
debugging workflow.

Xamarin Workbooks provides pre-built "empty" applications for a number of
platforms, such as iOS, Android, Mac and WPF. These applications explicitly
host the agent.

### Interactive Client

The client is a native shell (Cocoa on Mac, WPF on Windows) that hosts a web
browser surface for presenting the workbook/REPL interface. From an SDK
perspective, all client integrations are implemented in JavaScript and CSS.

The client is responsible (via Roslyn) for compiling source code into small
assemblies and sending them over to the connected agent for execution. Results
of execution are sent back to the client for rendering.

Because an agent can be running on any type of .NET platform and has access to
anything in the running application, care must be taken to serialize results in
a platform-agnostic manner.

### Representations

Within a workbook or inspector session, code that is executed and yields a
result (e.g. a method returning a value or the result of an expression) is
processed through the representation pipeline in the agent. All objects, with
the exception of primitives such as integers, will be reflected to produce
interactive member graphs and will go through a process to provide alternate
representations that the client can render more richly. Objects of any size and
depth are safely supported (including cycles and infinite enumerables) due to
lazy and interactive reflection and remoting.

Xamarin Interactive provides a few types common to all agents and clients that
allow for rich rendering of results. `Color` is one example of such a type,
where for example on iOS, the agent is responsible for converting `CGColor` or
`UIColor` objects into a `Xamarin.Interactive.Representations.Color` object.

In addition to common representations, the integration SDK provides APIs for
serializing custom representations in the agent and rendering representations
in the client.

## External Integrations

Integration assemblies should reference the `Xamarin.Interactive` PCL.

Eventually the SDK will be released directly as a NuGet package, but for now any
integrations should copy the SDK dependencies into their projects explicitly.

<table>
<tr>
<th>Mac</th>
<td><code>
/Library/Frameworks/Xamarin.Interactive.framework/Versions/Current/SDK
</code></td>
</tr>
<tr>
<th>Windows</th>
<td><code>
C:\Program Files (x86)\Xamarin\Workbooks\SDK
</code></td>
</tr>
</table>

Client integrations are initiated by placing JavaScript or CSS files with the
same name as the agent integration assembly in the same directory. For example,
if the agent integration assembly (which references the `Xamarin.Interactive`
PCL) is named `SampleExternalIntegration.dll`, then
`SampleExternalIntegration.js` and `SampleExternalIntegration.css` will be
integrated into the client as well if they exist. Client integrations are
optional.

The external integration itself can be packaged as a NuGet, provided and
referenced directly inside the application that is hosting the agent, or simply
placed alongside a `.workbook` file that wishes to consume it.

A workbook or live inspect session will load the integration by referencing the
integration assembly.

```csharp
#r "SampleExternalIntegration.dll"
```

The `Xamarin.Interactive` PCL provides a few important integration APIs. Every
integration must at least provide an integration entry point:

```csharp
using Xamarin.Interactive;

[assembly: AgentIntegration (typeof (AgentIntegration))]

class AgentIntegration : IAgentIntegration
{
	const string TAG = nameof (AgentIntegration);

	public void IntegrateWith (IAgent agent)
	{
		// hook into IAgent APIs
	}
}
```

At this point, once the integration assembly is referenced, the client will
implicitly load JavaScript and CSS integration files.

### APIs

As with any assembly that is referenced by a workbook or live inspect session,
any of its public APIs are accessible to the session. Therefore it is
important to have a safe and sensible API surface for users to explore.

The integration assembly is effectively a bridge between an application or
SDK of interest and the session. It can provide new APIs that make sense
specifically in the context of a workbook or live inspect session, or provide
no public APIs and simply perform "behind the scenes" tasks like yielding
object representations.

_Note: APIs which must be public but should not be surfaced via IntelliSense
can be marked with the usual `[EditorBrowsable (EditorBrowsableState.Never)]`
attribute._

### External Representations

`Xamarin.Interactive.IAgent.RepresentationManager` provides the ability to
register a `RepresentationProvider`, which an integration must implement to
convert from an arbitrary object to an agnostic form to render. These agnostic
forms must implement the `ISerializableObject` interface.

_Note: APIs that produce `ISerializableObject` objects directly do not need
to be handled by a `RepresentationProvider`._

#### Rendering a Representation

Renderers are implemented in JavaScript and will have access to a JavaScript
version of the object represented via `ISerializableObject`. The JavaScript
copy will also have a `$type` string property that indicates the .NET type
name.

We recommend using TypeScript for client integration code, which of course
compiles to vanilla JavaScript. Either way, the SDK provides [typings][typings]
which can be referenced directly by TypeScript or simply referred to manually
if writing vanilla JavaScript is preferred.

The main integration point for rendering is
`xamarin.interactive.RendererRegistry`:

```js
xamarin.interactive.RendererRegistry.registerRenderer(
  function (source) {
    if (source.$type === "SampleExternalIntegration.Person")
      return new PersonRenderer;
    return undefined;
  }
);
```

Here, `PersonRenderer` implements the `Renderer` interface. See the
[typings][typings] for more details.

![](Resources/KitchenSinkIntegrationScreenshot.png)

[typings]: typings/xamarin-interactive.d.ts