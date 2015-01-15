using DD4T.ViewModels.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ContentModel;
using System.Reflection;
using DD4T.ViewModels.Attributes;
using DD4T.Utils;
using DD4T.Mvc.Html;
using System.Web.Mvc;
using DD4T.ViewModels.Exceptions;

namespace DD4T.ViewModels.Builders
{

    public static class ViewModelCore
    {
        private static IViewModelBuilder viewModelBuilder = new ViewModelBuilder(); //singleton
        public static IViewModelBuilder Builder { get { return viewModelBuilder; } }
    }


    /// <summary>
    /// Implementation of View Model Builder. Use as a singleton.
    /// </summary>
    internal class ViewModelBuilder : IViewModelBuilder
    {
        private IDictionary<ViewModelKey, Type> viewModels = new Dictionary<ViewModelKey, Type>();
        private IList<Assembly> loadedAssemblies = new List<Assembly>();

        internal ViewModelBuilder() { } //only provide an internal constructor, this class can never be instantiated elsewhere

        #region IViewModelBuilder
        public void LoadViewModels(Assembly assembly) //A method like this assumes we have a singleton of this instance
        {
            if (!loadedAssemblies.Contains(assembly))
            {
                loadedAssemblies.Add(assembly);
                ViewModelAttribute viewModelAttr;
                ViewModelKey key;
                foreach (var type in assembly.GetTypes())
                {
                    viewModelAttr = ReflectionCache.GetViewModelAttribute(type);
                    if (viewModelAttr != null)
                    {
                        key = new ViewModelKey(viewModelAttr.SchemaName, viewModelAttr.ComponentTemplateName);
                        if (!viewModels.ContainsKey(key)) viewModels.Add(key, type);
                    }
                }
            }
        }
        public IComponentPresentationViewModel BuildCPViewModel(Type type, IComponentPresentation cp)
        {
            IComponentPresentationViewModel viewModel = null;
            viewModel = (IComponentPresentationViewModel)ReflectionCache.CreateInstance(type);
            viewModel.ComponentPresentation = cp;
            viewModel.Builder = this;
            IFieldSet fields = cp.Component.Fields;
            ProcessFields(fields, viewModel, type, cp.ComponentTemplate, cp.Component.MetadataFields);
            return viewModel;
        }
        public T BuildCPViewModel<T>(IComponentPresentation cp) where T : class, IComponentPresentationViewModel
        {
            Type type = typeof(T);
            return (T)BuildCPViewModel(type, cp);
        }
        public IComponentPresentationViewModel BuildCPViewModel(IComponentPresentation cp)
        {
            if (cp == null) throw new ArgumentNullException("cp");
            var key = new ViewModelKey(cp.Component.Schema.Title, cp.ComponentTemplate.Title);
            IComponentPresentationViewModel result = null;
            if (viewModels.ContainsKey(key))
            {
                Type type = viewModels[key];
                result = (IComponentPresentationViewModel)BuildCPViewModel(type, cp);
            }
            else
            {
                throw new ViewModelTypeNotFoundExpception(
                    String.Format("Could not find view model for schema {0} and component template {1} in loaded assemblies."
                    , key.SchemaName, key.ComponentTemplateName));
            }
            return result;
        }
        public T BuildEmbeddedViewModel<T>(IFieldSet embeddedFields, IComponentTemplate template) where T : class, IEmbeddedSchemaViewModel
        {
            Type type = typeof(T);
            return (T)BuildEmbeddedViewModel(type, embeddedFields, template);
        }
        public IEmbeddedSchemaViewModel BuildEmbeddedViewModel(IFieldSet embeddedFields, ISchema schema, IComponentTemplate template)
        {
            if (embeddedFields == null) throw new ArgumentNullException("embeddedFields");
            if (schema == null) throw new ArgumentNullException("schema");
            if (template == null) throw new ArgumentNullException("template");
            var key = new ViewModelKey(schema.Title, template.Title);
            IEmbeddedSchemaViewModel result = null;
            if (viewModels.ContainsKey(key))
            {
                Type type = viewModels[key];
                result = (IEmbeddedSchemaViewModel)BuildEmbeddedViewModel(type, embeddedFields, template);
            }
            else
            {
                throw new ViewModelTypeNotFoundExpception(
                    String.Format("Could not find view model for schema {0} and component template {1} in loaded assemblies."
                    , key.SchemaName, key.ComponentTemplateName));
            }
            return result;
        }
        public IEmbeddedSchemaViewModel BuildEmbeddedViewModel(Type type, IFieldSet embeddedFields, IComponentTemplate template)
        {
            IEmbeddedSchemaViewModel viewModel = (IEmbeddedSchemaViewModel)ReflectionCache.CreateInstance(type);
            viewModel.EmbeddedFields = embeddedFields;
            viewModel.ComponentTemplate = template;
            viewModel.Builder = this;
            ProcessFields(embeddedFields, viewModel, type, template);
            return viewModel;
        }
        #endregion

