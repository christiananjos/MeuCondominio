using MeuCondominio.Application.OCR;
using MeuCondominio.Application.OCR.Strategies;
using MeuCondominio.Domain.Interfaces;
using MeuCondominio.Domain.Interfaces.Repositories;
using MeuCondominio.Infrastructure.Messaging;
using MeuCondominio.Infrastructure.Persistence;
using MeuCondominio.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ──────────────────────────────────────────────────────────
// Configurações
// ──────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("Supabase")
    ?? throw new InvalidOperationException("Connection string 'Supabase' não configurada.");

var supabaseJwtSecret = builder.Configuration["Supabase:JwtSecret"]
    ?? throw new InvalidOperationException("Supabase:JwtSecret não configurado.");

// ──────────────────────────────────────────────────────────
// Autenticação via JWT do Supabase
// ──────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(supabaseJwtSecret)),
            ValidateIssuer   = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew        = TimeSpan.FromSeconds(30)
        };
    });

builder.Services.AddAuthorization();

// ──────────────────────────────────────────────────────────
// MediatR — registra todos os Handlers automaticamente
// ──────────────────────────────────────────────────────────
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(
        typeof(MeuCondominio.Application.UseCases.Encomendas.RegistrarEncomenda.RegistrarEncomendaHandler).Assembly));

// ──────────────────────────────────────────────────────────
// OCR — Strategy + Factory
// Novos marketplaces: adicione apenas a nova Strategy aqui
// ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IEtiquetaOcrStrategy, MercadoLivreOcrStrategy>();
builder.Services.AddScoped<IEtiquetaOcrStrategy, AmazonOcrStrategy>();
builder.Services.AddScoped<IEtiquetaOcrStrategy, GenericOcrStrategy>(); // Fallback — deve ser o último
builder.Services.AddScoped<OcrStrategyFactory>();

// ──────────────────────────────────────────────────────────
// Repositórios (Infrastructure)
// ──────────────────────────────────────────────────────────
builder.Services.AddScoped<IEncomendaRepository>(
    _ => new EncomendaRepository(connectionString));
builder.Services.AddScoped<IMoradorRepository>(
    _ => new MoradorRepository(connectionString));
builder.Services.AddScoped<IWhatsAppConfigRepository>(
    _ => new WhatsAppConfigRepository(connectionString));

// ──────────────────────────────────────────────────────────
// WhatsApp Service (Infrastructure)
// ──────────────────────────────────────────────────────────
builder.Services.AddHttpClient("MetaWhatsApp", client =>
{
    client.Timeout = TimeSpan.FromSeconds(15);
});
builder.Services.AddScoped<IWhatsAppService, MetaWhatsAppService>();

// ──────────────────────────────────────────────────────────
// API
// ──────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "MeuCondomínio API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new()
    {
        In          = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Informe o token JWT do Supabase: Bearer {token}",
        Name        = "Authorization",
        Type        = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme      = "bearer"
    });
});

var app = builder.Build();

// ──────────────────────────────────────────────────────────
// Executa migrações SQL ao iniciar a aplicação.
// DbUp rastreia scripts já aplicados — é idempotente e seguro.
// ──────────────────────────────────────────────────────────
var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
MeuCondominio.Infrastructure.Persistence.DatabaseMigrationRunner
    .Executar(connectionString, startupLogger);

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
