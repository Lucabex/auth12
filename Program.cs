using Microsoft.EntityFrameworkCore;
using auth12.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpClient();
builder.Services.AddControllers();
builder.Services.AddDbContext<AppDbContext>(options=>
options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
)

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();
