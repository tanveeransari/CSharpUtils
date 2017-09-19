using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Windows;

namespace CSharpUtils.WPF
{
    [SuppressMessage(
        "Microsoft.Design",
        "CA1012",
        Justification = "Constructors should remain public to allow serialization.")]
    public abstract class ViewModelBase : ObservableObject
    {
        private static bool? _isInDesignMode;

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification =
            "The security risk here is neglectible.")]
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (!_isInDesignMode.HasValue)
                {
                    DependencyProperty prop = DesignerProperties.IsInDesignModeProperty;
                    _isInDesignMode
                        = (bool) DependencyPropertyDescriptor
                            .FromProperty(prop, typeof(FrameworkElement))
                            .Metadata.DefaultValue;
                }

                return _isInDesignMode.Value;
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification =
            "Non static member needed for data binding")]
        public bool IsInDesignMode => IsInDesignModeStatic;

        [SuppressMessage(
            "Microsoft.Design",
            "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification =
            "This cannot be an event")]
        public virtual void RaisePropertyChanged<T>(
            string propertyName,
            T oldValue = default(T),
            T newValue = default(T),
            bool broadcast = false)
        {
            if (string.IsNullOrEmpty(propertyName))
                throw new ArgumentException("This method cannot be called with an empty string", nameof(propertyName));

            RaisePropertyChanged(propertyName);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1030:UseEventsWhereAppropriate",
            Justification = "This cannot be an event")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1006:GenericMethodsShouldProvideTypeParameter",
            Justification = "This syntax is more convenient than the alternatives.")]
        public virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression, T oldValue, T newValue,
            bool broadcast)
        {
            RaisePropertyChanged(propertyExpression);
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "This syntax is more convenient than the alternatives.")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1045:DoNotPassTypesByReference",
            MessageId = "1#")]
        protected bool Set<T>(
            Expression<Func<T>> propertyExpression,
            ref T field,
            T newValue,
            bool broadcast)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            RaisePropertyChanging(propertyExpression);
            T oldValue = field;
            field = newValue;
            RaisePropertyChanged(propertyExpression, oldValue, field, broadcast);
            return true;
        }

        [SuppressMessage(
            "Microsoft.Design",
            "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage(
            "Microsoft.Design",
            "CA1045:DoNotPassTypesByReference",
            MessageId = "1#")]
        protected bool Set<T>(
            string propertyName,
            ref T field,
            T newValue = default(T),
            bool broadcast = false)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            RaisePropertyChanging(propertyName);
            T oldValue = field;
            field = newValue;

            RaisePropertyChanged(propertyName, oldValue, field, broadcast);

            return true;
        }
    }
}