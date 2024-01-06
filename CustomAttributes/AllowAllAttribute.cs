namespace ESD_EDI_BE.CustomAttributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AllowAllAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class JSGenerateAttribute : Attribute
    {
    }

}
