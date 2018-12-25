using System;
using UnityEditor;
using UnityEngine.UIElements;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
	public sealed class PageAddition : DocumentationPageAddition
	{
		public override Type PageToAddToType => typeof(ExtendingPages);
		public override float Order => 0;

		public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
		{
			window.AddRichText($"This content after the header has been injected into the page using a {DocumentationPageAdditionString}.");

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

		public static readonly string DocumentationPageAdditionString = GetColouredString(nameof(DocumentationPageAddition), ExtendingPages.ExtendColor);
		public static readonly string DocumentationPageAdditionsSpacedString = GetColouredString(ObjectNames.NicifyVariableName($"{nameof(DocumentationPageAddition)}s"), ExtendingPages.ExtendColor);
	}
}