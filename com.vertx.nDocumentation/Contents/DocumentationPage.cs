using System;
using UnityEngine;
using UnityEngine.Experimental.UIElements;

namespace Vertx
{
	public abstract class DocumentationPage : IDocumentationPage
	{
		public abstract Color Color { get; }
		public abstract string Title { get; }

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
		public struct ButtonInjection
		{
			//Type name nameof(DocumentationPage) that should contain this page's link
			public readonly Type pageType;
			//The order this link is injected (lower is higher)
			public readonly float order;

			public ButtonInjection(Type pageType, float order)
			{
				this.pageType = pageType;
				this.order = order;
			}
		}
		#endregion
		
		public abstract void DrawDocumentation(VisualElement root);
		public abstract void Initialise(DocumentationWindow window);
	}
}