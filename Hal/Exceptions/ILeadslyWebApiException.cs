namespace Hal.Exceptions
{
    interface ILeadslyWebApiException
    {
        string Type { get; }
        string Title { get; }
        int Status { get; }
        string Detail { get; }
        string Instance { get; }
    }
}
