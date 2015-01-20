using DD4T.ViewModels.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;
using System.Reflection;
using DD4T.ViewModels.Attributes;
using DD4T.Mvc.Html;
using System.Web.Mvc;
using DD4T.ViewModels.Exceptions;
using DD4T.ViewModels.Reflection;
using DD4T.ViewModels;
using System.Web.Configuration;

namespace DD4T.ViewModels
{
    /// <summary>
    /// A static container class for default implementations of the View Model Framework
    /// </summary>
    public static class ViewModelDefaults
    {
        //Singletons
        private static readonly IViewModelKeyProvider keyProvider =
            new WebConfigViewModelKeyProvider("ViewModelKeyFieldName");
        private static readonly IComponentPresentationMocker mocker = new ComponentPresentationMocker(new CTMocker());
        private static readonly IViewModelBuilder viewModelBuilder = new ViewModelBuilder(keyProvider);
        /// <summary>
        /// Default View Model Builder. 
        /// <remarks>
        /// Set View Model Key Component Template Metadata field in Web config
        /// with key "ViewModelKeyFieldName". Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelBuilder Builder { get { return viewModelBuilder; } }
        /// <summary>
        /// Default Component Presentation Mocker
        /// </summary>
        public static IComponentPresentationMocker Mocker { get { return mocker; } }
        /// <summary>
        /// Default View Model Key Provider. 
        /// <remarks>
        /// Gets View Model Key from Component Template Metadata with field
        /// name specified in Web config App Settings wtih key "ViewModelKeyFieldName".
        /// Defaults to field name "viewModelKey".
        /// </remarks>
        /// </summary>
        public static IViewModelKeyProvider ViewModelKeyProvider { get { return keyProvider; } }
    }

    /// <summary>
    /// Base View Model Key Provider implementation with no external dependencies. Set protected 
    /// string ViewModelKeyField to CT Metadata Field name to use to retrieve View Model Keys.
    /// </summary>
    public abstract class ViewModelKeyProviderBase : IViewModelKeyProvider
    {
        protected string ViewModelKeyField = string.Empty;
        public string GetViewModelKey(IComponentTemplate template)
        {
            string result = null;
            if (template != null
                && template.MetadataFields != null
                && template.MetadataFields.ContainsKey(ViewModelKeyField))
            {
                result = template.MetadataFields[ViewModelKeyField].Value;
            }
            return result;
        }
    }
    /// <summary>
    /// Implementation of View Model Key Provider that uses the Web Config app settings 
    /// to retrieve the name of the Component Template Metadata field for the view model key.
    /// Default CT Metadata field name is "viewModelKey"
    /// </summary>
    public class WebConfigViewModelKeyProvider : ViewModelKeyProviderBase
    {
        public WebConfigViewModelKeyProvider(string webConfigKey)
        {
            ViewModelKeyField = WebConfigurationManager.AppSettings[webConfigKey];
            if (string.IsNullOrEmpty(ViewModelKeyField)) ViewModelKeyField = "viewModelKey"; //Default value
        }
    }
}
