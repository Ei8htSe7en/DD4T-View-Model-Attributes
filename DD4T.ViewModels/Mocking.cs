using DD4T.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DD4T.ViewModels.Mocking
{
    public static class MockHelper
    {
        public static void AddXpathToFields(IFieldSet fieldSet, string baseXpath)
        {
            // add XPath properties to all fields
            try
            {
                foreach (Field f in fieldSet.Values)
                {
                    f.XPath = string.Format("{0}/custom:{1}", baseXpath, f.Name);
                    int i = 1;
                    if (f.EmbeddedValues != null)
                    {
                        foreach (FieldSet subFields in f.EmbeddedValues)
                        {
                            AddXpathToFields(subFields, string.Format("{0}/custom:{1}[{2}]", baseXpath, f.Name, i++));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
