using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
    internal interface IDocumentationPage : IDocumentation
    {
        /// <summary>
        /// The colour of link buttons that reference this page.
        /// </summary>
        Color Color { get; }
        
        /// <summary>
        /// The label of link buttons that reference this page.
        /// </summary>
        string Title { get; }
        
        /// <summary>
        /// Similar to DrawDocumentation, but is drawn after all DocumentationPageAdditions.
        /// </summary>
        /// <param name="window">Parent EditorWindow (contains helper functions)</param>
        /// <param name="root">Visual Element to append UI to.</param>
        void DrawDocumentationAfterAdditions (DocumentationWindow window, VisualElement root);
    }
}