using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ESD_EDI_BE.CustomAttributes;
using System.Collections;
using System.Text;

namespace ESD_EDI_BE.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAll]
    public class GenerateController : ControllerBase
    {
        [HttpPost]
        public IActionResult Generate()
        {
            StringBuilder sb = new StringBuilder();
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "ESD_EDI_BE");
            foreach (Type t in assembly.GetTypes())
            {

                if (t.GetCustomAttributes(false).FirstOrDefault(a => a.GetType().Name == "JSGenerateAttribute") != null)
                {
                    sb.AppendLine($"export const {t.Name} = {{");
                    foreach (var prop in t.GetProperties())
                    {
                        if (typeof(IEnumerable).IsAssignableFrom(prop.PropertyType) && prop.PropertyType != typeof(string) && prop.PropertyType != typeof(byte[]))
                        {
                            sb.AppendLine($"{prop.Name}: [],");
                        }
                        else
                        {
                            if (prop.PropertyType == typeof(long)|| prop.PropertyType == typeof(int))
                            {
                                sb.AppendLine($"{prop.Name}: 0,");
                            }
                            else
                            {
                                if (prop.Name == "forRoot" || prop.Name == "forApp")
                                {
                                    sb.AppendLine($"{prop.Name}: false,");
                                }
                                else
                                {
                                    if (prop.PropertyType == typeof(string))
                                    {
                                        sb.AppendLine($"{prop.Name}: '',");
                                    }
                                    else
                                    {
                                        sb.AppendLine($"{prop.Name}: null,");
                                    }
                                }
                            }
                        }

                    }
                    sb.AppendLine(@"};");
                }

            }

            sb.AppendLine();

            var text = sb.ToString();


            return Ok(text);
        }
    }
}
