using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx.Example
{
    public class ExamplePage : DocumentationPage
    {
        public override ButtonInjection[] InjectButtonLinkAbove => null;
        public override ButtonInjection[] InjectButtonLinkBelow => new []{new ButtonInjection(typeof(LandingPage), 0)};
        public override Color Color => new Color(1f, 0.11f, 0.33f);
        public override string Title => "Creating Pages";
        public override void DrawDocumentation(VisualElement root)
        {
            
        }
    }
}