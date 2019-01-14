using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx
{
	/// <summary>
	/// Inheriting from Documentation Page creates Page content that's linkable in a DocumentationWindow.
	/// Links to this Page can also be injected into other pages.
	/// </summary>
	public abstract class DocumentationPage<T> : IDocumentationPage<T> where T : DocumentationWindow
	{
		#region Button Links

		/// <summary>
		/// Page link Button to be injected below a DocumentationPage's content.
		/// If your page is the root of the DocumentationWindow, use a ButtonInjection with your DocumentationWindow Type as the first index.
		/// </summary>
		public abstract ButtonInjection[] InjectedButtonLinks { get; }

		/// <summary>
		/// A description of a button to be injected above or below a DocumentationPage's content.
		/// </summary>
		public class ButtonInjection
		{
			/// <summary>
			/// Type of DocumentationPage that should contain this page's link.
			/// If your page is the root of the DocumentationWindow, use a ButtonInjection with your DocumentationWindow Type.
			/// </summary>
			public readonly Type ParentType;

			/// <summary>
			/// The order this link is injected (lower is higher)
			/// </summary>
			public readonly float Order;

			public ButtonInjection(Type parentType, float order)
			{
				ParentType = parentType;
				Order = order;
			}

			/// <summary>
			/// Assigned internally.
			/// </summary>
			public DocumentationPage<T> PageOfOrigin;
		}

		#endregion

		public abstract Color Color { get; }
		public abstract string Title { get; }

		public abstract void DrawDocumentation(T window, VisualElement root);
		public virtual void DrawDocumentationAfterAdditions(T window, VisualElement root) { }
		public virtual void Initialise(T window) { }
	}
}