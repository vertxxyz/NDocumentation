using System;
using UnityEngine.Experimental.UIElements;

namespace Vertx
{
    public abstract class DocumentationPageRoot : IDocumentationPage
    {
        public abstract Type ParentDocumentationWindowType { get; }
        public abstract void DrawDocumentation(VisualElement root);
        public abstract void Initialise(DocumentationWindow window);
    }
}