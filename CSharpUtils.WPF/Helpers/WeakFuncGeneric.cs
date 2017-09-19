using System;
using System.Diagnostics.CodeAnalysis;

namespace CSharpUtils.WPF.Helpers
{
    public class WeakFunc<T, TResult> : WeakFunc<TResult>, IExecuteWithObjectAndResult
    {
        private Func<T, TResult> _staticFunc;

        public WeakFunc(Func<T, TResult> func)
            : this(func == null ? null : func.Target, func)
        {
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "Method should fail with an exception if func is null.")]
        public WeakFunc(object target, Func<T, TResult> func)
        {
            if (func.Method.IsStatic)
            {
                _staticFunc = func;

                if (target != null)
                    Reference = new WeakReference(target);

                return;
            }

            Method = func.Method;
            FuncReference = new WeakReference(func.Target);

            Reference = new WeakReference(target);
        }

        public override string MethodName
        {
            get
            {
                if (_staticFunc != null)
                    return _staticFunc.Method.Name;
                return Method.Name;
            }
        }

        public override bool IsAlive
        {
            get
            {
                if (_staticFunc == null && Reference == null)
                    return false;

                if (_staticFunc != null)
                {
                    if (Reference != null)
                        return Reference.IsAlive;

                    return true;
                }

                return Reference.IsAlive;
            }
        }

        #region IExecuteWithObjectAndResult Members

        public object ExecuteWithObject(object parameter)
        {
            var parameterCasted = (T) parameter;
            return Execute(parameterCasted);
        }

        #endregion

        public new TResult Execute()
        {
            return Execute(default(T));
        }

        public TResult Execute(T parameter)
        {
            if (_staticFunc != null)
                return _staticFunc(parameter);

            object funcTarget = FuncTarget;

            if (IsAlive)
            {
                if (Method != null && FuncReference != null && funcTarget != null)
                    return (TResult)Method.Invoke(
                        funcTarget,
                        new object[]
                        {
                            parameter
                        });
            }

            return default(TResult);
        }

        public new void MarkForDeletion()
        {
            _staticFunc = null;
            base.MarkForDeletion();
        }
    }
}