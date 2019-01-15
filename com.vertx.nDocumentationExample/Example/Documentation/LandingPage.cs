using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Example
{
	public sealed class LandingPage : DocumentationPage<ExampleWindow>
	{
		public override ButtonInjection[] InjectedButtonLinks => new[] {new ButtonInjection(typeof(ExampleWindow), 0)};
		public override Color Color => Color.grey;
		public override string Title => "Home";

		public override void DrawDocumentation(ExampleWindow window)
		{
			window.AddHeader("<b>nDocumentation</b>");
			window.AddVerticalSpace(5);
			window.AddPlainText("NDocumentation provides an extensible Unity documentation window solution with inline buttons and styling.");
			window.AddVerticalSpace(5);
			window.AddSplitter();
			window.AddVerticalSpace(15);
			window.AddRichText("To learn about creating pages first you must learn:");
			window.AddFullWidthButton(typeof(LayoutPage));
			window.AddVerticalSpace(5);
			window.AddRichText("Afterwards, the following topics can be covered:");
		}

		public static void AddNextButton(DocumentationWindow window, Type pageType)
		{
			window.AddVerticalSpace(8);
			
			window.AddPlainText("Next:");
			window.AddFullWidthButton(pageType);
		}
	}
}