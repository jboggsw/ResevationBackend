using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using ResevationBackend.Database;
using ResevationBackend.Models;
using ResevationBackend.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ReservationBackendContext>(options =>
    options.UseInMemoryDatabase("ReservationDb"));

builder.Services.AddScoped<IReservationService, ReservationService>();

builder.Services.AddControllers();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Reservation API", Version = "v1" });
});

var app = builder.Build();

// Seed the database with initial data
//need a better way to seed the database 
SeedDatabase(app);

app.UseRouting();

//app.UseAuthorization();

app.MapControllers();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Reservation API V1");
});

app.Run();

void SeedDatabase(WebApplication app)
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var context = services.GetRequiredService<ReservationBackendContext>();

        var provider = new Provider
        {
            Name = "Dr. Jekyll",
        };
        context.Providers.Add(provider);
        context.SaveChanges();

        var slot1 = new AppointmentSlot
        {
            ProviderId = provider.Id,
            StartTime = DateTime.Today.AddHours(9),
            EndTime = DateTime.Today.AddHours(9).AddMinutes(15),
            IsReserved = false
        };
        var slot2 = new AppointmentSlot
        {
            ProviderId = provider.Id,
            StartTime = DateTime.Today.AddHours(48),
            EndTime = DateTime.Today.AddHours(48).AddMinutes(15),
            IsReserved = false
        };
        context.AppointmentSlots.AddRange(slot1, slot2);
        context.SaveChanges();
    }

}