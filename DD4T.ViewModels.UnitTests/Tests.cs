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

namespace DD4T.ViewModels.UnitTests
{
    [TestClass]
    public class Examples
    {
        [TestMethod]
        public void TestBuildCPViewModelGeneric()
        {

            ContentContainerViewModel model = null;
            IViewModelBuilder builder = ViewModelCore.Builder;
            for (int i = 0; i < 10000; i++)
            {
                IComponentPresentation cp = TestMocking();
                model = builder.BuildCPViewModel<ContentContainerViewModel>(cp);
                //builder.LoadViewModels(Assembly.GetAssembly(typeof(ContentContainerViewModel)));
                //builder.BuildCPViewModel(cp);
            }
            Assert.IsNotNull(model);
        }

        [TestMethod]
        public IComponentPresentation TestMocking()
        {
            ContentContainerViewModel model = new ContentContainerViewModel
            {
                Content = new ComponentViewModelList<GeneralContentViewModel>
                {
                    new GeneralContentViewModel
                    {
                        Body = new MvcHtmlString("<p>hello world</p>"),
                        Title = "The title",
                        SubTitle = "The sub title",
                        NumberFieldExample = 5,
                        ShowOnTop = true
                    }
                },
                Links = new EmbeddedViewModelList<EmbeddedLinkViewModel>
                {
                    new EmbeddedLinkViewModel
                    {
                        LinkText = "I am a link",
                        ExternalLink = "http://google.com",
                        InternalLink = new Component { Id = "tcm:1-123-16" },
                        Target = "_blank"
                    }
                },
                Title = "I am a content container!"
            };
            IComponentPresentation cp = ViewModelCore.Builder.BuildCPFromViewModel(model);
            Assert.IsNotNull(cp);
            return cp;
        }

    }

    [TestClass]
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
    [TestClass]
    [ViewModel("ContentContainer")]
    public class ContentContainerViewModel : ComponentPresentationViewModelBase
    {
        [TextField("title", InlineEditable = true)]
        public string Title { get; set; }

        [LinkedComponentField("content", LinkedComponentTypes = new Type[] { typeof(GeneralContentViewModel) }, AllowMultipleValues = true)]
        public ComponentViewModelList<GeneralContentViewModel> Content { get; set; }

        [EmbeddedSchemaField("links", typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public EmbeddedViewModelList<EmbeddedLinkViewModel> Links { get; set; }

    }
    [TestClass]
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
