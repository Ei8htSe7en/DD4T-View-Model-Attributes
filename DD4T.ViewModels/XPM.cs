using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Web.Mvc;
using DD4T.Mvc.SiteEdit;
using DD4T.ViewModels.Reflection;
using DD4T.ViewModels.Attributes;
using DD4T.ViewModels.Contracts;
using System.Reflection;
using DD4T.ContentModel;

namespace DD4T.ViewModels.XPM
{
    public static class XPM
    {
        public static MvcHtmlString SiteEditableField<TModel, TProp>(this TModel model, Expression<Func<TModel, TProp>> propertyLambda, int index = -1) where TModel : IComponentPresentationViewModel
        {
            string markup = string.Empty;
            object value = null;
            string propValue = string.Empty;
            try
            {
                Type modelType = typeof(TModel);
                PropertyInfo property = ReflectionCache.GetPropertyInfo(propertyLambda);
                var fieldProp = GetFieldProperty(modelType, property);
                if (SiteEditService.IsSiteEditEnabled(model.ComponentPresentation.Component))
                {
                    var field = GetField(model, fieldProp);
                    markup = GenerateSiteEditTag(field, index);
                }
                value = fieldProp.Get(model);
                propValue = value == null ? string.Empty : value.ToString();
            }
            catch (NullReferenceException)
            {
                return null;
            }
            return new MvcHtmlString(markup + propValue);
        }

        private static string GenerateSiteEditTag(IField field, int index)
        {
            var result = index > 0 ? SiteEditService.GenerateSiteEditFieldTag(field, index)
                            : SiteEditService.GenerateSiteEditFieldTag(field);
            return result ?? string.Empty;
        }
        private static IField GetField(IComponentPresentationViewModel model, FieldAttributeProperty fieldProp)
        {
            var fields = model.ComponentPresentation.Component.Fields;
            var fieldName = fieldProp.FieldAttribute.FieldName;
            return fields.ContainsKey(fieldName) ? fields[fieldName] : null;
        }
        private static FieldAttributeProperty GetFieldProperty(Type type, PropertyInfo property)
        {
            var props = ReflectionCache.GetFieldProperties(type);
            return props.FirstOrDefault(x => x.Name == property.Name);
        }

        public static MvcHtmlString XpmMarkupFor<TModel, TProp>(this TModel model, Expression<Func<TModel, TProp>> propertyLambda, int index = -1) where TModel : IComponentPresentationViewModel
        {
            if (SiteEditService.IsSiteEditEnabled(model.ComponentPresentation.Component))
            {
                IField field;
                try
                {
                    Type modelType = typeof(TModel);
                    PropertyInfo property = ReflectionCache.GetPropertyInfo(propertyLambda);
                    var fieldProp = GetFieldProperty(modelType, property);
                    field = GetField(model, fieldProp);
                }
                catch (NullReferenceException)
                {
                    return null;
                }
                return new MvcHtmlString(GenerateSiteEditTag(field, index));
            }
            else return null;
        }

        //testing only
        public static IField FieldFor<TModel, TProp>(this TModel model, Expression<Func<TModel, TProp>> propertyLambda, int index = -1) where TModel : IComponentPresentationViewModel
        {
            Type modelType = typeof(TModel);
            PropertyInfo property = ReflectionCache.GetPropertyInfo(propertyLambda);
            var props = ReflectionCache.GetFieldProperties(modelType);
            var fieldProp = props.FirstOrDefault(x => x.PropertyType == property.PropertyType);
            var fields = model.ComponentPresentation.Component.Fields;
            var fieldName = fieldProp.FieldAttribute.FieldName;
            var field = fields.ContainsKey(fieldName) ? fields[fieldName] : null;
            return field;
        }

        public static MvcHtmlString StartXpmEditingZone(this IComponentPresentationViewModel model, string region = null)
        {
            return new MvcHtmlString(SiteEditService.GenerateSiteEditComponentTag(model.ComponentPresentation, region));
        }
    }
}
