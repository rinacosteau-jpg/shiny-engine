using Articy.World_Of_Red_Moon.GlobalVariables;

public sealed class StealFromRuQuestWrapper : QuestWrapper
{
    public StealFromRuQuestWrapper() : base("stealFromRu")
    {
        SetStageDescription(1, "Ратко попросил украсть некий магический артефакт и я согласилась. Я не знаю, почему я согласилась.");
        SetStageDescription(2, "Ратко попросил украсть некий магический артефакт и я согласилась. Я не знаю, почему я согласилась, а теперь никто и не помнит, что я соглашалась. Что ж, я всё ещё могу попытаться его украсть. Зачем-то.");
        SetStageDescription(3, "У меня не вышло \"присвоить\" артефакт. Ожидаемо. Если я не хочу, чтобы она разорвала меня на куски, лучше оставить попытки. А я не хочу.");
        SetStageDescription(4, "У меня не вышло \"присвоить\" артефакт, но... Я ведь могу попытаться снова в следующем витке петли. (Я всё ещё не понимаю, зачем я это делаю).");
        SetStageDescription(5, "У меня есть кубы! Чем бы они ни были. Пора вернуться к Ратко и обсудить награду. Надеюсь, он расскажет что-то полезное.");
        SetStageDescription(6, "У меня есть кубы! Чем бы они ни были. Я бы могла вернуться к Ратко и обсудить награду... Только вот он ничего не вспомнит. Ладно, что-нибудь придумаю. Зря я что ли этим занималась?");
        SetStageDescription(7, "Теперь Ратко отвечает на мои вопросы. Точнее, отвечает на те вопросы, на которые хочет отвечать. Это лучше, чем ничего.");
        SetStageDescription(8, "Если я хочу, чтобы Ратко снова мне помогал, я должна снова принести ему эти кубы. Что ж, я уже знаю, как их получить.");
        AddStagesToAdvanceOnLoopReset(1, 7);
        MarkStageAsFailed(3, 4);
        MarkStageAsCompleted(7);
    }

    public override int ProcessStageFromArticy(QuestManager.Quest quest, int stage)
    {
        if (stage == 3)
        {
            var ps = ArticyGlobalVariables.Default?.PS;
            if (ps != null && ps.loopCounter > 0)
                return 4;
        }

        if (stage == 5)
        {
            if (quest?.Stage == 1)
                return 5;

            return 6;
        }

        return base.ProcessStageFromArticy(quest, stage);
    }

    public override void OnLoopReset(QuestManager.Quest quest)
    {
        if (quest == null)
            return;

        bool wasFailed = quest.State == QuestState.Failed;

        base.OnLoopReset(quest);

        if (wasFailed)
        {
            quest.Stage = 2;
            quest.State = QuestState.Active;
        }
    }
}
