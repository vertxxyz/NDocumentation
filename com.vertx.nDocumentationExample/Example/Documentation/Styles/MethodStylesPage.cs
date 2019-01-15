using System;
using UnityEngine;

namespace Vertx.Example
{
	public class MethodStylesPage : DocumentationPageAddition<ExampleWindow>
	{
		public override Type PageToAddToType => typeof(StylingPage);
		public override float Order => 1;

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddVerticalSpace(15);
			window.AddHeader("Methods", 15);

			//Full Width Buttons
			window.AddFullWidthButton("Full Width Buttons", Color.magenta, RichTextStylesPage.buttonKey);
			window.AddRichText($"<code>window.AddFullWidthButton(\"Label\", Color.magenta, buttonKey);</code>");

			//Vertical Space
			window.AddPlainText("Vertical");
			window.AddVerticalSpace(5);
			window.AddPlainText("Space");
			window.AddRichText($"<code>window.AddVerticalSpace(5);</code>");

			//Header
			window.AddHeader(RichTextUtility.GetColouredString("Headers", Color.gray), 14);
			window.AddRichText($"<code>window.AddHeader(\"Headers\");</code>");

			//Splitter
			window.AddPlainText("Splitter:");
			window.AddSplitter();
			window.AddRichText($"<code>window.AddSplitter();</code>");
		}
	}
}