using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GW.ApiLoader.Utils.Swagger
{
    public class SwaggerIgnoreFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {

            foreach (var ignoreApi in swaggerDoc.Paths)
            {
                //忽略了重复的options
                ignoreApi.Value.Operations.Remove(OperationType.Options);
            }
        }
    }
}
