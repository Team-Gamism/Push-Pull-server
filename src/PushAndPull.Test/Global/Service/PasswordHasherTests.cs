using PushAndPull.Global.Service;

namespace PushAndPull.Test.Global.Service;

public class PasswordHasherTests
{
    private const string Password = "s3cret-p@ssw0rd";

    public class WhenHashingAndVerifyingAPassword
    {
        private readonly PasswordHasher _sut = new();

        [Fact]
        public void It_VerifiesTheCorrectPassword()
        {
            var hash = _sut.Hash(Password);

            Assert.True(_sut.Verify(Password, hash));
        }

        [Fact]
        public void It_RejectsAnIncorrectPassword()
        {
            var hash = _sut.Hash(Password);

            Assert.False(_sut.Verify("wrong-password", hash));
        }

        [Fact]
        public void It_DoesNotStoreTheRawPassword()
        {
            var hash = _sut.Hash(Password);

            Assert.NotEqual(Password, hash);
        }

        [Fact]
        public void It_ProducesADifferentHashEachTimeBecauseOfTheSalt()
        {
            var first = _sut.Hash(Password);
            var second = _sut.Hash(Password);

            Assert.NotEqual(first, second);
        }
    }
}
