using System;

namespace IBaseFramework.IdHelper
{
    public class DisposableAction : IDisposable
    {
        readonly Action _action;

        public DisposableAction(Action action)
        {
            if (action != null)
                _action = action;
            else
                throw new ArgumentNullException(nameof(action));
        }

        public void Dispose()
        {
            _action();
        }
    }
}