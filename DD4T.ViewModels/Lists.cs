using DD4T.ViewModels.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ViewModels.Lists
{
    public class ViewModelList<T> : List<IDD4TViewModel>, IEnumerable<T> where T : IDD4TViewModel
    {
        public new IEnumerator<T> GetEnumerator()
        {
            return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T
        }
    }

    //public class EmbeddedViewModelList<T> : List<IEmbeddedSchemaViewModel>, IEnumerable<T> where T : IEmbeddedSchemaViewModel
    //{
    //    public new IEnumerator<T> GetEnumerator()
    //    {
    //        return this.ToArray().Cast<T>().GetEnumerator(); //Assuming all the objects added to this implement T
    //    }
    //}
}
