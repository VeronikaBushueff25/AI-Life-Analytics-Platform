namespace AILifeAnalytics.Domain.Enums;

public enum AchievementType
{
    /// <summary>
    /// Стрики
    /// </summary>
    Streak7Days = 0,
    Streak30Days = 1,
    Streak100Days = 2,

    /// <summary>
    /// Продуктивность
    /// </summary>
    ProductivityRecord = 10,  // личный рекорд
    ProductivitySuperstar = 11,  // >90 три дня подряд
    ProductivityConsistent = 12, // >70 семь дней подряд

    /// <summary>
    /// Сон
    /// </summary>
    SleepMaster = 20,  // 8ч+ пять дней подряд
    NightOwl = 21,  // <6ч пять раз (антидостижение)
    SleepRecovered = 22,  // после недосыпания вернулся к норме

    /// <summary>
    /// Работа и баланс
    /// </summary>
    WorkLifeBalance = 30, // баланс >80 три дня подряд
    NoOverwork = 31, // ни одной переработки за неделю
    DeepFocus = 32, // фокус 9-10 пять дней подряд

    /// <summary>
    /// КПТ
    /// </summary>
    FirstCbt = 40, // первая КПТ-сессия
    CbtPractitioner = 41, // 10 завершённых сессий
    CbtMaster = 42, // 30 завершённых сессий
    MindShift = 43, // снижение эмоции >50% в одной сессии

    /// <summary>
    /// Профиль
    /// </summary>
    FirstProfile = 50, // первый AI-профиль
    ProfileEvolved = 51, // профиль изменился (другой архетип)

    /// <summary>
    /// Общие
    /// </summary>
    FirstEntry = 60, // первая запись
    Veteran = 61, // 50 записей всего
    Legend = 62, // 200 записей всего

    /// <summary>
    /// Burnout
    /// </summary>
    BurnoutPrevented = 70, // снизил риск с High до Low за неделю
}