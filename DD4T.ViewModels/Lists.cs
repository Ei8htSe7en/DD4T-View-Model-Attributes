using DD4T.ViewModels.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ViewModels.Lists
{
    public class ComponentViewModelList<T> : List<IComponentPresentationViewModel>, IEnumerable<T> where T : IComponentPresentationViewModel
    {
        public new IEnumerator<T> GetEnumerator()
        {
            return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T
        }
    }
    public class EmbeddedViewModelList<T> : List<IEmbeddedSchemaViewModel>, IEnumerable<T> where T : IEmbeddedSchemaViewModel
    {
        public new IEnumerator<T> GetEnumerator()
        {
            return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T
        }
    }
}
