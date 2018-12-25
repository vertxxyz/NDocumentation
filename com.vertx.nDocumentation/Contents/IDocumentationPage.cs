using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
    internal interface IDocumentationPage : IDocumentation
    {
        Color Color { get; }
        string Title { get; }
        void DrawDocumentationAfterAdditions (DocumentationWindow window, VisualElement root);
    }
}