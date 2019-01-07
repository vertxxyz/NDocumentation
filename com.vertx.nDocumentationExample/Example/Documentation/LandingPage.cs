﻿using System;
using UnityEngine.UIElements;

namespace Vertx.Example
{
	public sealed class LandingPage : DocumentationPageRoot<ExampleWindow>
	{
		public override void DrawDocumentation(ExampleWindow window, VisualElement root)
		{
			window.AddHeader("nDocumentation");
			window.AddVerticalSpace(5);
			window.AddPlainText("NDocumentation provides an extensible Unity documentation window solution with inline buttons and styling.");
			window.AddVerticalSpace(5);
			window.AddSplitter();
			window.AddRichText("To learn about creating pages first you must learn:");
			window.AddFullWidthButton(typeof(LayoutPage));
			window.AddVerticalSpace(5);
			window.AddRichText("");
		}

		public static void AddNextButton(DocumentationWindow window, Type pageType)
		{
			window.AddVerticalSpace(8);
			
			window.AddPlainText("Next:");
			window.AddFullWidthButton(pageType);
		}
	}
}