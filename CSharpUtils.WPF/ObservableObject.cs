using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace CSharpUtils.WPF
{
    public class ObservableObject : INotifyPropertyChanged
    {
        protected PropertyChangedEventHandler PropertyChangedHandler => PropertyChanged;

        protected PropertyChangingEventHandler PropertyChangingHandler => PropertyChanging;

        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangingEventHandler PropertyChanging;

        [Conditional("DEBUG")]
        [DebuggerStepThrough]
        public void VerifyPropertyName(string propertyName)
        {
            Type myType = GetType();

            if (!string.IsNullOrEmpty(propertyName) && myType.GetProperty(propertyName) == null)
            {
                var descriptor = this as ICustomTypeDescriptor;

                if (descriptor != null)
                {
                    if (descriptor.GetProperties()
                       .Cast<PropertyDescriptor>()
                       .Any(property => property.Name == propertyName))
                        return;
                }

                throw new ArgumentException("Property not found", propertyName);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public virtual void RaisePropertyChanging(
            string propertyName)

        {
            VerifyPropertyName(propertyName);

            PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        public virtual void RaisePropertyChanged(
            string propertyName)

        {
            VerifyPropertyName(propertyName);

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        [SuppressMessage(
            "Microsoft.Design", "CA1006:GenericMethodsShouldProvideTypeParameter", Justification =
                "This syntax is more convenient than other alternatives.")]
        public virtual void RaisePropertyChanging<T>(Expression<Func<T>> propertyExpression)
        {
            PropertyChangingEventHandler handler = PropertyChanging;
            if (handler != null)
            {
                string propertyName = GetPropertyName(propertyExpression);
                handler(this, new PropertyChangingEventArgs(propertyName));
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate", Justification = "This cannot be an event")]
        [SuppressMessage(
            "Microsoft.Design", "CA1006:GenericMethodsShouldProvideTypeParameter", Justification =
                "This syntax is more convenient than other alternatives.")]
        public virtual void RaisePropertyChanged<T>(Expression<Func<T>> propertyExpression)
        {
            PropertyChangedEventHandler handler = PropertyChanged;

            if (handler != null)
            {
                string propertyName = GetPropertyName(propertyExpression);

                if (!string.IsNullOrEmpty(propertyName))
                    RaisePropertyChanged(propertyName);
            }
        }

        [SuppressMessage(
            "Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters", Justification =
                "This syntax is more convenient than the alternatives.")]
        [SuppressMessage(
            "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
                "This syntax is more convenient than the alternatives.")]
        protected static string GetPropertyName<T>(Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
                throw new ArgumentNullException(nameof(propertyExpression));

            var body = propertyExpression.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException("Invalid argument", nameof(propertyExpression));

            var property = body.Member as PropertyInfo;

            if (property == null)
                throw new ArgumentException("Argument is not a property", nameof(propertyExpression));

            return property.Name;
        }

        [SuppressMessage(
            "Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures", Justification =
                "This syntax is more convenient than the alternatives.")]
        [SuppressMessage(
            "Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#",
            Justification = "This syntax is more convenient than the alternatives.")]
        protected bool Set<T>(
            Expression<Func<T>> propertyExpression,
            ref T field,
            T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            RaisePropertyChanging(propertyExpression);
            field = newValue;
            RaisePropertyChanged(propertyExpression);
            return true;
        }

        [SuppressMessage(
            "Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "1#",
            Justification = "This syntax is more convenient than the alternatives.")]
        protected bool Set<T>(
            string propertyName,
            ref T field,
            T newValue)
        {
            if (EqualityComparer<T>.Default.Equals(field, newValue))
                return false;

            RaisePropertyChanging(propertyName);
            field = newValue;

            RaisePropertyChanged(propertyName);

            return true;
        }
    }
}