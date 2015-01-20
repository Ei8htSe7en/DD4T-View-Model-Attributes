using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ViewModels.Exceptions
{
    public class ViewModelTypeNotFoundExpception : Exception
    {
        public ViewModelTypeNotFoundExpception(string schemaName, string viewModelKey)
            : base(String.Format("Could not find view model for schema {0} and ID {1} in loaded assemblies."
                    , schemaName, viewModelKey)) 
        { }
    }

    public class FieldTypeMismatchException : Exception
    {
        public FieldTypeMismatchException(string message) : base(message) { }
    }
}
