using Graduation_Project.Context;
using Graduation_Project.Service;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stripe;
using System.Text;
using Microsoft.OpenApi.Models;


namespace Graduation_Project
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDBContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

			builder.Services.AddScoped<ChatService>();
			builder.Services.AddScoped<AIQueryService>();
			builder.Services.AddScoped<SearchQueryParserService>();
			builder.Services.AddHostedService<RentalNotificationService>();
			builder.Services.AddScoped<HospitalSearchService>();


			StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

			builder.Services.AddScoped<IPaymentService, PaymentService>();



			builder.Services.AddAuthentication(
            (options) =>
            {
	           options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(
            (options) => {
	           options.TokenValidationParameters = new TokenValidationParameters
	           {
		          ValidateIssuer = true,
		          ValidateAudience = true,
		          ValidateLifetime = true,
		          ValidateIssuerSigningKey = true,
		          ValidIssuer = builder.Configuration["Jwt:Issuer"],
		          ValidAudience = builder.Configuration["Jwt:Audience"],
		          IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
	           };
              }
            );
			builder.Services.AddAuthorization();


			// Add services to the container.

			builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();


			builder.Services.AddCors(options =>
			{
				options.AddPolicy("AllowAll",
					policy =>
					{
						policy.AllowAnyOrigin()
							  .AllowAnyMethod()
							  .AllowAnyHeader();
					});
			});

			//builder.Services.AddCors(options =>
			//{
			//	options.AddPolicy("ReactPolicy",
			//		policy =>
			//		{
			//			policy.WithOrigins("http://localhost:3000") // React URL
			//				  .AllowAnyHeader()
			//				  .AllowAnyMethod();
			//		});
			//});

			//app.UseCors("ReactPolicy");


			builder.Services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });

				c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Name = "Authorization",
					Type = SecuritySchemeType.Http,
					Scheme = "Bearer",
					BearerFormat = "JWT",
					In = ParameterLocation.Header,
					Description = "Enter JWT like: Bearer {token}"
				});

				c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
			});


			var app = builder.Build();


			//// Configure the HTTP request pipeline.
			//if (app.Environment.IsDevelopment())
			//{
			//    app.UseSwagger();
			//    app.UseSwaggerUI();
			//}

			app.UseSwagger();
			app.UseSwaggerUI();

			app.UseHttpsRedirection();

			app.UseCors("AllowAll");
			app.UseAuthentication();
			app.UseAuthorization();

			app.UseStaticFiles();

			app.MapControllers();

            app.Run();
        }
    }
}
