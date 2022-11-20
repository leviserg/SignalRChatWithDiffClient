using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.SignalR;
using SignalRChatWithDiffClient.Hubs;
using SignalRChatWithDiffClient.Providers;
using SignalRChatWithDiffClient.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// builder.Services.AddSignalR();
// install NuGet package Microsoft AspNetCore SignalR Protocols MessagePack for using MessagePack protocol
// profit - compact message (in comparison with uncompressed json - default) + quicker serialize/deserialize process - limits on data types + item counts
builder.Services.AddSignalR().AddMessagePackProtocol();

builder.Services.AddSingleton<IUserIdProvider, CustomUserProvider>();
builder.Services.AddHostedService<SubscriptionWorker>();
//builder.Services.AddCors();
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyCorsPolicy", builder =>
    {
        builder.SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
    });
});

builder.Services.AddResponseCompression(
    options => options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "application/octet-stream" })
    );

var app = builder.Build();

app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseRouting();
/*
app.UseCors(policy =>
{
    policy
        .SetIsOriginAllowed(origin => true)
        .AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials();
});
*/
app.UseCors("MyCorsPolicy");

app.MapHub<ChatHub>("/chat");

app.Run();
