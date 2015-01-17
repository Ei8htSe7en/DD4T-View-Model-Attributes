using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DD4T.ViewModels.Contracts;
using DD4T.ContentModel;
using System.Reflection;
using DD4T.Mvc.Html;
using System.Web.Mvc;
using DD4T.ViewModels.Reflection;

namespace DD4T.ViewModels.Attributes
{
    public abstract class FieldAttributeBase : Attribute
    {
        protected readonly string fieldName;
        protected bool allowMultipleValues = false;
        protected bool inlineEditable = false;
        protected bool mandatory = false; //probably don't need this one
        protected bool isMetadata = false;
        public FieldAttributeBase(string fieldName)
        {
            this.fieldName = fieldName;
        }
        public abstract object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null);
        public abstract IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null);
        public abstract Type ExpectedReturnType { get; }
        public string FieldName { get { return fieldName; } }
        public bool AllowMultipleValues
        {
            get
            {
                return allowMultipleValues;
            }
            set { allowMultipleValues = value; }
        }
        public bool InlineEditable
        {
            get
            {
                return inlineEditable;
            }
            set
            {
                inlineEditable = value;
            }
        }
        public bool Mandatory
        {
            get
            {
                return mandatory;
            }
            set
            {
                mandatory = value;
            }
        }
        public bool IsMetadata
        {
            get { return isMetadata; }
            set { isMetadata = value; }
        }
    }

    //Do we need a Metadata attribtue or should is just be a param of the Field Attribute?
    //public class MetadataFieldAttribute : FieldAttribute
    //{
    //    //Any key differences between Metadata and normal fields? Only difference is to use Component.MetadataField
    //    public MetadataFieldAttribute(string fieldName, SchemaFieldType fieldType)
    //        : base(fieldName, fieldType)
    //    { }
    //}

    /// <summary>
    /// Attribute for a Component Link Field
    /// </summary>
    public class LinkedComponentFieldAttribute : FieldAttributeBase
    {
        protected Type[] linkedComponentTypes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fieldName"></param>
        public LinkedComponentFieldAttribute(string fieldName) : base(fieldName) { }
        public Type[] LinkedComponentTypes //Is there anyway to enforce the types passed to this?
        {
            get
            {
                return linkedComponentTypes;
            }
            set
            {
                linkedComponentTypes = value;
            }
        }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                if (linkedComponentTypes == null)
                {
                    fieldValue = field.LinkedComponentValues;
                }
                else
                {
                    //Property must implement IList<IComponentPresentationViewModel> -- use ComponentViewModelList<T>
                    IList<IComponentPresentationViewModel> list =
                        (IList<IComponentPresentationViewModel>)ReflectionCache.CreateInstance(propertyType);
                    foreach (var component in field.LinkedComponentValues)
                    {
                        list.Add(BuildLinkedComponent(field.LinkedComponentValues[0], template, builder));
                    }
                    fieldValue = list;
                }
            }
            else
            {
                fieldValue = linkedComponentTypes == null ? (object)field.LinkedComponentValues[0]
                    : (object)BuildLinkedComponent(field.LinkedComponentValues[0], template, builder);
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.ComponentLink };
            if (value != null)
            {
                field.LinkedComponentValues = new List<Component>();
                if (AllowMultipleValues)
                {

                    if (linkedComponentTypes == null)
                    {
                        field.LinkedComponentValues = (List<Component>)value;
                    }
                    else
                    {
                        //Property must implement IList<IComponentPresentationViewModel> -- use ComponentViewModelList<T>
                        IList<IComponentPresentationViewModel> list = (IList<IComponentPresentationViewModel>)value;
                        foreach (var model in list)
                        {
                            field.LinkedComponentValues.Add((Component)builder.ConvertToComponentPresentation(model).Component);
                        }
                    }
                }
                else
                {
                    field.LinkedComponentValues.Add(linkedComponentTypes == null ? (Component)value
                        : (Component)builder.ConvertToComponentPresentation((IComponentPresentationViewModel)value).Component);
                }
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get
            {
                if (AllowMultipleValues)
                {
                    return linkedComponentTypes == null ? typeof(IList<IComponent>) : typeof(IList<IComponentPresentationViewModel>);
                }
                else
                {
                    return linkedComponentTypes == null ? typeof(IComponent) : typeof(IComponentPresentationViewModel);
                }
            }
        }

        private IComponentPresentationViewModel BuildLinkedComponent(IComponent component, IComponentTemplate template, IViewModelBuilder builder)
        {
            IComponentPresentation linkedCp = new ComponentPresentation
            {
                Component = component as Component,
                ComponentTemplate = template as ComponentTemplate
            };
            //need to determine schema to choose the Type
            Type type = GetViewModelType(component.Schema, template);
            //linkedModel = BuildCPViewModel(linkedType, linkedCp);
            return builder.BuildCPViewModel(type, linkedCp);
        }

        public Type GetViewModelType(ISchema schema, IComponentTemplate template = null)
        {
            //Create some algorithm to determine the proper view model type, perhaps build a static collection of all Types with the
            //View Model Attribute and set the key to the schema name + template name?
            if (schema == null) throw new ArgumentNullException("schema");
            //string ctName;
            foreach (var type in LinkedComponentTypes)
            {
                ViewModelAttribute modelAttr = ReflectionCache.GetViewModelAttribute(type);
                if (modelAttr != null && schema.Title == modelAttr.SchemaName) return type;

                //TODO: Possible include another type of marker besides CT to differentiate between different view models when using different template
                //ctName = template == null ? null : template.Title;
                //Compare CT and Schema -- this won't work for re-using component view models as both linked components and CPs until we have multiple CT names in attribute
                //if (modelAttr != null && new ViewModelKey(schema.Title, ctName).Equals(
                //    new ViewModelKey(modelAttr.SchemaName, modelAttr.ComponentTemplateName))) return type; 
            }
            return null;
        }
    }

    public class EmbeddedSchemaFieldAttribute : FieldAttributeBase
    {
        protected Type embeddedSchemaType;
        public EmbeddedSchemaFieldAttribute(string fieldName, Type embeddedSchemaType)
            : base(fieldName)
        {
            this.embeddedSchemaType = embeddedSchemaType;
        }
        public Type EmbeddedSchemaType
        {
            get
            {
                return embeddedSchemaType;
            }
        }
        public EmbeddedSchemaFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                //Property must implement IList<IEmbeddedSchemaViewModel> -- use EmbeddedViewModelList<T>
                IList<IEmbeddedSchemaViewModel> list = (IList<IEmbeddedSchemaViewModel>)ReflectionCache.CreateInstance(propertyType);
                foreach (var fieldSet in field.EmbeddedValues)
                {
                    list.Add(builder.BuildEmbeddedViewModel(
                    EmbeddedSchemaType,
                    fieldSet, template));
                }
                fieldValue = list;
            }
            else
            {
                fieldValue = builder.BuildEmbeddedViewModel(EmbeddedSchemaType, field.EmbeddedValues[0], template);
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Embedded };
            string schemaName = string.Empty;
            if (value != null)
            {
                field.EmbeddedValues = new List<FieldSet>();
                if (AllowMultipleValues)
                {

                    if (embeddedSchemaType == null)
                    {
                        field.EmbeddedValues = (List<FieldSet>)value;
                    }
                    else
                    {
                        //Property must implement IList<IEmbeddedSchemaViewModel> -- use EmbeddedViewModelList<T>
                        IList<IEmbeddedSchemaViewModel> list = (IList<IEmbeddedSchemaViewModel>)value;

                        foreach (var model in list)
                        {
                            field.EmbeddedValues.Add((FieldSet)builder.ConvertToFieldSet(model, out schemaName));
                        }
                    }
                }
                else
                {
                    field.EmbeddedValues.Add(embeddedSchemaType == null ? (FieldSet)value
                        : (FieldSet)builder.ConvertToFieldSet((IEmbeddedSchemaViewModel)value, out schemaName));
                }
                field.EmbeddedSchema = new Schema { Title = schemaName };
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<IEmbeddedSchemaViewModel>) : typeof(IEmbeddedSchemaViewModel); }
        }
    }

    public class MultimediaFieldAttribute : FieldAttributeBase
    {
        public MultimediaFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                fieldValue = field.LinkedComponentValues.Select(v => v.Multimedia).ToList();
            }
            else
            {
                fieldValue = field.LinkedComponentValues[0].Multimedia;
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.MultiMediaLink };

            field.LinkedComponentValues = AllowMultipleValues ?
                ((List<IMultimedia>)value).Select(x => new Component { Multimedia = (Multimedia)x }).ToList()
                : new List<Component>() { new Component { Multimedia = (Multimedia)value } };

            return field;
        }
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<IMultimedia>) : typeof(IMultimedia); }
        }
    }

    public class TextFieldAttribute : FieldAttributeBase, ICanBeBoolean
    {
        public TextFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                if (IsBooleanValue)
                    fieldValue = field.Values.Select(v => { bool b; return bool.TryParse(v, out b) && b; }).ToList();
                else fieldValue = field.Values;
            }
            else
            {
                if (IsBooleanValue)
                {
                    bool b;
                    fieldValue = bool.TryParse(field.Value, out b) && b;
                }
                else fieldValue = field.Value;
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.MultiMediaLink };
            if (AllowMultipleValues)
            {
                if (IsBooleanValue)
                {
                    field.Values = ((List<bool>)value).Select(x => x.ToString()).ToList();
                }
                else
                    field.Values = (List<string>)value;
            }
            else
            {
                field.Values = new List<string>() { value.ToString() };
            }
            return field;
        }
        public bool IsBooleanValue { get; set; }
        public override Type ExpectedReturnType
        {
            get
            {
                if (AllowMultipleValues)
                    return IsBooleanValue ? typeof(IList<bool>) : typeof(IList<string>);
                else return IsBooleanValue ? typeof(bool) : typeof(string);
            }
        }
    }

    public class RichTextFieldAttribute : FieldAttributeBase
    {
        public RichTextFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                fieldValue = field.Values.Select(v => v.ResolveRichText()).ToList();
            }
            else
            {
                fieldValue = field.Value.ResolveRichText();
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Xhtml };
            if (AllowMultipleValues)
            {
                field.Values = ((List<MvcHtmlString>)value).Select(x => x.ToHtmlString()).ToList();
            }
            else
            {
                field.Values = new List<string>() { ((MvcHtmlString)value).ToHtmlString() };
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<MvcHtmlString>) : typeof(MvcHtmlString); }
        }
    }

    public class NumberFieldAttribute : FieldAttributeBase
    {
        public NumberFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                fieldValue = field.NumericValues;
            }
            else
            {
                fieldValue = field.NumericValues[0];
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Number };
            if (AllowMultipleValues)
            {
                field.NumericValues = (List<double>)value;
            }
            else
            {
                field.NumericValues = new List<double>() { (double)value };
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<double>) : typeof(double); }
        }

    }

    public class DateFieldAttribute : FieldAttributeBase
    {
        public DateFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                fieldValue = field.DateTimeValues;
            }
            else
            {
                fieldValue = field.DateTimeValues[0];
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Date };
            if (AllowMultipleValues)
            {
                field.DateTimeValues = (List<DateTime>)value;
            }
            else
            {
                field.DateTimeValues = new List<DateTime>() { (DateTime)value };
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<DateTime>) : typeof(DateTime); }
        }
    }

    public class KeywordFieldAttribute : FieldAttributeBase
    {
        public KeywordFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object fieldValue = null;
            if (AllowMultipleValues)
            {
                fieldValue = field.Keywords;
            }
            else
            {
                fieldValue = field.Keywords[0];
            }
            return fieldValue;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Keyword };
            if (AllowMultipleValues)
            {
                field.Keywords = (List<Keyword>)value;
            }
            else
            {
                field.Keywords = new List<Keyword>() { (Keyword)value };
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get { return AllowMultipleValues ? typeof(IList<IKeyword>) : typeof(IKeyword); }
        }
    }

    public class KeywordKeyFieldAttribute : FieldAttributeBase, ICanBeBoolean
    {
        public KeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object value = null;
            if (AllowMultipleValues)
            {
                if (IsBooleanValue)
                    value = field.Keywords.Select(k => { bool b; return bool.TryParse(k.Key, out b) && b; }).ToList();
                else value = field.Keywords.Select(k => k.Key);
            }
            else
            {
                if (IsBooleanValue)
                {
                    bool b;
                    value = bool.TryParse(field.Keywords[0].Key, out b) && b;
                }
                else value = field.Keywords[0].Key;
            }
            return value;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Keyword };
            if (AllowMultipleValues)
            {
                if (IsBooleanValue)
                {
                    field.Keywords = ((List<bool>)value).Select(x => new Keyword { Key = x.ToString() }).ToList();
                }
                else
                    field.Keywords = ((List<string>)value).Select(x => new Keyword { Key = x }).ToList();
            }
            else
            {
                field.Keywords = new List<Keyword>() { new Keyword { Key = value.ToString() } };
            }
            return field;
        }
        public bool IsBooleanValue { get; set; }
        public override Type ExpectedReturnType
        {
            get
            {
                if (AllowMultipleValues)
                    return IsBooleanValue ? typeof(IList<bool>) : typeof(IList<string>);
                else return IsBooleanValue ? typeof(bool) : typeof(string);
            }
        }
    }

    public class NumericKeywordKeyFieldAttribute : FieldAttributeBase
    {
        public NumericKeywordKeyFieldAttribute(string fieldName) : base(fieldName) { }
        public override object GetFieldValue(IField field, Type propertyType, IComponentTemplate template, IViewModelBuilder builder = null)
        {
            object value = null;
            if (AllowMultipleValues)
            {
                value = field.Keywords.Select(k => { double i; double.TryParse(k.Key, out i); return i; }).ToList();
            }
            else
            {
                double i;
                double.TryParse(field.Keywords[0].Key, out i);
                value = i;
            }
            return value;
        }
        public override IField SetFieldValue(object value, Type propertyType, IComponentPresentationMocker builder = null)
        {
            Field field = new Field { FieldType = FieldType.Keyword };
            if (AllowMultipleValues)
            {
                field.Keywords = ((List<double>)value).Select(x => new Keyword { Key = x.ToString() }).ToList();
            }
            else
            {
                field.Keywords = new List<Keyword>() { new Keyword { Key = value.ToString() } };
            }
            return field;
        }
        public override Type ExpectedReturnType
        {
            get
            {
                return AllowMultipleValues ? typeof(IList<double>) : typeof(double);
            }
        }
    }

    //TODO: Allow multiple CT Names
    public class ViewModelAttribute : Attribute
    {
        private string schemaName;
        private bool inlineEditable = false;
        private string componentTemplateName;
        public ViewModelAttribute(string schemaName)
        {
            this.schemaName = schemaName;
        }

        public string SchemaName
        {
            get
            {
                return schemaName;
            }
        }

        public string ComponentTemplateName
        {
            get { return componentTemplateName; }
            set { componentTemplateName = value; }
        }

        public bool InlineEditable
        {
            get
            {
                return inlineEditable;
            }
            set
            {
                inlineEditable = value;
            }
        }

    }
}
