using System;

namespace OpenDMS.Security
{
    public enum AccessType
    {
        None = 0,
        Read = 1,
        Write = 2,
        Delete = 4,
        ReadWrite = Read & Write,
        All = ReadWrite & Delete
    }
}