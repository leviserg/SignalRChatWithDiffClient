using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using SignalRChatWithDiffClient.Hubs;
using SignalRChatWithDiffClient.Providers;
using SignalRChatWithDiffClient.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, CustomUserProvider>();
builder.Services.AddHostedService<SubscriptionWorker>();
builder.Services.AddCors();
builder.Services.AddResponseCompression(
    options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" })
    );

var app = builder.Build();

app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(policy =>
{
    policy
        .SetIsOriginAllowed(origin => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});

app.MapHub<ChatHub>("/chat");

app.Run();
