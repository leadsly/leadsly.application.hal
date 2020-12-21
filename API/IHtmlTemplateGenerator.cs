namespace API
{
    public interface IHtmlTemplateGenerator
    {
        string GenerateBodyFor(EmailTemplateTypes templateType);
    }
}
