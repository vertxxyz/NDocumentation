﻿using System;
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
			public DocumentationPage<T> pageOfOrigin;
		}
		#endregion
		
		public abstract Color Color { get; }
		public abstract string Title { get; }
		
		public abstract void DrawDocumentation(T window, VisualElement root);
		public virtual void DrawDocumentationAfterAdditions(T window, VisualElement root) { }
		public virtual void Initialise(T window) { }
	}
}