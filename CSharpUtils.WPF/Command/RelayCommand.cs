using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Windows.Input;
using CSharpUtils.WPF.Helpers;

namespace CSharpUtils.WPF.Command
{
    public class RelayCommand : ICommand
    {
        private readonly WeakFunc<bool> _canExecute;
        private readonly WeakAction _execute;

        private EventHandler _requerySuggestedLocal;

        /// <summary>
        ///     Initializes a new instance of the RelayCommand class that
        ///     can always execute.
        /// </summary>
        /// <param name="execute">
        ///     The execution logic. IMPORTANT: Note that closures are not supported at the moment
        ///     due to the use of WeakActions (see http://stackoverflow.com/questions/25730530/).
        /// </param>
        /// <exception cref="ArgumentNullException">If the execute argument is null.</exception>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the RelayCommand class.
        /// </summary>
        /// <param name="execute">
        ///     The execution logic. IMPORTANT: Note that closures are not supported at the moment
        ///     due to the use of WeakActions (see http://stackoverflow.com/questions/25730530/).
        /// </param>
        /// <param name="canExecute">The execution status logic.</param>
        /// <exception cref="ArgumentNullException">
        ///     If the execute argument is null. IMPORTANT: Note that closures are not supported at the moment
        ///     due to the use of WeakActions (see http://stackoverflow.com/questions/25730530/).
        /// </exception>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            _execute = new WeakAction(execute);

            if (canExecute != null)
                _canExecute = new WeakFunc<bool>(canExecute);
        }

        #region ICommand Members

        /// <summary>
        ///     Occurs when changes occur that affect whether the command should execute.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add
            {
                if (_canExecute != null)
                {
                    // add event handler to local handler backing field in a thread safe manner
                    EventHandler handler2;
                    EventHandler canExecuteChanged = _requerySuggestedLocal;

                    do
                    {
                        handler2 = canExecuteChanged;
                        var handler3 = (EventHandler) Delegate.Combine(handler2, value);
                        canExecuteChanged = Interlocked.CompareExchange(
                            ref _requerySuggestedLocal,
                            handler3,
                            handler2);
                    } while (canExecuteChanged != handler2);

                    CommandManager.RequerySuggested += value;
                }
            }

            remove
            {
                if (_canExecute != null)
                {
                    // removes an event handler from local backing field in a thread safe manner
                    EventHandler handler2;
                    EventHandler canExecuteChanged = _requerySuggestedLocal;

                    do
                    {
                        handler2 = canExecuteChanged;
                        var handler3 = (EventHandler) Delegate.Remove(handler2, value);
                        canExecuteChanged = Interlocked.CompareExchange(
                            ref _requerySuggestedLocal,
                            handler3,
                            handler2);
                    } while (canExecuteChanged != handler2);

                    CommandManager.RequerySuggested -= value;
                }
            }
        }

        /// <summary>
        ///     Defines the method that determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        /// <returns>true if this command can be executed; otherwise, false.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null || (_canExecute.IsStatic || _canExecute.IsAlive) && _canExecute.Execute();
        }

        /// <summary>
        ///     Defines the method to be called when the command is invoked.
        /// </summary>
        /// <param name="parameter">This parameter will always be ignored.</param>
        public virtual void Execute(object parameter)
        {
            if (CanExecute(parameter) && _execute != null && (_execute.IsStatic || _execute.IsAlive))
                _execute.Execute();
        }

        #endregion

        /// <summary>
        ///     Raises the <see cref="CanExecuteChanged" /> event.
        /// </summary>
        [SuppressMessage(
            "Microsoft.Performance",
            "CA1822:MarkMembersAsStatic",
            Justification = "The this keyword is used in the Silverlight version")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event")]
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}