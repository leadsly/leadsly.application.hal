namespace API.Exceptions
{
    interface IOdmWebApiException
    {
        string Type { get; }
        string Title { get; }
        int Status { get; }
        string Detail { get; }
        string Instance { get; }
    }
}
