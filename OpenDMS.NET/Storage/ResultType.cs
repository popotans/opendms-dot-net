using System;

namespace OpenDMS.Storage
{
    public enum ResultType
    {
        Failure = 0,
        Success,
        NotFound,
        IOError,
        SerializationError,
        InstantiationError,
        ResourceIsLocked,
        PermissionsError,
        LengthMismatch,
        Md5Mismatch,
        VersionMismatch
    }
}
