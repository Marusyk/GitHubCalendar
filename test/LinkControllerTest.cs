using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using Web.Controllers;
using Web.Services;
using Xunit;

namespace Tests
{
    public class LinkControllerTest
    {
        private readonly LinkController _sut;

        public LinkControllerTest()
        {
            var storageMock = new Mock<IStorage>();
            storageMock
                .Setup(t => t.Add(It.IsAny<string>()))
                .Returns(Guid.NewGuid().ToString());
            storageMock
                .Setup(t => t.Get(It.IsAny<string>()))
                .Returns(Guid.NewGuid().ToString());
            storageMock
                .Setup(t => t.Get(It.Is<string>(x => x.Equals("null"))))
                .Returns(string.Empty);

            _sut = new LinkController(storageMock.Object);
        }

        [Fact]
        public void Post_returns_bad_request_when_empty_url_passed()
        {
            // act
            var result = _sut.Post(string.Empty);
            var badRequestResult = result as BadRequestResult;

            // assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void Post_returns_bad_request_when_not_valid_url_passed()
        {
            // act
            var result = _sut.Post(Guid.NewGuid().ToString());
            var badRequestResult = result as BadRequestResult;

            // assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Theory]
        [InlineData("http://google.com")]
        [InlineData("https://microsoft.com")]
        [InlineData("https://github.com/Marusyk/")]
        public void Post_returns_created_status_code_with_url(string url)
        {
            // act
            var result = _sut.Post(url);
            var createdAtRouteResult = result as CreatedAtRouteResult;

            // assert
            Assert.NotNull(createdAtRouteResult);
            Assert.NotNull(createdAtRouteResult.Value);
            Assert.Equal(201, createdAtRouteResult.StatusCode);
        }

        [Fact]
        public void Get_returns_bad_request_when_empty_url_requested()
        {
            // act
            var result = _sut.Get(string.Empty);
            var badRequestResult = result as BadRequestResult;

            // assert
            Assert.NotNull(badRequestResult);
            Assert.Equal(400, badRequestResult.StatusCode);
        }

        [Fact]
        public void Get_redirect_to_shorten_url()
        {
            // act
            var result = _sut.Get(Guid.NewGuid().ToString());
            var redirectResult = result as RedirectResult;

            // assert
            Assert.NotNull(redirectResult);
        }

        [Fact]
        public void Get_returns_not_found_when_requested_not_shorten_url()
        {
            // act
            var result = _sut.Get("null");
            var notFoundResult = result as NotFoundResult;

            // assert
            Assert.NotNull(notFoundResult);
            Assert.Equal(404, notFoundResult.StatusCode);
        }
    }
}
