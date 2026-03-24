namespace AILifeAnalytics.Domain.Entities
{
    /// <summary>
    /// AI-профиль личности пользователя
    /// </summary>
    public class PersonalityProfile
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid UserId { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Период анализа
        /// </summary>
        public DateTime PeriodFrom { get; set; }
        public DateTime PeriodTo { get; set; }
        public int DaysAnalyzed { get; set; }

        /// <summary>
        /// Архетип — самая интересная часть
        /// </summary>
        public string ArchetypeName { get; set; } = string.Empty; 
        public string ArchetypeDescription { get; set; } = string.Empty; 
        public string ArchetypeEmoji { get; set; } = string.Empty; 

        /// <summary>
        /// Паттерны
        /// </summary>
        public string PeakPerformancePattern { get; set; } = string.Empty; 
        public string EnergyPattern { get; set; } = string.Empty; 
        public string StressPattern { get; set; } = string.Empty; 

        /// <summary>
        /// Суперсилы и уязвимости
        /// </summary>
        public string Superpowers { get; set; } = "[]"; 
        public string Vulnerabilities { get; set; } = "[]"; 
        public string Recommendations { get; set; } = "[]"; 

        /// <summary>
        /// Числовые инсайты
        /// </summary>
        public double OptimalSleepHours { get; set; } 
        public double OptimalWorkHours { get; set; }  
        public string MostProductiveDayOfWeek { get; set; } = string.Empty;
        public double CorrelationSleepFocus { get; set; }
        public double CorrelationStressMood { get; set; }

        /// <summary>
        /// Прогноз
        /// </summary>
        public string ForecastText { get; set; } = string.Empty;
        public string ForecastRisk { get; set; } = string.Empty;
        public string FullAnalysis { get; set; } = string.Empty;

        /// <summary>
        /// Navigation
        /// </summary>
        public User? User { get; set; }
    }
}
