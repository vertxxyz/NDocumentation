using System;
using UnityEditor;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	// ReSharper disable once ClassNeverInstantiated.Global
	public sealed class PageAddition : DocumentationPageAddition<ExampleWindow>
	{
		public override Type PageToAddToType => typeof(ExtendingPages);
		public override float Order => 0;

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddRichText($"<b><i>This content</i></b> has been injected into the page using a {DocumentationPageAdditionString}.");

			window.AddRichText(@"<code>public class FooAddition : DocumentationPageAddition<FooWindow>
{
	public override Type PageToAddToType => typeof(BarPage);
	public override float Order => 0;

	public override void DrawDocumentation(FooWindow window)
	{
		...
	}
}</code>");
		}

		public static readonly string DocumentationPageAdditionString = GetColouredString(nameof(DocumentationPageAddition<DocumentationWindow>), ExtendingPages.ExtendColor);
		public static readonly string DocumentationPageAdditionsSpacedString = GetColouredString(ObjectNames.NicifyVariableName($"{nameof(DocumentationPageAddition<DocumentationWindow>)}s"), ExtendingPages.ExtendColor);
	}
}