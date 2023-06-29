using Lox.Tokens;

namespace Lox
{
    internal sealed class LoxInstance
    {
        private readonly LoxClass Class;
        private readonly Dictionary<string, object?> fields;

        internal LoxInstance(LoxClass @class)
        {
            Class = @class;
            fields = new();
        }

        public override string ToString()
        {
            return $"<class {Class.Name} instance>";
        }

        public object? this[Token name]
        {
            get
            {
                if (fields.TryGetValue(name.Lexeme, out var value))
                {
                    return value;
                }

                var meathod =  Class.FindMethod(name.Lexeme);
                if (meathod is not null)
                {
                    return meathod.Bind(this);
                }
                throw new RuntimeException(name, "Method not found.");
            }
            set => fields[name.Lexeme] = value;
        }
    }
}
