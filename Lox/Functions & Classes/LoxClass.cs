using Lox.Functions;
using Lox.Interpreting;

namespace Lox
{
    internal sealed record LoxClass : LoxCallable
    {
        internal readonly string Name;
        internal readonly Dictionary<string, LoxFunction> Meathods = new();
        readonly LoxClass? Superclass;
        internal LoxClass(string name, LoxClass? superclass, Dictionary<string, LoxFunction> meathods)
        {
            Meathods = meathods;
            Name = name;
            Superclass = superclass;
            // Arity:
            var initializer = FindMethod("init");
            if (initializer is null)
            {
                Arity = 0;
            }
            else
            {
                Arity = initializer.Arity;
            }
        }

        public override int Arity { get; init; }

        public override object? FunctionCall(Interpreter interpreter, IList<object?> arguments)
        {
            LoxInstance instance = new(this);
            LoxFunction? initializer = FindMethod("init");
            initializer?.Bind(instance).FunctionCall(interpreter, arguments);
            return instance;
        }

        public override string ToString()
        {
            return $"<class {Name}>";
        }

        public LoxFunction? FindMethod(string name)
        {
            if (Meathods.TryGetValue(name, out LoxFunction? value))
            {
                return value;
            }

            if (Superclass is not null)
            {
                return Superclass.FindMethod(name);
            }
            return null;
        }
    }
}
