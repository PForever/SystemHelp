using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemHelp
{
    internal class UnexpectedTypeException : Exception
    {
        private readonly Type[] _expectedTypes;
        private readonly StringBuilder _sb = new StringBuilder();
        private const string Sep = ", ";
        private string _expectedTypesNames
        {
            get
            {
                int prelen = _expectedTypes.Length - 1;
                for (var i = 0; i < prelen; i++)
                {
                    _sb.Append(_expectedTypes[i].Name).Append(Sep);
                }
                _sb.Append(_expectedTypes[prelen].Name);
                return _sb.ToString();
            }
        }
        private Type _unexpectedType;
        public object Target { get; }
        private string _message;
        public override string Message => _message ?? (_message = Target == null ?
                                              $"Unexpected type {_unexpectedType.Name} of object {Target}. Expected type like {_expectedTypesNames}" :
                                              $"Unexpected null object. Expected type like {_expectedTypesNames}");

        public UnexpectedTypeException(object target, params Type[] expectedTypes)
        {
            _expectedTypes = expectedTypes;
            _unexpectedType = target?.GetType();
            Target = target;
        }
    }
}
