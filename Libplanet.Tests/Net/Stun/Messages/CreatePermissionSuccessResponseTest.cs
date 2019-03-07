using System.IO;
using System.Threading.Tasks;
using Libplanet.Net.Stun.Messages;
using Xunit;

namespace Libplanet.Tests.Net.Stun.Messages
{
    public class CreatePermissionSuccessResponseTest
    {
        [Fact]
        public async Task ParseBytes()
        {
            var bytes = new byte[]
            {
                0x01, 0x08, 0x00, 0x28, 0x21, 0x12, 0xa4, 0x42, 0xf8, 0x57,
                0xe3, 0x50, 0x4c, 0x8f, 0xd3, 0x9d, 0xb8, 0xca, 0x69, 0x83,
                0x80, 0x22, 0x00, 0x04, 0x4e, 0x6f, 0x6e, 0x65, 0x00, 0x08,
                0x00, 0x14, 0xee, 0x56, 0x3a, 0x74, 0x03, 0x16, 0x80, 0xc4,
                0xe5, 0x2f, 0xfa, 0x37, 0x1e, 0x30, 0x42, 0xa6, 0x45, 0x15,
                0x8e, 0x84, 0x80, 0x28, 0x00, 0x04, 0xa0, 0x39, 0xc6, 0x25,
            };

            using (var stream = new MemoryStream(bytes))
            {
                var response = await StunMessage.Parse(stream);
                Assert.IsType<CreatePermissionSuccessResponse>(response);
            }
        }
    }
}
