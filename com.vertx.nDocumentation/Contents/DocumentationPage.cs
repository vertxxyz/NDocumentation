using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
	public abstract class DocumentationPage : IDocumentationPage
	{
		#region Button Links
		/// <summary>
		/// Page link Button to be injected above a DocumentationPage's content
		/// </summary>
		public abstract ButtonInjection[] InjectButtonLinkAbove { get; }
		/// <summary>
		/// Page link Button to be injected below a DocumentationPage's content
		/// </summary>
		public abstract ButtonInjection[] InjectButtonLinkBelow { get; }

		/// <summary>
		/// A description of a button to be injected above or below a DocumentationPage's content.
		/// </summary>
		public class ButtonInjection
		{
			/// <summary>
			/// Type name nameof(DocumentationPage) that should contain this page's link
			/// </summary>
			public readonly Type pageType;
			/// <summary>
			/// The order this link is injected (lower is higher)
			/// </summary>
			public readonly float order;

			public ButtonInjection(Type pageType, float order)
			{
				this.pageType = pageType;
				this.order = order;
			}

			/// <summary>
			/// Assigned internally.
			/// </summary>
			public DocumentationPage pageOfOrigin;
		}
		#endregion
		public abstract Color Color { get; }
		public abstract string Title { get; }

		public abstract void DrawDocumentation(DocumentationWindow window, VisualElement root);
		public virtual void Initialise(DocumentationWindow window) { }
	}
}