using UnityEngine;
using UnityEngine.UIElements;
using static Vertx.Example.ExtendingPages;
using static Vertx.Example.PageAddition;
using static Vertx.RichTextUtility;

namespace Vertx.Example
{
    public sealed class CreatingPage : DocumentationPage
    {
        public override ButtonInjection[] InjectButtonLinkAbove => null;
        public override ButtonInjection[] InjectButtonLinkBelow => new []{new ButtonInjection(typeof(LandingPage), 0)};
        public override Color Color => WindowPage.CreateColor;
        public override string Title => "Creating Pages";
        public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
        {
            window.AddHeader(Title, 18, FontStyle.Normal);
            window.AddRichText($"Sub-pages can be created with a {DocumentationPageString}.");
            window.AddRichText(@"<code>public class BarPage : DocumentationPage
{
    public override ButtonInjection[] InjectButtonLinkAbove => null;
    public override ButtonInjection[] InjectButtonLinkBelow => new []{new ButtonInjection(typeof(FooWindowPageRoot), 0)};
    public override Color Color => new Color(1,1,1);
    public override string Title => ""Bar Page"";
    public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
    {
        ...
    }
}</code>");
            window.AddRichText($"You can optionally extend a {DocumentationPageString} with additional content by using a {DocumentationPageAdditionString}. (see {ExtendingPagesButton})");
        }
            
        public static readonly string DocumentationPageString = GetColouredString(nameof(DocumentationPage), WindowPage.CreateColor);
    }
}