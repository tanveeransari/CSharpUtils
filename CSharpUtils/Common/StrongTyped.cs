using System;

namespace CSharpUtils.Common
{
    public class StrongTyped<T> : IEquatable<StrongTyped<T>>
    {
        public StrongTyped(T value)
        {
            Value = value;
        }

        public T Value { get; protected set; }

        public static bool operator !=(StrongTyped<T> left, StrongTyped<T> right)
        {
            return !Equals(left, right);
        }

        public static bool operator ==(StrongTyped<T> left, StrongTyped<T> right)
        {
            return Equals(left, right);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public bool Equals(StrongTyped<T> other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.Value, Value);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}