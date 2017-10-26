using System;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Models.Extensions {
    public static class TypeExtensions {

        /// <summary>
        /// Determines whether the given type is anonymous or not.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns><see langword="true"/> if type is anonymous, <see langword="false"/> otherwise</returns>
        public static bool IsAnonymousType(this Type type) {
            if (type == null) {
                return false;
            }

            return type.GetTypeInfo().IsGenericType
                   && (type.GetTypeInfo().Attributes & TypeAttributes.NotPublic) == TypeAttributes.NotPublic
                   && (type.Name.StartsWith("<>", StringComparison.OrdinalIgnoreCase) || type.Name.StartsWith("VB$", StringComparison.OrdinalIgnoreCase))
                   && (type.Name.Contains("AnonymousType") || type.Name.Contains("AnonType"))
                   && type.GetTypeInfo().GetCustomAttributes(typeof(CompilerGeneratedAttribute)).Any();
        }
    }
}