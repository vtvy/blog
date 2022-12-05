using blog;
using blog.Areas.Database.Models;
using blog.Data;
using blog.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddConsole();
builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(cfg =>
{                   
    cfg.Cookie.Name = "vtvy";            
    cfg.IdleTimeout = new TimeSpan(0, 30, 0); 
});


builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("BlogDbContext") ?? throw new InvalidOperationException("Connection string 'BlogDbContext' not found.")));


// Configure for Route
builder.Services.Configure<RouteOptions>(options =>
{
    options.AppendTrailingSlash = false;        // Adding Trailing Slash / in URL ending
    options.LowercaseUrls = true;               // url lowercase
    options.LowercaseQueryStrings = false;      // not forcing lowercase query url
});

builder.Services.AddIdentity<BlogUser, IdentityRole>()
    .AddEntityFrameworkStores<BlogDbContext>()
    .AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password setting
    options.Password.RequireDigit = false; // not require digit and so on
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;

    // Configure Lockout - lock user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // lock 5 minutes
    options.Lockout.MaxFailedAccessAttempts = 5; // Fail 5 times
    options.Lockout.AllowedForNewUsers = true;

    // Configure User.
    options.User.AllowedUserNameCharacters = // user characters
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true; // Email is unique

    // Configure login
    options.SignIn.RequireConfirmedEmail = false; // Confirm email when sign
    options.SignIn.RequireConfirmedPhoneNumber = false; // Not confirm phone

});

// Configure Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    // options.Cookie.HttpOnly = true;  
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
    options.LoginPath = $"/login/";                                 // Url to login
    options.LogoutPath = $"/logout/";
    options.AccessDeniedPath = $"/khongduoctruycap.html";   // Redirect when user is not allowed to access
});
builder.Services.Configure<SecurityStampValidatorOptions>(options =>
{
    // Over 5s will validate again
    options.ValidationInterval = TimeSpan.FromSeconds(5);
});

builder.Services.AddOptions();
builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));

builder.Services.AddTransient<IEmailSender, SendMailService>();

builder.Services.AddAuthorization(options =>
{
    // User thỏa mãn policy khi có roleclaim: permission với giá trị manage.user
    options.AddPolicy("AdminDropdown", policy =>
    {
        policy.RequireClaim("permission", "manage.user");
    });

    options.AddPolicy("ViewManageMenu", policy =>
    {
        //policy.RequireAuthenticatedUser();
        policy.RequireRole(RoleName.Administrator);
    });
});

builder.Services.AddAuthentication()
    .AddGoogle(googleOptions =>
    {
        // Đọc thông tin Authentication:Google từ appsettings.json
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");

        // Thiết lập ClientID và ClientSecret để truy cập API google
        googleOptions.ClientId = googleAuthNSection["ClientId"];
        googleOptions.ClientSecret = googleAuthNSection["ClientSecret"];
        // Cấu hình Url callback lại từ Google (không thiết lập thì mặc định là /signin-google)
        googleOptions.CallbackPath = "/dang-nhap-tu-google";

    });


// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

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

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    endpoints.MapControllerRoute(
                     name: "MyArea",
                     pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");



    // Đến Razor Page    
    endpoints.MapRazorPages();
});

app.UseStatusCodePages(handleError =>
{
    handleError.Run(async context =>
    {
        await context.Response.WriteAsync(
            @$"<html lang=""en"">
                <head>
                    <meta charset=""UTF-8"" />
                    <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"" />
                    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
                    <title>Bootstrap 5 404 Error Page</title>
                    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.1.3/dist/css/bootstrap.min.css"" rel=""stylesheet"">
                </head>
                <body>
                    <div class=""d-flex align-items-center justify-content-center vh-100"">
                        <div class=""text-center"">
                            <h1 class=""display-1 fw-bold"">404</h1>
                            <p class=""fs-3""> <span class=""text-danger"">Opps!</span> Page not found.</p>
                            <p class=""lead"">
                                The page you’re looking for doesn’t exist.
                              </p>
                            <a href=""/"" class=""btn btn-primary"">Go Home</a>
                        </div>
                    </div>
                </body>
               </html>"
            );
    });
});

app.Run();
