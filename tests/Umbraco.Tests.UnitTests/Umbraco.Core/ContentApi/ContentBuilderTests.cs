using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core.ContentApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.ContentApi;

[TestFixture]
public class ContentBuilderTests : ContentApiTests
{
    [Test]
    public void ContentBuilder_MapsContentDataAndPropertiesCorrectly()
    {
        var content = new Mock<IPublishedContent>();

        var prop1 = new PublishedElementPropertyBase(ContentApiPropertyType, content.Object, false, PropertyCacheLevel.None);
        var prop2 = new PublishedElementPropertyBase(DefaultPropertyType, content.Object, false, PropertyCacheLevel.None);

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        var key = Guid.NewGuid();
        content.SetupGet(c => c.Properties).Returns(new[] { prop1, prop2 });
        content.SetupGet(c => c.UrlSegment).Returns("url-segment");
        content.SetupGet(c => c.Name).Returns("The page");
        content.SetupGet(c => c.Key).Returns(key);
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);
        content.SetupGet(c => c.ItemType).Returns(PublishedItemType.Content);

        var publishedUrlProvider = new Mock<IPublishedUrlProvider>();
        publishedUrlProvider
            .Setup(p => p.GetUrl(It.IsAny<IPublishedContent>(), It.IsAny<UrlMode>(), It.IsAny<string?>(), It.IsAny<Uri?>()))
            .Returns((IPublishedContent content, UrlMode mode, string? culture, Uri? current) => $"url:{content.UrlSegment}");

        var routeBuilder = new ApiContentRouteBuilder(publishedUrlProvider.Object, CreateGlobalSettings());

        var builder = new ApiContentBuilder(new ApiContentNameProvider(), routeBuilder, CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(content.Object);

        Assert.NotNull(result);
        Assert.AreEqual("The page", result.Name);
        Assert.AreEqual("thePageType", result.ContentType);
        Assert.AreEqual("/url:url-segment", result.Route.Path);
        Assert.AreEqual(key, result.Id);
        Assert.AreEqual(2, result.Properties.Count);
        Assert.AreEqual("Content API value", result.Properties["contentApi"]);
        Assert.AreEqual("Default value", result.Properties["default"]);
    }

    [Test]
    public void ContentBuilder_CanCustomizeContentNameInContentApiOutput()
    {
        var content = new Mock<IPublishedContent>();

        var contentType = new Mock<IPublishedContentType>();
        contentType.SetupGet(c => c.Alias).Returns("thePageType");

        content.SetupGet(c => c.Properties).Returns(Array.Empty<PublishedElementPropertyBase>());
        content.SetupGet(c => c.Name).Returns("The page");
        content.SetupGet(c => c.ContentType).Returns(contentType.Object);

        var customNameProvider = new Mock<IApiContentNameProvider>();
        customNameProvider.Setup(n => n.GetName(content.Object)).Returns($"Custom name for: {content.Object.Name}");

        var builder = new ApiContentBuilder(customNameProvider.Object, Mock.Of<IApiContentRouteBuilder>(), CreateOutputExpansionStrategyAccessor());
        var result = builder.Build(content.Object);

        Assert.NotNull(result);
        Assert.AreEqual("Custom name for: The page", result.Name);
    }
}
