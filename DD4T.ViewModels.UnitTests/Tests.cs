using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DD4T.ViewModels.Contracts;
using DD4T.ViewModels.Attributes;
using DD4T.ContentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Reflection;
using DD4T.ViewModels.XPM;
using System.Web;
using System.Web.Routing;
using System.IO;
using DD4T.Mvc.SiteEdit;
using Ploeh.AutoFixture;
using System.Linq.Expressions;

namespace DD4T.ViewModels.UnitTests
{
    [TestClass]
    public class Examples
    {
        private Random r = new Random();
        private Fixture autoMocker = new Fixture();
        [TestInitialize]
        public void Init()
        {
            //Mock up Current HttpContext so we can use Site Edit
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://ei8htSe7en.io", "num=" + GetRandom()),
                new HttpResponse(new StringWriter())
                ) { };
            MockSiteEdit("1", false);
        }
        private void MockSiteEdit(string pubId, bool enabled)
        {
            //Mock up Site Edit Settings for Pub ID 1
            SiteEditService.SiteEditSettings.Enabled = enabled;
            if (!SiteEditService.SiteEditSettings.ContainsKey(pubId))
            {
                SiteEditService.SiteEditSettings.Add(pubId,
                   new SiteEditSetting
                   {
                       Enabled = enabled,
                       ComponentPublication = pubId,
                       ContextPublication = pubId,
                       PagePublication = pubId,
                       PublishPublication = pubId
                   });
            }
        }
        private int GetRandom()
        {
            return r.Next(0, 101);
        }
        [TestMethod]
        public void TestBuildCPViewModelGeneric()
        {
            ContentContainerViewModel model = null;
            IViewModelBuilder builder = ViewModelDefaults.Builder;
            for (int i = 0; i < 10000; i++)
            {
                IComponentPresentation cp = GetMockCp(GetMockModel());
                model = builder.BuildCPViewModel<ContentContainerViewModel>(cp);
            }
            Assert.IsNotNull(model);
        }
        [TestMethod]
        public void TestBuildCPViewModelLoadedAssemblies()
        {

            ContentContainerViewModel model = null;
            IViewModelBuilder builder = ViewModelDefaults.Builder;
            for (int i = 0; i < 10000; i++)
            {
                IComponentPresentation cp = GetMockCp(GetMockModel());
                builder.LoadViewModels(Assembly.GetAssembly(typeof(ContentContainerViewModel)));
                model = (ContentContainerViewModel)builder.BuildCPViewModel(cp);
            }
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void TestXpmMarkup()
        {
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(GetMockCp(GetMockModel()));
            var titleMarkup = model.XpmMarkupFor(m => m.Title);
            var compMarkup = model.StartXpmEditingZone();
            //make sure the nested component gets a mock ID - used to test if site edit is enabled for component
            ((Component)((GeneralContentViewModel)model.Content[0]).ComponentPresentation.Component).Id = "tcm:1-555-16";
            var markup = ((GeneralContentViewModel)model.Content[0]).XpmMarkupFor(m => m.Body);
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmMarkupFor(m => m.LinkText);
            Assert.IsNotNull(markup);
        }

        [TestMethod]
        public void TestEditableField()
        {
            ContentContainerViewModel model = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(GetMockCp(GetMockModel()));
            MvcHtmlString contentMarkup;
            foreach (var content in model.Content)
            {
                contentMarkup = model.XpmEditableField(m => m.Content, content);
            }
            var titleMarkup = model.XpmEditableField(m => m.Title);
            var compMarkup = model.StartXpmEditingZone();

            var markup = ((GeneralContentViewModel)model.Content[0]).XpmEditableField(m => m.Body);
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmEditableField(m => m.LinkText);
            Assert.IsNotNull(markup);
        }
        [TestMethod]
        public void GetMockCp()
        {
            var model = GetMockCp(GetMockModel());
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void TestBodyField()
        {
            string expectedString = autoMocker.Create<string>();
            var cp = GetCPMockup<GeneralContentViewModel, MvcHtmlString>(x => x.Body, new MvcHtmlString(expectedString));
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<GeneralContentViewModel>(cp);
            Assert.AreEqual(expectedString, newModel.Body.ToHtmlString());
        }

        private ComponentPresentation GetManuallyBuiltCp()
        {
            Component comp = new Component { Id = "tcm:1-23" };
            var linksFieldSet = new FieldSet 
                { 
                    {
                        "internalLink",
                        new Field { LinkedComponentValues = new List<Component> 
                            { 
                                comp,
                            }
                        }
                    }
                };
            var links = new List<FieldSet>
            {
                linksFieldSet, linksFieldSet, linksFieldSet

            };
            var cp = new ComponentPresentation
            {
                Component = new Component
                {
                    Id = "tcm:1-45",
                    Fields = new FieldSet
                    {
                        {
                            "links", new Field
                            {
                                EmbeddedValues = links
                            }
                        }
                    }

                },
                ComponentTemplate = new ComponentTemplate()
            };
            return cp;
        }

        [TestMethod]
        public void TestEmbeddedAndIComponentField()
        {
            //setup
            string expectedString = autoMocker.Create<string>();
            var cp = GetManuallyBuiltCp();
            cp.Component.Id = expectedString;
            //exercise
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(cp);
            //test
            Assert.AreEqual(3, newModel.Links.Count);
            Assert.AreEqual(cp.Component.Id,
                newModel.Links.FirstOrDefault<EmbeddedLinkViewModel>().InternalLink.Id);
        }

        [TestMethod]
        public void TestLinkedComponentRichTextField()
        {
            string expectedString = autoMocker.Create<string>();
            var linkedCompModel = GetCPModelMockup<GeneralContentViewModel, MvcHtmlString>(
                x => x.Body, new MvcHtmlString(expectedString));
            var cp = GetCPMockup<ContentContainerViewModel, ViewModelList<GeneralContentViewModel>>(
                x => x.Content, new ViewModelList<GeneralContentViewModel> { linkedCompModel });
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(cp);
            Assert.AreEqual(expectedString,
                newModel.Content.FirstOrDefault<GeneralContentViewModel>().Body.ToHtmlString());
        }

        [TestMethod]
        public void TestEmbeddedField()
        {
            string expectedString = autoMocker.Create<string>();
            var linkedCompModel = autoMocker.Build<EmbeddedLinkViewModel>()
                 .Without(x => x.ComponentTemplate)
                 .Without(x => x.Builder)
                 .Without(x => x.Fields)
                 .Without(x => x.InternalLink)
                 .With(x => x.LinkText, expectedString)
                 .Create();
            var cp = GetCPMockup<ContentContainerViewModel, ViewModelList<EmbeddedLinkViewModel>>(
                x => x.Links, new ViewModelList<EmbeddedLinkViewModel> { linkedCompModel });
            var newModel = ViewModelDefaults.Builder.BuildCPViewModel<ContentContainerViewModel>(cp);
            Assert.AreEqual(expectedString,
                newModel.Links.FirstOrDefault<EmbeddedLinkViewModel>().LinkText);
        }

        [TestMethod]
        public void TestViewModelId()
        {
            string viewModelKey = "TitleOnly";
            ViewModelDefaults.Builder.LoadViewModels(typeof(GeneralContentViewModel).Assembly);
            var cp = GetMockCp(GetMockModel());
            ((ComponentTemplate)cp.ComponentTemplate).MetadataFields = new FieldSet
            {
                {
                    "viewModelKey",
                    new Field { Values = new List<string> { viewModelKey }}
                }
            };

            //exercise
            var model = ViewModelDefaults.Builder.BuildCPViewModel(cp);

            //test
            Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }

        [TestMethod]
        public void TestCustomKeyProvider()
        {
            string key = autoMocker.Create<string>();
            var cp = GetManuallyBuiltCp();
            cp.ComponentTemplate.MetadataFields = new FieldSet()
            {
                {
                    key,
                    new Field { Values = new List<string> { "TitleOnly" }}
                }
            };
            cp.Component.Schema = new Schema { Title = "ContentContainer" };
            var provider = new CustomKeyProvider(key);
            var builder = new ViewModelBuilder(provider);
            builder.LoadViewModels(typeof(ContentContainerViewModel).Assembly);
            var model = builder.BuildCPViewModel(cp);

            Assert.IsInstanceOfType(model, typeof(TitleViewModel));
        }

        private TModel GetCPModelMockup<TModel, TProp>(Expression<Func<TModel, TProp>> propLambda, TProp value)
            where TModel : IComponentPresentationViewModel
        {
            return autoMocker.Build<TModel>()
                 .Without(x => x.ComponentPresentation)
                 .Without(x => x.Builder)
                 .Without(x => x.Fields)
                 .With(propLambda, value)
                 .Create();
        }
        private TModel GetEmbeddedModelMockup<TModel, TProp>(Expression<Func<TModel, TProp>> propLambda, TProp value)
            where TModel : IEmbeddedSchemaViewModel
        {
            return autoMocker.Build<TModel>()
                 .Without(x => x.ComponentTemplate)
                 .Without(x => x.Builder)
                 .Without(x => x.Fields)
                 .With(propLambda, value)
                 .Create();
        }


        private IComponentPresentation GetCPMockup<TModel, TProp>(Expression<Func<TModel, TProp>> propLambda, TProp value)
            where TModel : IComponentPresentationViewModel
        {
            return ViewModelDefaults.Mocker.ConvertToComponentPresentation(GetCPModelMockup<TModel, TProp>(propLambda, value));
        }


        private IDD4TViewModel GetMockModel()
        {
            ContentContainerViewModel model = new ContentContainerViewModel
            {
                Content = new ViewModelList<GeneralContentViewModel>
                {
                    new GeneralContentViewModel
                    {
                        Body = new MvcHtmlString("<p>" + GetRandom() + "</p>"),
                        Title = "The title" + GetRandom(),
                        SubTitle = "The sub title"+ GetRandom(),
                        NumberFieldExample = GetRandom(),
                        ShowOnTop = true
                    }
                },
                Links = new ViewModelList<EmbeddedLinkViewModel>
                {
                    new EmbeddedLinkViewModel
                    {
                        LinkText = "I am a link " + GetRandom(),
                        ExternalLink = "http://google.com",
                        InternalLink = new Component { Id = GetRandom().ToString() },
                        Target = "_blank" + GetRandom()
                    }
                },
                Title = "I am a content container!"
            };
            return model;
        }

        private IComponentPresentation GetMockCp(IDD4TViewModel model)
        {
            IComponentPresentation cp = ViewModelDefaults.Mocker.ConvertToComponentPresentation(model);
            ((Component)cp.Component).Id = "tcm:1-23-16";
            Assert.IsNotNull(cp);
            return cp;
        }

    }

    public class CustomKeyProvider : ViewModelKeyProviderBase
    {
        public CustomKeyProvider(string fieldName)
        {
            this.ViewModelKeyField = fieldName;
        }
    }


    [ViewModel("ContentContainer", false, ViewModelKeys = new string[] { "TitleOnly" })]
    public class TitleViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title")]
        public string Title { get; set; }
    }

