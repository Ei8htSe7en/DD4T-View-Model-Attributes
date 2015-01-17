using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DD4T.ViewModels.Contracts;
using DD4T.ViewModels.Attributes;
using DD4T.ContentModel;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using DD4T.ViewModels.Lists;
using DD4T.ViewModels.Builders;
using System.Reflection;
using DD4T.ViewModels.XPM;
using System.Web;
using System.Web.Routing;
using System.IO;
using DD4T.Mvc.SiteEdit;

namespace DD4T.ViewModels.UnitTests
{
    [TestClass]
    public class Examples
    {
        private Random r = new Random();
        [TestInitialize]
        public void Init()
        {
            //Mock up Current HttpContext so we can use Site Edit
            HttpContext.Current = new HttpContext(
                new HttpRequest("", "http://ei8htSe7en.io", "num=" + GetRandom()),
                new HttpResponse(new StringWriter())
                ){};
            MockSiteEdit("1", false);
        }
        public void MockSiteEdit(string pubId, bool enabled)
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
            IViewModelBuilder builder = ViewModelCore.Builder;
            for (int i = 0; i < 10000; i++)
            {
                IComponentPresentation cp = TestMockup();
                model = builder.BuildCPViewModel<ContentContainerViewModel>(cp);
            }
            Assert.IsNotNull(model);
        }
        [TestMethod]
        public void TestBuildCPViewModelLoadedAssemblies()
        {

            ContentContainerViewModel model = null;
            IViewModelBuilder builder = ViewModelCore.Builder;
            for (int i = 0; i < 10000; i++)
            {
                IComponentPresentation cp = TestMockup();
                builder.LoadViewModels(Assembly.GetAssembly(typeof(ContentContainerViewModel)));
                model = (ContentContainerViewModel)builder.BuildCPViewModel(cp);
            }
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void TestXpmMarkup()
        {
            ContentContainerViewModel model = ViewModelCore.Builder.BuildCPViewModel<ContentContainerViewModel>(TestMockup());
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
            ContentContainerViewModel model = ViewModelCore.Builder.BuildCPViewModel<ContentContainerViewModel>(TestMockup());
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
        public IComponentPresentation TestMockup()
        {
            Random r = new Random();
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

            IComponentPresentation cp = ViewModelCore.Mocker.ConvertToComponentPresentation(model);
            ((Component)cp.Component).Id = "tcm:1-23-16";
            Assert.IsNotNull(cp);
            return cp;
        }

    }

    [ViewModel("GeneralContent")]
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
    [ViewModel("ContentContainer")]
    public class ContentContainerViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title", InlineEditable = true)]
        public string Title { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
        public ViewModelList<GeneralContentViewModel> Content { get; set; }

        [EmbeddedSchemaField("links", typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public ViewModelList<EmbeddedLinkViewModel> Links { get; set; }

    }
    [ViewModel("EmbeddedLink")]
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
