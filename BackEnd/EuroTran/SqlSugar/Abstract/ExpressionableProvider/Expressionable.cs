using System;
using System.Linq.Expressions;

namespace SqlSugar
{
    public class Expressionable<T> where T : class, new()
    {
        private Expression<Func<T, bool>> _exp = null;

        public Expressionable<T> And(Expression<Func<T, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, bool>>(Expression.AndAlso(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T> AndIF(bool isAnd, Expression<Func<T, bool>> exp)
        {
            if (isAnd)
                And(exp);
            return this;
        }

        public Expressionable<T> Or(Expression<Func<T, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, bool>>(Expression.OrElse(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T> OrIF(bool isOr, Expression<Func<T, bool>> exp)
        {
            if (isOr)
                Or(exp);
            return this;
        }

        public Expression<Func<T, bool>> ToExpression()
        {
            if (_exp == null)
                _exp = it => true;
            return _exp;
        }
    }

    public class Expressionable<T, T2> where T : class, new() where T2 : class, new()
    {
        private Expression<Func<T, T2, bool>> _exp = null;

        public Expressionable<T, T2> And(Expression<Func<T, T2, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, T2, bool>>(Expression.AndAlso(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T, T2> AndIF(bool isAnd, Expression<Func<T, T2, bool>> exp)
        {
            if (isAnd)
                And(exp);
            return this;
        }

        public Expressionable<T, T2> Or(Expression<Func<T, T2, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, T2, bool>>(Expression.OrElse(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T, T2> OrIF(bool isOr, Expression<Func<T, T2, bool>> exp)
        {
            if (isOr)
                Or(exp);
            return this;
        }

        public Expression<Func<T, T2, bool>> ToExpression()
        {
            if (_exp == null)
                _exp = (it, t2) => true;
            return _exp;
        }
    }

    public class Expressionable<T, T2, T3> where T : class, new() where T2 : class, new() where T3 : class, new()
    {
        private Expression<Func<T, T2, T3, bool>> _exp = null;

        public Expressionable<T, T2, T3> And(Expression<Func<T, T2, T3, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, T2, T3, bool>>(Expression.AndAlso(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T, T2, T3> AndIF(bool isAnd, Expression<Func<T, T2, T3, bool>> exp)
        {
            if (isAnd)
                And(exp);
            return this;
        }

        public Expressionable<T, T2, T3> Or(Expression<Func<T, T2, T3, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, T2, T3, bool>>(Expression.OrElse(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T, T2, T3> OrIF(bool isOr, Expression<Func<T, T2, T3, bool>> exp)
        {
            if (isOr)
                Or(exp);
            return this;
        }

        public Expression<Func<T, T2, T3, bool>> ToExpression()
        {
            if (_exp == null)
                _exp = (it, t2, t3) => true;
            return _exp;
        }
    }

    public class Expressionable<T, T2, T3, T4> where T : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new()
    {
        private Expression<Func<T, T2, T3, T4, bool>> _exp = null;

        public Expressionable<T, T2, T3, T4> And(Expression<Func<T, T2, T3, T4, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, T2, T3, T4, bool>>(Expression.AndAlso(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T, T2, T3, T4> AndIF(bool isAnd, Expression<Func<T, T2, T3, T4, bool>> exp)
        {
            if (isAnd)
                And(exp);
            return this;
        }

        public Expressionable<T, T2, T3, T4> Or(Expression<Func<T, T2, T3, T4, bool>> exp)
        {
            _exp = _exp == null ? exp : Expression.Lambda<Func<T, T2, T3, T4, bool>>(Expression.OrElse(_exp.Body, exp.Body), _exp.Parameters);
            return this;
        }

        public Expressionable<T, T2, T3, T4> OrIF(bool isOr, Expression<Func<T, T2, T3, T4, bool>> exp)
        {
            if (isOr)
                Or(exp);
            return this;
        }

        public Expression<Func<T, T2, T3, T4, bool>> ToExpression()
        {
            if (_exp == null)
                _exp = (it, t2, t3, t4) => true;
            return _exp;
        }
    }

    public class Expressionable
    {
        public static Expressionable<T> Create<T>() where T : class, new()
        {
            return new Expressionable<T>();
        }

        public static Expressionable<T, T2> Create<T, T2>() where T : class, new() where T2 : class, new()
        {
            return new Expressionable<T, T2>();
        }

        public static Expressionable<T, T2, T3> Create<T, T2, T3>() where T : class, new() where T2 : class, new() where T3 : class, new()
        {
            return new Expressionable<T, T2, T3>();
        }

        public static Expressionable<T, T2, T3, T4> Create<T, T2, T3, T4>() where T : class, new() where T2 : class, new() where T3 : class, new() where T4 : class, new()
        {
            return new Expressionable<T, T2, T3, T4>();
        }
    }
}