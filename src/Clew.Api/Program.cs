using System.Text.Json;
using System.Text.Json.Serialization;
using Clew.Application.Extensions;
using Clew.Infrastructure.Extensions;
using Microsoft.AspNetCore.Diagnostics;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

builder.Services
    .AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = context =>
        {
            var exceptionHandlerFeature = context.HttpContext.Features.Get<IExceptionHandlerFeature>();
            if (exceptionHandlerFeature?.Error is not null)
            {
                context.ProblemDetails.Detail = exceptionHandlerFeature.Error.Message;
            }
        };
    })
    .AddApplication()
    .AddInfrastructure(configuration)
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else app.UseExceptionHandler();

app.UseStatusCodePages();

app.MapControllers();

app.Run();