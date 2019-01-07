using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Vertx {
    internal abstract class DocumentationContentBase : IButtonRegistry
    {
        public abstract VisualElement GetRoot(VisualElement root = null);
        public abstract void SetCurrentDefaultRoot(VisualElement root);
        public abstract void AddToRoot(VisualElement element, VisualElement root = null);
        public abstract bool InitialiseContent();
        public abstract void Home();
        public abstract void GoToPage(string pageName, bool addToHistory = true);

        public abstract string GetTitleFromPage(Type pageType);
        public abstract Color GetColorFromPage(Type pageType);
        
        public abstract bool RegisterButton(string key, Action action);
        public abstract bool GetRegisteredButtonAction(string key, out Action action);
    }
}