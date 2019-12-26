using LrcLyrics.BackEnd.Services;
using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace LrcLyrics.BackEnd.Middlewares
{
    public class VisitorMiddleWare
    {
        private readonly RequestDelegate requestDelegate;
        private readonly StatisticsService statisticsService;

        public VisitorMiddleWare(RequestDelegate _requestDelegate)
        {
            requestDelegate = _requestDelegate;
            statisticsService = new StatisticsService();
        }

        public async Task Invoke(HttpContext context)
        {
            statisticsService.IncrementVisit();

            await requestDelegate(context);
        }
    }
}
