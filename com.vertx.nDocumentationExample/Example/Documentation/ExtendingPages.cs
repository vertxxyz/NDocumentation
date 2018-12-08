using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Example
{
    public class ExtendingPages : DocumentationPage
    {
        public override ButtonInjection[] InjectButtonLinkAbove => null;
        public override ButtonInjection[] InjectButtonLinkBelow => new[] {new ButtonInjection(typeof(LandingPage), 1)};
        public override Color Color => new Color(1f, 0.87f, 0.32f);
        public override string Title => "Extending Pages";
        public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
        {
            window.AddHeader("Extending Pages", 18, FontStyle.Normal);
        }
    }
}