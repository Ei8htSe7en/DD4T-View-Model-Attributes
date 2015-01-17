using DD4T.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }
    /// <summary>
    /// Data structure for uniquely identifying View Models
    /// </summary>
    internal struct ViewModelKey
    {
        private string schemaName;
        private string componentTemplateName;
        public string SchemaName { get { return schemaName; } }
        public string ComponentTemplateName { get { return componentTemplateName; } }
        
        public ViewModelKey(string schemaName, string componentTemplateName)
        {
            this.schemaName = schemaName;
            this.componentTemplateName = componentTemplateName;
        }
        public override int GetHashCode()
        {
            //Does the hash code ever change? can we store this in the constructor?
            return (schemaName + componentTemplateName).GetHashCode();
        }
        public override bool Equals(object obj)
        {
            if (obj != null && obj is ViewModelKey)
            {
                ViewModelKey key = (ViewModelKey)obj;
                if (this.ComponentTemplateName == null || key.ComponentTemplateName == null)
                {
                    //if either one doesn't have a CT set, just compare Schemas
                    return this.SchemaName == key.SchemaName;
                }
                else if (this.ComponentTemplateName != null && key.ComponentTemplateName != null)
                {
                    //if both have a CT set, use both CT and schema
                    return this.SchemaName == key.SchemaName && this.ComponentTemplateName == key.ComponentTemplateName;
                }
            }
            return false;
        }
    }

}
