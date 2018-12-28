using System;

namespace Vertx {
    public interface IButtonRegistry
    {
        bool RegisterButton(string key, Action action);
        bool GetRegisteredButtonAction(string key, out Action action);
    }
}