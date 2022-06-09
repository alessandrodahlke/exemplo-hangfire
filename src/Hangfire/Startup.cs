using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hangfire.Storage.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;

namespace Hangfire
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Hangfire", Version = "v1" });
            });

            services.HangfireServiceConfig();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Hangfire v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.HangfireApplicationConfig();

            ExemploFireAndForgetJob();
            ExemploDelayedJobs();
            ExemploRecurringJobs();
            ExemploExecutarJobFilho();
            //ExemploFalharJob();
        }

        /// <summary>
        /// Jobs fire-and-forget são executados imediatamente e depois "esquecidos".
        /// Você ainda poderá voltar a executá-los manualmente sempre que quiser
        /// graças ao Dashboard e ao histórico do Hangfire.
        /// </summary>
        public void ExemploFireAndForgetJob()
        {
            BackgroundJob.Enqueue(() => Executar("JOB Fire-and-forget!"));
        }

        /// <summary>
        /// Jobs delayed são executados em uma data futura pré-definida.
        /// </summary>
        public void ExemploDelayedJobs()
        {
            BackgroundJob.Schedule(
                () => Executar("Job Delayed executado 2 minutos após o início da aplicação!"),
                TimeSpan.FromMinutes(2));
        }

        /// <summary>
        /// Reurring jobs permitem que você agende a execução de jobs recorrentes utilizando uma notação Cron.
        /// </summary>
        public void ExemploRecurringJobs()
        {
            RecurringJob.AddOrUpdate(
                "JOB-RECORRENTE",
                () => Executar("RecurringJob"),
                Cron.Minutely,
                TimeZoneInfo.Local);
        }

        /// <summary>
        /// Esta abordagem permite que você defina para a execução de um job iniciar
        /// apenas após a conclusão de um job pai.
        /// </summary>
        public void ExemploExecutarJobFilho()
        {
            var jobId = BackgroundJob.Enqueue(() => Executar("Job fire-and-forget PAI!"));
            BackgroundJob.ContinueJobWith(jobId, () => Executar($"Job fire-and-forget filho! (ContinueJobWith {jobId})"));
        }

        /// <summary>
        /// Quando um job falha o Hangfire irá adiciona-la a fila para executar novamente.
        /// Por padrão ele irá tentar reexecutar 10 vezes, você pode alterar este comportamento
        /// adicionando a seguinte configuração ao método ConfigureServices:
        /// <code>GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3, DelaysInSeconds = new int[] { 300 } });</code>
        /// </summary>
        public void ExemploFalharJob()
        {
            BackgroundJob.Enqueue(() => FalharJob());
        }

        public void FalharJob()
        {
            throw new Exception("Erro ao executar o Job...");
        }

        public void Executar(string job)
        {
            var dataExecucao = DateTime.Now;
            Console.WriteLine($"{job} em: {dataExecucao}");
        }
    }
}
