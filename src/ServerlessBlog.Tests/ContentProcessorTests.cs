using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.Core;
using Amazon.Lambda.S3Events;
using Amazon.Lambda.TestUtilities;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Moq;
using ServerlessBlog.Application;
using ServerlessBlog.Processor;
using Xunit;

namespace ServerlessBlog.Tests
{
    public class ContentProcessorTests
    {
        private readonly Mock<IAmazonS3> _s3ClientMock;
        private readonly Mock<IBlogDataProvider> _dataProviderMock;
        private readonly ContentProcessor _sut;

        public ContentProcessorTests()
        {
            _s3ClientMock = new Mock<IAmazonS3>();
            _dataProviderMock = new Mock<IBlogDataProvider>();
            _sut = new ContentProcessor(_s3ClientMock.Object, _dataProviderMock.Object);
        }

        [Fact]
        public async Task ShouldProcessContent()
        {
            // arrange
            var documentContent =
                await File.ReadAllTextAsync("./DocumentSamples/detect-when-a-javascript-popup-window-gets-closed.md");
            var documentStream = new MemoryStream(Encoding.UTF8.GetBytes(documentContent));
            var s3Response = new GetObjectResponse
            {
                ResponseStream = documentStream
            };
            _s3ClientMock.Setup(x =>
                    x.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(s3Response);

            var s3Evt = new S3Event
            {
                Records = new List<S3EventNotification.S3EventNotificationRecord>()
                {
                    new S3EventNotification.S3EventNotificationRecord
                    {
                        S3 = new S3EventNotification.S3Entity
                        {
                            Bucket = new S3EventNotification.S3BucketEntity
                            {
                                Name = "test-bucket"
                            },
                            Object = new S3EventNotification.S3ObjectEntity
                            {
                                Key = "some-key.md"
                            }
                        },
                        EventName = EventType.ObjectCreatedPost
                    }
                }
            };

            var lambdaContextMock = new Mock<ILambdaContext>();
            lambdaContextMock.SetupGet(x => x.Logger).Returns(new TestLambdaLogger());

            // act
            await _sut.Process(s3Evt, lambdaContextMock.Object);

            // assert
            const string expectedPostId = "some-key";
            const string expectedTitle = "Post title";
            const string expectedDate = "12 Nov 2014";
            const string expectedBody = "<p>First line.</p>\n" +
                                        "<pre><code class=\"language-javascript\">var sm = &quot;abc&quot;;\n" +
                                        "</code></pre>\n" +
                                        "<p>Last line.</p>\n";
            _dataProviderMock.Verify(
                x => x.SavePostAsync(It.Is<Post>(p =>
                    p.PostId == expectedPostId &&
                    p.Title == expectedTitle &&
                    p.CreateDateString == expectedDate &&
                    p.Body == expectedBody)),
                Times.Once);
        }
    }
}
