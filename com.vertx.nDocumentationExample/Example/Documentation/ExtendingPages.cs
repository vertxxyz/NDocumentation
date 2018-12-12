using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
    public class ExtendingPages : DocumentationPage
    {
        public override ButtonInjection[] InjectButtonLinkAbove => null;
        public override ButtonInjection[] InjectButtonLinkBelow => new[] {new ButtonInjection(typeof(LandingPage), 1)};
        public override Color Color => ExtendColor;
        public override string Title => "Extending Pages";
        public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
        {
            window.AddHeader("Extending Pages", 18, FontStyle.Normal);
        }

        public static readonly Color ExtendColor = new Color(1f, 0.87f, 0.32f);
        public static readonly string extendingPages = GetButtonString(typeof(ExtendingPages), GetColouredString("Extending Pages", ExtendColor));
    }
}