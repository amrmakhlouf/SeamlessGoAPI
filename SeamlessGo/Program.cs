
using Microsoft.EntityFrameworkCore;
using SeamlessGo.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register the repository
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderLineRepository, OrderLineRepository>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionLineRepository, TransactionLineRepository>();
builder.Services.AddScoped<IitemRepository, ItemRepository>();
builder.Services.AddScoped<IitemPackRepository, ItemPackRepository>();
builder.Services.AddScoped<IPaymentsRepository, PaymentRepository>();
builder.Services.AddScoped<IPaymentAllocationsRepository, PaymentAllocationsRepository>();
builder.Services.AddScoped<IDownPaymentsRepository, DownPaymentRepository>();
builder.Services.AddScoped<IDownPaymentAllocationsRepository, DownPaymentAllocationsRepository>();
builder.Services.AddScoped<IUsersRepository, UsersRepository>();
builder.Services.AddScoped<IRouteRepository, RouteRepository>(); 
builder.Services.AddScoped<IStockLocationRepository, StockLocationRepository>();

builder.Services.AddAutoMapper(typeof(Program));

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add CORS if needed
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();