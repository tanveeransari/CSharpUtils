using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSharpUtils.WPF.Helpers
{
    public class WeakFunc<TResult>
    {
        private Func<TResult> _staticFunc;

        protected WeakFunc()
        {
        }

        public WeakFunc(Func<TResult> func)
            : this(func == null ? null : func.Target, func)
        {
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "Method should fail with an exception if func is null.")]
        public WeakFunc(object target, Func<TResult> func)
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

        protected MethodInfo Method { get; set; }

        public bool IsStatic => _staticFunc != null;

        public virtual string MethodName
        {
            get
            {
                if (_staticFunc != null)
                    return _staticFunc.Method.Name;

                return Method.Name;
            }
        }

        protected WeakReference FuncReference { get; set; }

        protected WeakReference Reference { get; set; }

        public virtual bool IsAlive
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

        public object Target
        {
            get
            {
                if (Reference == null)
                    return null;

                return Reference.Target;
            }
        }

        protected object FuncTarget
        {
            get
            {
                if (FuncReference == null)
                    return null;

                return FuncReference.Target;
            }
        }

        public TResult Execute()
        {
            if (_staticFunc != null)
                return _staticFunc();

            object funcTarget = FuncTarget;

            if (IsAlive)
            {
                if (Method != null && FuncReference != null && funcTarget != null)
                    return (TResult)Method.Invoke(funcTarget, null);
            }

            return default(TResult);
        }

        public void MarkForDeletion()
        {
            Reference = null;
            FuncReference = null;
            Method = null;
            _staticFunc = null;
        }
    }
}