    [ViewModel("GeneralContent", true, ViewModelKeys = new string[] { "BasicGeneralContent" })]
    public class GeneralContentViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title")]
        public string Title { get; set; }

        [TextField("sutTitle", InlineEditable = true)]
        public string SubTitle { get; set; }

        [RichTextField("body", InlineEditable = true)]
        public MvcHtmlString Body { get; set; }

        [KeywordKeyField("showOnTop", IsBooleanValue = true)]
        public bool ShowOnTop { get; set; }

        [NumberField("someNumber")]
        public double NumberFieldExample { get; set; }
    }

    [ViewModel("ContentContainer", true)]
    public class ContentContainerViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title", InlineEditable = true)]
        public string Title { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
        public ViewModelList<GeneralContentViewModel> Content { get; set; }

        [EmbeddedSchemaField("links", typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public ViewModelList<EmbeddedLinkViewModel> Links { get; set; }
    }

    [ViewModel("EmbeddedLink", true)]
    public class EmbeddedLinkViewModel : EmbeddedSchemaViewModelBase
    {
        [TextField("linkText")]
        public string LinkText { get; set; }

        [LinkedComponentField("internalLink")]
        public IComponent InternalLink { get; set; }

        [TextField("externalLink")]
        public string ExternalLink { get; set; }

        [KeywordKeyField("openInNewWindow", AllowMultipleValues = false)]
        public string Target { get; set; }
    }
}
