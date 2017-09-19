using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace CSharpUtils.WPF.Helpers
{
    public class WeakAction
    {
        private Action _staticAction;

        protected WeakAction()
        {
        }

        public WeakAction(Action action)
            : this(action?.Target, action)
        {
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1062:Validate arguments of public methods",
            MessageId = "1",
            Justification = "Method should fail with an exception if action is null.")]
        public WeakAction(object target, Action action)
        {
            if (action.Method.IsStatic)
            {
                _staticAction = action;

                if (target != null)
                    Reference = new WeakReference(target);

                return;
            }

            Method = action.Method;
            ActionReference = new WeakReference(action.Target);

            Reference = new WeakReference(target);
        }

        protected MethodInfo Method { get; set; }

        public virtual string MethodName
        {
            get
            {
                if (_staticAction != null)
                    return _staticAction.Method.Name;
                return Method.Name;
            }
        }

        protected WeakReference ActionReference { get; set; }

        protected WeakReference Reference { get; set; }

        public bool IsStatic => _staticAction != null;

        public virtual bool IsAlive
        {
            get
            {
                if (_staticAction == null && Reference == null)
                    return false;

                if (_staticAction != null)
                {
                    if (Reference != null)
                        return Reference.IsAlive;

                    return true;
                }

                return Reference.IsAlive;
            }
        }

        public object Target => Reference?.Target;

        protected object ActionTarget => ActionReference?.Target;

        public void Execute()
        {
            if (_staticAction != null)
            {
                _staticAction();
                return;
            }

            object actionTarget = ActionTarget;

            if (IsAlive)
            {
                if (Method != null && ActionReference != null && actionTarget != null)
                {
                    Method.Invoke(actionTarget, null);
                }
            }
        }

        public void MarkForDeletion()
        {
            Reference = null;
            ActionReference = null;
            Method = null;
            _staticAction = null;
        }
    }
}