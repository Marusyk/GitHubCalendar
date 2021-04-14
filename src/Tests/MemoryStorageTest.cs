using Web.Services;
using Xunit;

namespace Tests
{
    public class MemoryStorageTest
    {
        private readonly IStorage _sut;
        private readonly string _url;
        private readonly string _shortName;

        public MemoryStorageTest()
        {
            _sut = new MemoryStorage();
            _url = "https://google.com";
            _shortName = "ahr0chm";
        }

        [Fact]
        public void Add_value()
        {
            // act
            var result = _sut.Add(_url);

            // assert
            Assert.Equal(_shortName, result);
        }

        [Fact]
        public void Get_url_success()
        {
            // aggange
            _ = _sut.Add(_url);

            // act
            var result = _sut.Get(_shortName);

            // assert
            Assert.Equal(_url, result);
        }

        [Fact]
        public void Get_url_fail()
        {
            // aggange
            _ = _sut.Add("http://misc.com");

            // act
            var result = _sut.Get(_shortName);

            // assert
            Assert.NotEqual(_url, result);
        }

        [Fact]
        public void Clear()
        {
            // aggange
            _ = _sut.Add(_url);

            // act
            _sut.Clear();
            var result = _sut.Get(_shortName);

            // assert
            Assert.Null(result);
        }
    }
}
