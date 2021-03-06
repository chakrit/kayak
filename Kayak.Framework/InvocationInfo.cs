﻿using System;
using System.Collections.Generic;
using System.Reflection;

namespace Kayak.Framework
{
    /// <summary>
    /// Defines a method invocation, including the target object, the `MethodInfo` to be invoked, and the arguments to the method.
    /// </summary>
    public sealed class InvocationInfo
    {
        public object Target;
        public MethodInfo Method;
        public object[] Arguments;
        public object Result;
        public Exception Exception;

        public void Invoke()
        {
            if (Method == null)
                throw new Exception("Invalid instance of InvocationInfo. Method was null.");
            if (Target == null)
                throw new Exception("Invalid instance of InvocationInfo. Target was null.");

            try
            {
                Result = Method.Invoke(Target, Arguments);
            }
            catch (Exception e)
            {
                Exception = e;

                if (Exception is TargetInvocationException)
                    Exception = Exception.InnerException;
            }
        }

        public override string ToString()
        {
            if (Method == null) return base.ToString();
            return Method.DeclaringType.Namespace + "." + Method.DeclaringType.Name + "." + Method.Name;
        }
    }

    public static partial class InvocationInfoExtensions
    {
        static string InvocationInfoContextKey = "InvocationInfo";

        public static InvocationInfo GetInvocationInfo(this IDictionary<string, object> context)
        {
            if (!context.ContainsKey(InvocationInfoContextKey)) return null;
            return context[InvocationInfoContextKey] as InvocationInfo;
        }

        internal static void SetInvocationInfo(this IDictionary<string, object> context, InvocationInfo info)
        {
            context[InvocationInfoContextKey] = info;
        }
    }
}
