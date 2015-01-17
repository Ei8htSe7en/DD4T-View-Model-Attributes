using DD4T.ContentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web.Mvc;

namespace DD4T.ViewModels.Contracts
{
    public interface IDD4TViewModel
    {
        IViewModelBuilder Builder { get; set; }
    }

    public interface IComponentPresentationViewModel : IDD4TViewModel
    {
        IComponentPresentation ComponentPresentation { get; set; }
    }
    public interface IEmbeddedSchemaViewModel : IDD4TViewModel
    {
        IFieldSet EmbeddedFields { get; set; }
        IComponentTemplate ComponentTemplate { get; set; } //we required Component Template here in order to generate Site Edit markup for any linked components in the embedded fields
    }

    public interface IViewModelBuilder
    {
        IComponentPresentationViewModel BuildCPViewModel(IComponentPresentation cp); //A way to build view model without passing type -- type is inferred using loaded assemblies
        IEmbeddedSchemaViewModel BuildEmbeddedViewModel(IFieldSet embeddedFields, ISchema schema, IComponentTemplate template);
        IComponentPresentationViewModel BuildCPViewModel(Type type, IComponentPresentation cp);
        IEmbeddedSchemaViewModel BuildEmbeddedViewModel(Type type, IFieldSet embeddedFields, IComponentTemplate template);
        T BuildCPViewModel<T>(IComponentPresentation cp) where T : class, IComponentPresentationViewModel;
        T BuildEmbeddedViewModel<T>(IFieldSet embeddedFields, IComponentTemplate template) where T : class, IEmbeddedSchemaViewModel;
        IFieldSet ConvertToFieldSet(IEmbeddedSchemaViewModel viewModel, out string schemaName);
        IComponentPresentation ConvertToComponentPresentation(IComponentPresentationViewModel viewModel);
        /// <summary>
        /// Loads all view model class types into the builder
        /// </summary>
        /// <param name="assembly"></param>
        void LoadViewModels(Assembly assembly);
    }

    public interface IComponentPresentationMocker
    {
        IFieldSet ConvertToFieldSet(IEmbeddedSchemaViewModel viewModel, out string schemaName);
        IComponentPresentation ConvertToComponentPresentation(IComponentPresentationViewModel viewModel);
        void AddXpathToFields(IFieldSet fieldSet, string baseXpath);
        string GetXmlRootName(IComponentPresentationViewModel viewModel);
        string GetXmlRootName(IEmbeddedSchemaViewModel viewModel);
        DateTime GetLastPublishedDate(IComponentPresentationViewModel viewModel);

    }


    public interface ICanBeBoolean
    {
        bool IsBooleanValue { get; set; }
    }
    public abstract class ComponentPresentationViewModelBase : IComponentPresentationViewModel
    {
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
    }

    public abstract class EmbeddedSchemaViewModelBase : IEmbeddedSchemaViewModel
    {
        public IFieldSet EmbeddedFields
        {
            get;
            set;
        }

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
    }
    //Find a better namespace for this
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
