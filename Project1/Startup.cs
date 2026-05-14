using Microsoft.OpenApi;
using AwsTest01.Application;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

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

            var region = Configuration["Cognito:Region"]!;
            var userPoolId = Configuration["Cognito:UserPoolId"]!;
            var clientId = Configuration["Cognito:ClientId"]!;

            var issuer = $"https://cognito-idp.{region}.amazonaws.com/{userPoolId}";

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = issuer;

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer,
                        ValidateLifetime = true,

                        // Cognito の access token は aud ではなく client_id を確認する
                        ValidateAudience = false,

                        NameClaimType = "username",
                        RoleClaimType = "cognito:groups"
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = context =>
                        {
                            var claims = context.Principal?.Claims;

                            var tokenUse = claims?.FirstOrDefault(c => c.Type == "token_use")?.Value;
                            var tokenClientId = claims?.FirstOrDefault(c => c.Type == "client_id")?.Value;

                            if (tokenUse != "access")
                            {
                                context.Fail("Access token is required.");
                                return Task.CompletedTask;
                            }

                            if (tokenClientId != clientId)
                            {
                                context.Fail("Invalid client_id.");
                                return Task.CompletedTask;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization();
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
                //c.OAuth2RedirectUrl("https://cphr17sdgi.execute-api.ap-northeast-1.amazonaws.com/Prod/swagger/oauth2-redirect.html");
            });

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
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