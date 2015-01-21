using DD4T.ContentModel;
using DD4T.ViewModels.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace DD4T.ViewModels.Contracts
{
    /// <summary>
    /// A DD4T View Model
    /// </summary>
    public interface IDD4TViewModel
    {
        IViewModelBuilder Builder { get; set; }
        IFieldSet Fields { get; set; }
        IFieldSet MetadataFields { get; }
        /// <summary>
        /// Publication ID of the underlying Tridion item
        /// </summary>
        int PublicationId { get; }
    }


    public interface IComponentPresentationViewModel : IDD4TViewModel
    {
        IComponentPresentation ComponentPresentation { get; set; }
    }
    //TODO: Consider removing this interface, holding on to template is not actually necessary after building is done
    public interface IEmbeddedSchemaViewModel : IDD4TViewModel
    {
        IComponentTemplate ComponentTemplate { get; set; } //we required Component Template here in order to generate Site Edit markup for any linked components in the embedded fields
    }

    public interface IViewModelBuilder
    {
        /// <summary>
        /// Builds a View Model from previously loaded Assemblies using the input Component Presentation. This method infers the View Model type by comparing
        /// information from the Component Presentation object to the attributes of the View Model classes.
        /// </summary>
        /// <param name="cp">Component Presentation object</param>
        /// <remarks>
        /// The LoadViewModels method must be called with the desired View Model Types in order for this to return a valid object.
        /// </remarks>
        /// <returns>Component Presentation View Model</returns>
        IComponentPresentationViewModel BuildCPViewModel(IComponentPresentation cp); //A way to build view model without passing type -- type is inferred using loaded assemblies
        /// <summary>
        /// Builds a View Model from a FieldSet using the schema determine the View Model class to use.
        /// </summary>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="embeddedSchema">Embedded Schema</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        IEmbeddedSchemaViewModel BuildEmbeddedViewModel(IFieldSet embeddedFields, ISchema embeddedSchema, IComponentTemplate template);
        /// <summary>
        /// Builds a View Model from a Component Presentation using the type parameter to determine the View Model class to use.
        /// </summary>
        /// <param name="type">Type of View Model class to return</param>
        /// <param name="cp">Component Presentation</param>
        /// <returns>Component Presentation View Model</returns>
        IComponentPresentationViewModel BuildCPViewModel(Type type, IComponentPresentation cp);
        /// <summary>
        /// Builds a View Model from a FieldSet using the generic type to determine the View Model class to use.
        /// </summary>
        /// <param name="type">Type of View Model class to return</param>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        IEmbeddedSchemaViewModel BuildEmbeddedViewModel(Type type, IFieldSet embeddedFields, IComponentTemplate template);
        /// <summary>
        /// Builds a View Model from a Component Presentation using the generic type to determine the View Model class to use.
        /// </summary>
        /// <typeparam name="T">Type of View Model class to return</typeparam>
        /// <param name="cp">Component Presentation</param>
        /// <returns>Component Presentation View Model</returns>
        T BuildCPViewModel<T>(IComponentPresentation cp) where T : class, IComponentPresentationViewModel;
        /// <summary>
        /// Builds a View Model from a FieldSet using the generic type to determine the View Model class to use.
        /// </summary>
        /// <typeparam name="T">Type of View Model class to return</typeparam>
        /// <param name="embeddedFields">Embedded FieldSet</param>
        /// <param name="template">Component Template to use for generating XPM Markup for any linked components</param>
        /// <returns>Embedded Schema View Model</returns>
        T BuildEmbeddedViewModel<T>(IFieldSet embeddedFields, IComponentTemplate template) where T : class, IEmbeddedSchemaViewModel;
        /// <summary>
        /// The View Model Key Provider for this Builder
        /// </summary>
        IViewModelKeyProvider ViewModelKeyProvider { get; }
        /// <summary>
        /// Loads all View Model classes from an assembly
        /// </summary>
        /// <param name="assembly">The Assembly with the view model Types to load</param>
        /// <remarks>
        /// Required for use of builder methods that don't require a Type parameter or generic.
        /// The Builder will only use Types tagged with the ViewModelAttribute class.
        /// </remarks>
        void LoadViewModels(Assembly assembly);
    }

    public interface IComponentPresentationMocker
    {
        /// <summary>
        /// Converts a DD4T View Model to a Field Set object
        /// </summary>
        /// <remarks>Primarily used for mocking purposes.</remarks>
        /// <param name="viewModel">A View Model object</param>
        /// <param name="schemaName">The embedded schema name</param>
        /// <returns>Field Set</returns>
        IFieldSet ConvertToFieldSet(IDD4TViewModel viewModel, out string schemaName);
        /// <summary>
        /// Converts a DD4T View Model to a Component Presentation object
        /// </summary>
        /// <remarks>Primarily used for mocking purposes.</remarks>
        /// <param name="viewModel">A View Model object</param>
        /// <returns>Component Presentation</returns>
        IComponentPresentation ConvertToComponentPresentation(IDD4TViewModel viewModel);
        /// <summary>
        /// Adds XPaths to all fields for using Site Edit/XPM
        /// </summary>
        /// <param name="fieldSet">A Field Set</param>
        /// <param name="baseXpath">The base XPath</param>
        void AddXpathToFields(IFieldSet fieldSet, string baseXpath);
        string GetXmlRootName(IDD4TViewModel viewModel);
        DateTime GetLastPublishedDate(IComponentPresentationViewModel viewModel);
    }

    public interface IXpmMarkupService
    {
        /// <summary>
        /// Renders the XPM Markup for a field
        /// </summary>
        /// <param name="field">DD4T Field</param>
        /// <param name="index">Optional index for multi value fields</param>
        /// <returns>XPM Markup</returns>
        string RenderXpmMarkupForField(IField field, int index = -1);
        /// <summary>
        /// Renders the XPM Markup for a Component Presentation
        /// </summary>
        /// <param name="cp">Component Presentation</param>
        /// <param name="region">Optional region</param>
        /// <returns>XPM Markup</returns>
        string RenderXpmMarkupForComponent(IComponentPresentation cp, string region = null);
        /// <summary>
        /// Determines if Site Edit is enabled for a particular item
        /// </summary>
        /// <param name="item">Item</param>
        /// <returns></returns>
        bool IsSiteEditEnabled(IRepositoryLocal item);
        /// <summary>
        /// Determines if Site Edit is enabeld for a publication
        /// </summary>
        /// <param name="publicationId">Publication ID</param>
        /// <returns></returns>
        bool IsSiteEditEnabled(int publicationId);
    }

    //TODO: Instantiate and add these to the IDD4TViewModel using System.Reflection's MakeGenericType
    public interface IXpmRenderer<TModel> where TModel : IDD4TViewModel
    {
        /// <summary>
        /// Renders both XPM Markup and Field Value 
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="index">Optional index for a multi-value field</param>
        /// <returns>XPM Markup and field value</returns>
        MvcHtmlString XpmEditableField<TProp>(Expression<Func<TModel, TProp>> propertyLambda, int index = -1);
        /// <summary>
        /// Renders both XPM Markup and Field Value for a multi-value field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <typeparam name="TItem">Item type - this must match the generic type of the property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="item">The particular value of the multi-value field</param>
        /// <example>
        /// foreach (var content in model.Content)
        /// {
        ///     @model.XpmEditableField(m => m.Content, content);
        /// }
        /// </example>
        /// <returns>XPM Markup and field value</returns>
        MvcHtmlString XpmEditableField<TProp, TItem>(Expression<Func<TModel, TProp>> propertyLambda, TItem item);
        /// <summary>
        /// Renders the XPM markup for a field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="index">Optional index for a multi-value field</param>
        /// <returns>XPM Markup</returns>
        MvcHtmlString XpmMarkupFor<TProp>(Expression<Func<TModel, TProp>> propertyLambda, int index = -1);
        /// <summary>
        /// Renders XPM Markup for a multi-value field
        /// </summary>
        /// <typeparam name="TModel">Model type</typeparam>
        /// <typeparam name="TProp">Property type</typeparam>
        /// <typeparam name="TItem">Item type - this must match the generic type of the property type</typeparam>
        /// <param name="model">Model</param>
        /// <param name="propertyLambda">Lambda expression representing the property to render. This must be a direct property of the model.</param>
        /// <param name="item">The particular value of the multi-value field</param>
        /// <example>
        /// foreach (var content in model.Content)
        /// {
        ///     @model.XpmMarkupFor(m => m.Content, content);
        ///     @content;
        /// }
        /// </example>
        /// <returns>XPM Markup</returns>
        MvcHtmlString XpmMarkupFor<TProp, TItem>(Expression<Func<TModel, TProp>> propertyLambda, TItem item);
        /// <summary>
        /// Renders the XPM Markup for a Component Presentation
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="region">Region</param>
        /// <returns>XPM Markup</returns>
        MvcHtmlString StartXpmEditingZone(string region = null);
    }

    /// <summary>
    /// Provides the View Model Key using a Component Template
    /// </summary>
    public interface IViewModelKeyProvider
    {
        /// <summary>
        /// Retrieves a View Model Key based on a Component Template. Should return the same key for the same template every time.
        /// Return values of null or empty string will be ignored.
        /// </summary>
        /// <param name="template">Component Template</param>
        /// <returns>View Model Key</returns>
        string GetViewModelKey(IComponentTemplate template);
    }

    public interface ICTMocker
    {
        IComponentTemplate GetComponentTemplate(ViewModelAttribute viewModelAttribute);
    }

    public interface ICanBeBoolean
    {
        bool IsBooleanValue { get; set; }
    }
    /// <summary>
    /// Base class for all Component Presentation View Models
    /// </summary>
    public abstract class ComponentPresentationViewModelBase : IComponentPresentationViewModel
    {
        private IFieldSet fields = null;
        private IFieldSet metadataFields = null;
        public IComponentPresentation ComponentPresentation
        {
            get;
            set;
        }
        public IViewModelBuilder Builder
        {
            get;
            set;
        }
        public IFieldSet Fields
        {
            get
            {
                if (fields == null && ComponentPresentation != null)
                {
                    fields = ComponentPresentation.Component.Fields;
                }
                return fields;
            }
            set
            {
                fields = value;
            }
        }
        public IFieldSet MetadataFields
        {
            get
            {
                if (metadataFields == null && ComponentPresentation != null)
                {
                    metadataFields = ComponentPresentation.Component.MetadataFields;
                }
                return metadataFields;
            }
        }
        public int PublicationId
        {
            get
            {
                try
                {
                    return new TcmUri(ComponentPresentation.Component.Id).PublicationId;
                }
                catch
                {
                    return 0; //TODO: Get pub id
                }
            }
        }
    }
    /// <summary>
    /// Base class for all Embedded Schema View Models
    /// </summary>
    public abstract class EmbeddedSchemaViewModelBase : IEmbeddedSchemaViewModel
    {
        public IComponentTemplate ComponentTemplate
        {
            get;
            set;
        }
        public IViewModelBuilder Builder
        {
            get;
            set;
        }
        public IFieldSet Fields
        {
            get;
            set;
        }
        public IFieldSet MetadataFields
        {
            get
            {
                return null;
            }
        }
        public int PublicationId
        {
            get
            {
                try
                {
                    return new TcmUri(ComponentTemplate.Id).PublicationId;
                }
                catch
                {
                    return 0; //TODO: Get pub id
                }
            }
        }
    }
    [Obsolete]
    /// <summary>
    /// Data structure for uniquely identifying View Models
    /// </summary>
    internal struct ViewModelEntry
    {
        public string SchemaName { get; set; }
        public string[] ViewModelIds { get; set; }
        public bool IsDefault { get; set; }
        public ViewModelAttribute ViewModelAttribute { get; set; }
        public override int GetHashCode()
        {
            //Does the hash code ever change? can we store this in the constructor?
            string viewModelKey = ViewModelIds == null || ViewModelIds.Length < 1 ? string.Empty : ViewModelIds.FirstOrDefault();
            return (SchemaName + viewModelKey).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj != null && obj is ViewModelEntry)
            {
                ViewModelEntry key = (ViewModelEntry)obj;
                if (this.ViewModelIds != null && key.ViewModelIds != null)
                {
                    //if both have a ViewModelKey set, use both ViewModelKey and schema
                    //Check for a match anywhere in both lists
                    var match = from i in this.ViewModelIds
                                join j in key.ViewModelIds
                                on i equals j
                                select i;
                    //Schema names match and there is a matching view model ID
                    return this.SchemaName == key.SchemaName && match.Count() > 0;
                }
                else if (((this.ViewModelIds == null || this.ViewModelIds.Length == 0) && key.IsDefault) //this set of IDs is empty and the input is default
                    || ((key.ViewModelIds == null || key.ViewModelIds.Length == 0) && this.IsDefault)) //input set of IDs is empty and this is default
                {
                    //if either one doesn't have a ViewModelIds and the other is a default, just compare Schemas
                    return this.SchemaName == key.SchemaName;
                }
            }
            return false;
        }
    }

}
