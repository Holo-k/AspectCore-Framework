﻿using AspectCore.Lite.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AspectCore.Lite.Internal
{
    internal sealed class DefaultAspectBuilder : IAspectBuilder
    {
        private readonly IList<Func<AspectDelegate, AspectDelegate>> delegates;

        public DefaultAspectBuilder()
        {
            delegates = new List<Func<AspectDelegate, AspectDelegate>>();
        }

        public void AddAspectDelegate(Func<AspectContext, AspectDelegate, Task> interceptorInvoke)
        {
            if (interceptorInvoke == null)
            {
                throw new ArgumentNullException(nameof(interceptorInvoke));
            }
            delegates.Add(next => context => interceptorInvoke(context, next));
        }

        public AspectDelegate Build(Func<object> targetInvoke)
        {
            if (targetInvoke == null)
            {
                throw new ArgumentNullException(nameof(targetInvoke));
            }
            AspectDelegate invoke = context =>
            {
                context.ReturnParameter.Value = targetInvoke();
                return Task.FromResult(0);
            };
            foreach (var next in delegates.Reverse())
            {
                invoke = next(invoke);
            }
            return invoke;
        }
    }
}