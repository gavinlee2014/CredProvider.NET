using SuperSocket.ProtoBase;
using System;

namespace CredProvider.NET.Socket
{
    class DefaultPackageInfo : IPackageInfo<CommandName>, IPackageInfo
    {
        public CommandName Key { get; private set; }

        public byte[] Body { get; private set; }

        public DefaultPackageInfo(string key, string body)
        {
            Key = GetCommandKey(key);
            Body = Convert.FromBase64String(body);
        }

        private CommandName GetCommandKey(string key)
        {
            switch (key)
            {
                case "CODE":
                    return CommandName.CODE;
                case "AUTH":
                    return CommandName.AUTH;
                default:
                    return CommandName.UNKNOW;
            }
        }
    }
}
