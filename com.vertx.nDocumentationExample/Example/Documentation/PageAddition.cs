using System;
using UnityEngine.UIElements;

namespace Vertx.Example
{
    public class PageAddition : DocumentationPageAddition
    {
        public override Type PageToAddToType => typeof(ExtendingPages);
        public override float Order => 0;
        public override void DrawDocumentation(DocumentationWindow window, VisualElement root)
        {
            window.AddPlainText($"This content after the header has been injected into the page using a {nameof(DocumentationPageAddition)}.");
        }
    }
}