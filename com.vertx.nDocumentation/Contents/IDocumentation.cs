using UnityEngine.UIElements;

namespace Vertx
{
	internal interface IDocumentation
	{
		/// <summary>
		/// Use this function to draw documentation content.
		/// </summary>
		/// <param name="window">The window associated with this documentation. For use in calling helper-functions that construct buttons, labels, and rich text.</param>
		/// <param name="root">The root element to add content to.</param>
		void DrawDocumentation(DocumentationWindow window, VisualElement root);

		/// <summary>
		/// Called when the DocumentationWindow Enables.
		/// </summary>
		/// <param name="window">The EditorWindow that called Initialise</param>
		void Initialise(DocumentationWindow window);
	}
}