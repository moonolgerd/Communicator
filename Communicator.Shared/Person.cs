using ProtoBuf;
using System;

namespace Communicator.Shared
{
    [ProtoContract]
    public class Person
    {
        [ProtoMember(1)]
        public string FirstName { get; set; }

        [ProtoMember(2)]
        public string LastName { get; set; }
    }
}
