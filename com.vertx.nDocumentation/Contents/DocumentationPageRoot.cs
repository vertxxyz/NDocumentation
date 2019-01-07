using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
	/// <summary>
	/// Inheriting from Documentation Page Root creates Page content that's the first page displayed in the parent DocumentationWindow.
	/// </summary>
	public abstract class DocumentationPageRoot<T> : IDocumentationPage<T> where T : DocumentationWindow
	{
		/// <summary>
		/// Add UI to root or use window functions to draw documentation content
		/// </summary>
		/// <param name="window">Parent EditorWindow (contains helper functions)</param>
		/// <param name="root">Visual Element to append UI to.</param>
		public abstract void DrawDocumentation(T window, VisualElement root);
		
		public virtual void DrawDocumentationAfterAdditions(T window, VisualElement root) { }
		
		public virtual void Initialise(T window) { }

		public virtual Color Color => Color.grey;
		public virtual string Title => "Home";
	}
}