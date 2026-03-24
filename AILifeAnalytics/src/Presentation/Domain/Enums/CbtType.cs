namespace AILifeAnalytics.Domain.Enums
{
    public enum CbtRecordType
    {
        ThoughtDiary = 0,  // ABC дневник мыслей
        CognitiveDistortion = 1, // анализ искажений
        BehavioralExperiment = 2 // поведенческий эксперимент
    }

    public enum EmotionType
    {
        Anxiety = 0,  // тревога
        Sadness = 1,  // грусть
        Anger = 2,  // злость
        Shame = 3,  // стыд
        Guilt = 4,  // вина
        Fear = 5,  // страх
        Loneliness = 6,  // одиночество
        Frustration = 7, // разочарование
        Other = 8
    }

    public enum CognitiveDistortionType
    {
        Catastrophizing = 0,  // катастрофизация
        BlackAndWhiteThinking = 1,  // чёрно-белое мышление
        MindReading = 2,  // чтение мыслей
        FortuneTelling = 3,  // предсказание будущего
        Overgeneralization = 4,  // сверхобобщение
        Labeling = 5,  // навешивание ярлыков
        ShouldStatements = 6,  // долженствование
        PersonalizationBlame = 7,  // персонализация
        EmotionalReasoning = 8,  // эмоциональные рассуждения
        Filtering = 9   // ментальный фильтр
    }
}
