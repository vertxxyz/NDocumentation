using System;

namespace Vertx.Example
{
	// ReSharper disable once UnusedMember.Global
	public class SpanStylesPage : DocumentationPageAddition<ExampleWindow>
	{
		public override Type PageToAddToType => typeof(StylingPage);
		public override float Order => 1;

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddRichText("<span class=\"text-example\">Span Classes</span>\n" +
			                   @"<code>""<span class=\""class-key\"">Span Classes</>""</code>");//This code has an EN SPACE (" ") present to avoid the span parsing.
		}
	}
}