
namespace OpenDMS.Storage.Security
{
    [System.Flags]
    public enum PermissionType : int
    {
        None = 0x0,

        // Resource ONLY

        GetReadOnlyResource = 0x00100,
        CheckoutResource = 0x00101,
        CreateNewResource = 0x00110,
        ModifyResource = 0x00111,
        AllResource = GetReadOnlyResource | CheckoutResource | CreateNewResource | ModifyResource,

        // Version ONLY

        GetReadOnlyVersion = 0x01000,
        CheckoutVersion = 0x01001,
        CreateNewVersion = 0x01010,
        ModifyVersion = 0x01011,
        AllVersion = GetReadOnlyVersion | CheckoutVersion | CreateNewVersion | ModifyVersion,

        All = AllResource | AllVersion
    }
}
