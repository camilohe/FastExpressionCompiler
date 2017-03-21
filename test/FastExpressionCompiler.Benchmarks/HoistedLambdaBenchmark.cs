using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using BenchmarkDotNet.Attributes;

namespace FastExpressionCompiler.Benchmarks
{
    public class HoistedLambdaBenchmark
    {
        private static Expression<Func<X>> GetHoistedExpr()
        {
            var a = new A();
            var b = new B();
            Expression<Func<X>> e = () => new X(a, b);
            return e;
        }

        private static readonly Expression<Func<X>> _hoistedExpr = GetHoistedExpr();

        private readonly Expression<Func<object>> _manualExpr = GetManualExpr();

        private static Expression<Func<object>> GetManualExpr()
        {
            var a = new A();
            var b = new B();
            var e = Expression.Lambda<Func<object>>(
                Expression.New(typeof(X).GetTypeInfo().DeclaredConstructors.First(),
                Expression.Constant(a, typeof(A)),
                Expression.Constant(b, typeof(B))));
            return e;
        }

        public class Compile
        {
            [Benchmark]
            public Func<X> Expr()
            {
                return _hoistedExpr.Compile();
            }

            [Benchmark]
            public Func<X> Fast()
            {
                return ExpressionCompiler.Compile(_hoistedExpr);
            }
        }

        public class Invoke
        {
            private static readonly Func<X> _lambdaCompiled = _hoistedExpr.Compile();
            private static readonly Func<X> _lambdaCompiledFast = ExpressionCompiler.Compile(_hoistedExpr);

            A aa = new A();
            B bb = new B();

            [Benchmark(Baseline = true)]
            public X ConstructorDirectly()
            {
                return new X(aa, bb);
            }

            [Benchmark]
            public X CompiledLambda()
            {
                return _lambdaCompiled();
            }

            [Benchmark]
            public X FastCompiledLambda()
            {
                return _lambdaCompiledFast();
            }
        }
    }
}