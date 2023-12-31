﻿using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using ProjectManager.Shared.Dto;
using ProjectManager.Shared.Interfaces;

namespace ProjectManager.Client.Services {
    public class ReportService : IReport {
        private const String EntityName = "report";
        private readonly HttpClient _http;

        public ReportService(HttpClient http) {
            _http = http;
        }
        
                                                                        public async Task<ForecastReportDto> Forecast(DateTime startDate, DateTime endDate, Guid companyId) {
            ForecastReportDto? forecast = await _http.GetFromJsonAsync<ForecastReportDto>($"api/report/forecast?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}&companyId={companyId}");
            if (forecast == null) throw new ApplicationException("Error while getting forecast report");
            return forecast;
        }

                                                                public async Task<UserProductivityDto> UserProductivity(String userId, Guid companyId) {
            UserProductivityDto? userReport = await _http.GetFromJsonAsync<UserProductivityDto>($"api/report/user?userId={userId}&companyId={companyId}");
            if (userReport == null) throw new ApplicationException("Error while getting user report");
            return userReport;
        }
    }
}
