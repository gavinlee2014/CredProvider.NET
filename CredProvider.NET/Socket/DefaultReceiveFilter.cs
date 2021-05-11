using SuperSocket.ProtoBase;
using System.Text;

namespace CredProvider.NET.Socket
{
    class DefaultReceiveFilter : TerminatorReceiveFilter<DefaultPackageInfo>
    {
        private const string terminator = "\r\n";

        public DefaultReceiveFilter() : base(Encoding.ASCII.GetBytes(terminator))
        {
        }

        public override DefaultPackageInfo ResolvePackage(IBufferStream bufferStream)
        {
            string content = bufferStream.ReadString((int)bufferStream.Length, Encoding.UTF8);
            if (content.IndexOf(terminator) > -1)
            {
                content = content.Substring(0, content.IndexOf(terminator));
            }

            string key = content.Substring(0, content.IndexOf(" "));
            string body = content.Substring(content.IndexOf(" "));


            return new DefaultPackageInfo(key, body);
        }
    }
}
