using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Vertx
{
	public class LandingPage : DocumentationPage
	{
		public override Color Color => Color.clear;
		public override string Title => string.Empty;
		public override ButtonInjection[] InjectButtonLinkAbove => null;
		public override ButtonInjection[] InjectButtonLinkBelow => null;
		public override void DrawDocumentation(VisualElement root)
		{
			
		}

		public override void Initialise(DocumentationWindow window)
		{
			
		}
	}
}