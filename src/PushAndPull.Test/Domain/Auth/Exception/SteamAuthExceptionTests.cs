using System.Net;
using Gamism.SDK.Extensions.AspNetCore.Exceptions;
using PushAndPull.Domain.Auth.Exception;

namespace PushAndPull.Test.Domain.Auth.Exception;

public class SteamAuthExceptionTests
{
    public class WhenAnInvalidTicketExceptionIsCreated
    {
        [Fact]
        public void It_IsAnExpectedException()
        {
            Assert.IsAssignableFrom<ExpectedException>(new InvalidTicketException());
        }

        [Fact]
        public void It_MapsToUnauthorized()
        {
            Assert.Equal(HttpStatusCode.Unauthorized, new InvalidTicketException().StatusCode);
        }
    }

    public class WhenAVacBannedExceptionIsCreated
    {
        [Fact]
        public void It_MapsToForbidden()
        {
            Assert.Equal(HttpStatusCode.Forbidden, new VacBannedException(7UL).StatusCode);
        }

        [Fact]
        public void It_PreservesTheSteamId()
        {
            Assert.Equal(7UL, new VacBannedException(7UL).SteamId);
        }
    }

    public class WhenAPublisherBannedExceptionIsCreated
    {
        [Fact]
        public void It_MapsToForbidden()
        {
            Assert.Equal(HttpStatusCode.Forbidden, new PublisherBannedException(7UL).StatusCode);
        }

        [Fact]
        public void It_PreservesTheSteamId()
        {
            Assert.Equal(7UL, new PublisherBannedException(7UL).SteamId);
        }
    }

    public class WhenAFamilySharingNotAllowedExceptionIsCreated
    {
        [Fact]
        public void It_MapsToForbidden()
        {
            Assert.Equal(HttpStatusCode.Forbidden, new FamilySharingNotAllowedException(7UL).StatusCode);
        }

        [Fact]
        public void It_PreservesTheSteamId()
        {
            Assert.Equal(7UL, new FamilySharingNotAllowedException(7UL).SteamId);
        }
    }

    public class WhenASteamApiExceptionIsCreated
    {
        [Fact]
        public void It_IsNotAnExpectedException()
        {
            Assert.False(typeof(ExpectedException).IsAssignableFrom(typeof(SteamApiException)));
        }
    }
}
