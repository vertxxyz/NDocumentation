using System;
using UnityEngine.UIElements;

namespace Vertx.Example
{
	public class LandingPage : DocumentationPageRoot
	{
		public override Type ParentDocumentationWindowType => typeof(ExampleWindow);

		public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
		{
			window.AddHeader("nDocumentation");
			window.AddPlainText("NDocumentation provides an extensible Unity documentation solution with inline buttons and styling.");
		}
	}
}