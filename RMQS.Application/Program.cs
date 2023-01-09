using RabbitMQ.Client;
using RMQS.Application.Hubs;
using RMQS.Application.Queue;

namespace RMQS.Application
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConnectionFactory factory = new()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest",
                Port = 5672
            };

            builder.Services.AddSingleton<IConnectionFactory>(factory);
            builder.Services.AddSingleton<IQueuePublisher, QueuePublisher>();

            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            builder.Services.AddHostedService<QueueListener>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<RabbitMQHub>("/rabbitmqhub");
            app.Run();
        }
    }
}