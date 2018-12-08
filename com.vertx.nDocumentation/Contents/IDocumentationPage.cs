using UnityEngine;

namespace Vertx
{
    internal interface IDocumentationPage : IDocumentation
    {
        Color Color { get; }
        string Title { get; }
    }
}