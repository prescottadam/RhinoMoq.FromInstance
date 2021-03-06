﻿using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RhinoMoq.FromInstance
{
    /// <summary>
    /// Wrapper for a <see cref="Delegate"/> that does not contain any 
    /// <see cref="MethodBase.GetParameters"/>
    /// </summary>
    /// <remarks>
    /// RhinoMocks expects (<c>AbstractExpectation.AssertDelegateArgumentsMatchMethod</c>)
    /// expects that the<see cref="Delegate"/> passed into <c>MethodOptions{T}.Do</c>
    /// has the same number of <see cref="MethodBase.GetParameters"/> as the actual
    /// method / property we are mocking.  
    /// 
    /// However, a <see cref="LambdaExpression.Compile()"/> returns a <see cref="Delegate"/>
    /// that has a hidden Parameter, which will throw off this check: 
    /// http://stackoverflow.com/questions/7935306/compiling-a-lambda-expression-results-in-delegate-with-closure-argument 
    /// 
    /// This class creates a wrapper around <see cref="LambdaExpression.Compile()"/> that
    /// (<c>AbstractExpectation.AssertDelegateArgumentsMatchMethod</c>) is happy with.
    ///</remarks>
    /// <remarks>
    /// Moq doesn't seem to care either way, so there's no harm using this with Moq.
    /// </remarks>
    internal abstract class DelegateWrapper
    {
        internal abstract Delegate InvokeDelegate { get; }

        public static Delegate Create(Delegate originalDelegate)
        {
            Type returnType = originalDelegate.Method.ReturnType;
            var wrapperType = typeof(DelegateWrapper<>).MakeGenericType(returnType);
            var wrapper = Activator.CreateInstance(wrapperType, originalDelegate);
            return ((DelegateWrapper)wrapper).InvokeDelegate;
        }
    }

    internal sealed class DelegateWrapper<T> : DelegateWrapper
    {
        private readonly Delegate _originalDelegate;

        public DelegateWrapper(Delegate originalDelegate)
        {
            _originalDelegate = originalDelegate;
        }

        private T Invoke()
        {
            return (T)_originalDelegate.DynamicInvoke();
        }

        internal override Delegate InvokeDelegate
        {
            get
            {
                return new Func<T>(Invoke);
            }
        }
    }
}
