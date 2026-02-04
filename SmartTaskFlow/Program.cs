using Microsoft.EntityFrameworkCore;
using SmartTaskFlow.Data;

var builder = WebApplication.CreateBuilder(args);

// Ajouter les services au conteneur
builder.Services.AddControllersWithViews();

// Configurer la connexion à la base de données
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Ajouter DistributedMemoryCache 
builder.Services.AddDistributedMemoryCache();

// Ajouter les sessions (pour stocker l'utilisateur connecté)
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.Name = ".SmartTaskFlow.Session";
});

// Ajouter HttpContextAccessor (pour accéder à la session)
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure le pipeline HTTP
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Activer les sessions
app.UseSession();

app.UseAuthorization();

// Route par défaut vers Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();