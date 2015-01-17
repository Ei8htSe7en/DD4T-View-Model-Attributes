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
            var markup = ((GeneralContentViewModel)model.Content[0]).XpmMarkupFor(m => m.Body);
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmMarkupFor(m => m.LinkText);
            Assert.IsNotNull(markup);
        }

        [TestMethod]
        public void TestEditableField()
        {
            ContentContainerViewModel model = ViewModelCore.Builder.BuildCPViewModel<ContentContainerViewModel>(TestMockup());
            foreach (var content in model.Content)
            {
                model.XpmEditableField(m => m.Content, 0);
            }
            var titleMarkup = model.XpmEditableField(m => m.Title);
            var compMarkup = model.StartXpmEditingZone();
            
            var markup = ((GeneralContentViewModel)model.Content[0]).XpmEditableField(m => m.Body);
            var embeddedTest = ((EmbeddedLinkViewModel)model.Links[0]).XpmEditableField(m => m.LinkText);
            Assert.IsNotNull(markup);
        }

        
        [TestMethod]
        public void TestFieldForExtension()
        {
            ContentContainerViewModel model = ViewModelCore.Builder.BuildCPViewModel<ContentContainerViewModel>(TestMockup());
            var titleField = model.FieldFor(m => m.Title);
            var compMarkup = model.ComponentPresentation.Component;
            IField markup = null;
            IField embeddedTest = null;
            foreach (var content in model.Content)
            {
                markup = content.FieldFor(m => m.Body);
            }
            foreach (var link in model.Links)
            {
                embeddedTest = link.FieldFor(m => m.InternalLink);
            }
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
                        Body = new MvcHtmlString("<p>" + r.Next(0,100) + "</p>"),
                        Title = "The title" + r.Next(0,100),
                        SubTitle = "The sub title"+ r.Next(0,100),
                        NumberFieldExample = r.Next(0,100),
                        ShowOnTop = true
                    }
                },
                Links = new ViewModelList<EmbeddedLinkViewModel>
                {
                    new EmbeddedLinkViewModel
                    {
                        LinkText = "I am a link " + r.Next(0,100),
                        ExternalLink = "http://google.com",
                        InternalLink = new Component { Id = r.Next(0,100).ToString() },
                        Target = "_blank" + r.Next(0,100)
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
        public ViewModelList<GeneralContentViewModel> Content { get; set; }

        [EmbeddedSchemaField("links", typeof(EmbeddedLinkViewModel), AllowMultipleValues = true)]
        public ViewModelList<EmbeddedLinkViewModel> Links { get; set; }

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
