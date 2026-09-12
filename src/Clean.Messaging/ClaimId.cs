namespace Clean.Messaging;

public static class ClaimId
{
    public static string Create()
    {
        return Guid.NewGuid().ToString("N");
    }
}