        public IComponentPresentation BuildCPFromViewModel(IComponentPresentationViewModel viewModel) //For mocking DD4T objects
        {
            Type type = viewModel.GetType();
            ViewModelAttribute attr = ReflectionCache.GetViewModelAttribute(type);
            IComponentTemplate template = new ComponentTemplate { Title = attr.ComponentTemplateName };
            IFieldSet metadataFields;
            IFieldSet fields = CreateFields(viewModel, type, template, out metadataFields);
            IComponentPresentation result = new ComponentPresentation
            {
                Component = new Component
                {
                    Fields = (FieldSet)fields,
                    MetadataFields = (FieldSet)metadataFields,
                    Schema = new Schema { Title = attr.SchemaName }
                },
                ComponentTemplate = (ComponentTemplate)template
            };
            return result;
        }

        public IFieldSet BuildEmbeddedFieldsFromViewModel(IEmbeddedSchemaViewModel viewModel, out string schemaName)
        {
            Type type = viewModel.GetType();
            ViewModelAttribute attr = ReflectionCache.GetViewModelAttribute(type);
            IComponentTemplate template = new ComponentTemplate { Title = attr.ComponentTemplateName };
            IFieldSet metadataFields;
            schemaName = attr.SchemaName;
            return CreateFields(viewModel, type, template, out metadataFields);
        }

        #region Private methods
        private IFieldSet CreateFields(object viewModel, Type type, IComponentTemplate template, out IFieldSet metadataFields)
        {
            IFieldSet fields;
            IFieldSet contentFields = new FieldSet();
            metadataFields = new FieldSet();
            var props = ReflectionCache.GetFieldProperties(type);
            IField field = new Field();
            FieldAttributeBase fieldAttribute;
            object fieldValue = null;
            foreach (var prop in props)
            {
                fieldAttribute = prop.FieldAttribute;//prop.GetCustomAttributes(typeof(FieldAttributeBase), true).FirstOrDefault() as FieldAttributeBase;
                if (fieldAttribute != null) //It has a FieldAttribute
                {
                    fields = fieldAttribute.IsMetadata ? metadataFields : contentFields; //switch between the two as needed
                    if (contentFields != null)
                    {
                        //TODO: Check the property type and make sure it matches expected return type or throw an exception -- not sure this is worth it
                        fieldValue = prop.Get(viewModel);
                        if (fieldValue != null)
                        {
                            try
                            {
                                field = fieldAttribute.SetFieldValue(fieldValue, prop.PropertyType, this);
                            }
                            catch (Exception e)
                            {
                                if (e is TargetException || e is InvalidCastException)
                                    throw new InvalidCastException(
                                        String.Format("Type mismatch for property {0}. Expected type for {1} is {2}. Property is of type {3}."
                                        , prop.Name, fieldAttribute.GetType().Name, fieldAttribute.ExpectedReturnType.FullName, prop.PropertyType.FullName));
                                else throw e;
                            }
                            fields.Add(fieldAttribute.FieldName, field);
                        }
                    }
                }
            }
            return contentFields;
        }

        private void ProcessFields(IFieldSet contentFields, object viewModel, Type type, IComponentTemplate template, IFieldSet metadataFields = null)
        {
            //PropertyInfo[] props = type.GetProperties();
            var props = ReflectionCache.GetFieldProperties(type);
            IField field;
            IFieldSet fields;
            string fieldName;
            FieldAttributeBase fieldAttribute;
            object fieldValue = null;
            foreach (var prop in props)
            {
                fieldAttribute = prop.FieldAttribute;//prop.GetCustomAttributes(typeof(FieldAttributeBase), true).FirstOrDefault() as FieldAttributeBase;
                if (fieldAttribute != null) //It has a FieldAttribute
                {
                    fieldName = fieldAttribute.FieldName;
                    fields = fieldAttribute.IsMetadata ? metadataFields : contentFields;
                    if (fields != null && fields.ContainsKey(fieldName))
                    {
                        //TODO: Check the property type and make sure it matches expected return type or throw an exception -- not sure this is worth it
                        field = fields[fieldName];
                        fieldValue = fieldAttribute.GetFieldValue(field, prop.PropertyType, template, this); //delegate all the real work to the Field Attribute object itself. Allows for custom attribute types to easily be added
                        try
                        {
                            prop.Set(viewModel, fieldValue);
                        }
                        catch (Exception e)
                        {
                            if (e is TargetException || e is InvalidCastException)
                                throw new InvalidCastException(
                                    String.Format("Type mismatch for property {0}. Expected type for {1} is {2}. Property is of type {3}."
                                    , prop.Name, fieldAttribute.GetType().Name, fieldAttribute.ExpectedReturnType.FullName, prop.PropertyType.FullName));
                        }
                    }
                }
            }
        }
        #endregion
    }


}
