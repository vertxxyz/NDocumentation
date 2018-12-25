using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
	public abstract class DocumentationPageRoot : IDocumentationPage
	{
		public abstract Type ParentDocumentationWindowType { get; }
		public abstract void DrawDocumentation(DocumentationWindow window, VisualElement root);
		public virtual void DrawDocumentationAfterAdditions(DocumentationWindow window, VisualElement root) { }
		public virtual void Initialise(DocumentationWindow window) { }
		public virtual Color Color => Color.grey;
		public virtual string Title => "Home";
	}
}