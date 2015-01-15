using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ViewModels.Exceptions
{
    public class ViewModelTypeNotFoundExpception : Exception
    {
        public ViewModelTypeNotFoundExpception(string message) : base(message) { }
    }

    public class FieldTypeMismatchException : Exception
    {
        public FieldTypeMismatchException(string message) : base(message) { }
    }
}
