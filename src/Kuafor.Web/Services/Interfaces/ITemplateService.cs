namespace Kuafor.Web.Services.Interfaces;

public interface ITemplateService
{
    string GetTemplate(string templateName);
    Task<string> RenderTemplateAsync(string templateName, object model);
}