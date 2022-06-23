using System;

namespace IBaseFramework.Id
{
    internal class DisposableAction : IDisposable
    {
        readonly Action _action;

        internal DisposableAction(Action action)
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