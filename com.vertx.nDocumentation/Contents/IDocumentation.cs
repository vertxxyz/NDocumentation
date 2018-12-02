using UnityEngine.Experimental.UIElements;

namespace Vertx
{
	internal interface IDocumentation
	{
		/// <summary>
		/// Use this function to draw documentation content.
		/// </summary>
		/// <param name="root">The root element to add content to.</param>
		void DrawDocumentation(VisualElement root);

		void Initialise(DocumentationWindow window);
	}
}