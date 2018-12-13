using System;
using UnityEngine.UIElements;

namespace Vertx.Example
{
	public class PageAddition : DocumentationPageAddition
	{
		public override Type PageToAddToType => typeof(ExtendingPages);
		public override float Order => 0;

		public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
		{
			window.AddRichText($"This content after the header has been injected into the page using a {DocumentationPageAddition}.");

			window.AddRichText(@"<code>public class FooAddition : DocumentationPageAddition
{
	public override Type PageToAddToType => typeof(BarPage);
	public override float Order => 0;

	public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
	{
		...
	}
}</code>");
		}

		public static readonly string DocumentationPageAddition = RichTextUtility.GetColouredString(nameof(DocumentationPageAddition), ExtendingPages.ExtendColor);
	}
}