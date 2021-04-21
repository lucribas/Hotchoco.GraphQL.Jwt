using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hotchoco.GraphQL.Jwt.GraphQLModels.ObjectTypes;
using Hotchoco.GraphQL.Jwt.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Hotchoco.GraphQL.Jwt
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{

			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
			.AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidIssuer = Configuration.GetSection("TokenSettings").GetValue<string>("Issuer"),
					ValidateIssuer = true,

					ValidAudience = Configuration.GetSection("TokenSettings").GetValue<string>("Audience"),
					ValidateAudience = true,

					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration.GetSection("TokenSettings").GetValue<string>("Key"))),

					ValidateIssuerSigningKey = true
				};
			});

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hotchoco.GraphQL.Jwt", Version = "v1" });
			});

			services.AddGraphQLServer()
			.AddQueryType<QuerObjectType>()
			.AddMutationType<MutationObjectType>()
			.AddAuthorization();

			// Add Policy-Based Roles Authorization: Role
			services.AddAuthorization(options =>
			{
				options.AddPolicy("user-policy", policy =>
				{
					policy.RequireRole(new[] { "user", "admin" });
				});
			});

			// Add Policy-Based Roles Authorization: Claim
			services.AddAuthorization(options =>
			{
				options.AddPolicy("country-policy", policy =>
				{
					policy.RequireClaim("usercountry", "Brazil", "India", "USA");
				});
			});

			services.Configure<TokenSettings>(Configuration.GetSection("TokenSettings"));
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				// app.UseSwagger();
				// app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hotchoco.GraphQL.Jwt v1"));

			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGraphQL();
				endpoints.MapControllers();
			});
		}
	}
}
