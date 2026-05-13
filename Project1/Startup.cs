using Microsoft.OpenApi;
using AwsTest01.Application;
using Microsoft.OpenApi.Models;

namespace Project1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            services.AddScoped<ITestInterface, TestService>();

            // Swagger生成サービスを追加
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "My API",
                    Version = "v1"
                });
                c.EnableAnnotations();

                var cognitoDomain = Configuration["Cognito:Domain"]!;

                c.AddSecurityDefinition("cognito", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Description = "Cognito Authorization Code Flow with PKCE",
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{cognitoDomain}/oauth2/authorize"),
                            TokenUrl = new Uri($"{cognitoDomain}/oauth2/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "openid", "OpenID Connect" },
                                { "email", "Email" },
                                { "profile", "Profile" }
                            }
                        }
                    }
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "cognito"
                            }
                        },
                        new[] { "openid", "email", "profile" }
                    }
                });


            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Swaggerミドルウェアを有効化
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "My API V1");
                c.RoutePrefix = string.Empty; // ルートURLでSwagger UIを表示

                // タブの初期状態制御
                c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                c.DefaultModelsExpandDepth(-1);
                c.EnableFilter();

                // Cognito認証設定
                c.OAuthClientId(Configuration["Cognito:ClientId"]);
                c.OAuthAppName("My API Swagger UI");

                // Authorization Code + PKCE
                c.OAuthUsePkce();
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Welcome to running ASP.NET Core on AWS Lambda");
                });
            });
        }
    }
}