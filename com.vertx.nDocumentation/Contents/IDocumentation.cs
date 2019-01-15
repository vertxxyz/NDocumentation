namespace Vertx
{
	internal interface IDocumentation<in T> where T : DocumentationWindow
	{
		/// <summary>
		/// Use this function to draw documentation content.
		/// </summary>
		/// <param name="window">The window associated with this documentation. For use in calling helper-functions that construct buttons, labels, and rich text.</param>
		void DrawDocumentation(T window);

		/// <summary>
		/// Called when the DocumentationWindow Enables.
		/// </summary>
		/// <param name="window">The EditorWindow that called Initialise</param>
		void Initialise(T window);
	}
}