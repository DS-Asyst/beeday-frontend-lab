using System.Collections.ObjectModel;

namespace BeeDayLab.Web.Components.DesignSystem.Icons;

public static class BeeDayIconRegistry
{
    public const string SpritePath = "/icons/sprite.svg";
    public const BeeDayIconName DefaultFallback = BeeDayIconName.Warning;

    private static readonly IReadOnlyDictionary<BeeDayIconName, BeeDayIconDefinition> Definitions =
        new ReadOnlyDictionary<BeeDayIconName, BeeDayIconDefinition>(
            new Dictionary<BeeDayIconName, BeeDayIconDefinition>
            {
                [BeeDayIconName.Add] = Define("add", "lucide/actions/add.svg", BeeDayIconCategory.Actions, "Add"),
                [BeeDayIconName.Edit] = Define("edit", "lucide/actions/edit.svg", BeeDayIconCategory.Actions, "Edit"),
                [BeeDayIconName.Delete] = Define("delete", "lucide/actions/delete.svg", BeeDayIconCategory.Actions, "Delete"),
                [BeeDayIconName.Save] = Define("save", "lucide/actions/save.svg", BeeDayIconCategory.Actions, "Save"),
                [BeeDayIconName.Close] = Define("close", "lucide/actions/close.svg", BeeDayIconCategory.Actions, "Close"),
                [BeeDayIconName.Search] = Define("search", "lucide/actions/search.svg", BeeDayIconCategory.Actions, "Search"),
                [BeeDayIconName.More] = Define("more", "lucide/actions/more.svg", BeeDayIconCategory.Actions, "More options"),
                [BeeDayIconName.Check] = Define("check", "lucide/feedback/check.svg", BeeDayIconCategory.Feedback, "Check"),
                [BeeDayIconName.Warning] = Define("warning", "lucide/feedback/warning.svg", BeeDayIconCategory.Feedback, "Warning"),
                [BeeDayIconName.Information] = Define("information", "lucide/feedback/information.svg", BeeDayIconCategory.Feedback, "Information"),
                [BeeDayIconName.Settings] = Define("settings", "lucide/system/settings.svg", BeeDayIconCategory.System, "Settings"),
                [BeeDayIconName.Lock] = Define("lock", "lucide/system/lock.svg", BeeDayIconCategory.System, "Lock"),
                [BeeDayIconName.Account] = Define("account", "lucide/profile/account.svg", BeeDayIconCategory.Navigation, "Account"),
                [BeeDayIconName.Language] = Define("language", "lucide/navigation/language.svg", BeeDayIconCategory.Navigation, "Language"),
                [BeeDayIconName.ChevronDown] = Define("chevron-down", "lucide/navigation/chevron-down.svg", BeeDayIconCategory.Navigation, "Expand"),
                [BeeDayIconName.ChevronLeft] = Define("chevron-left", "lucide/navigation/chevron-left.svg", BeeDayIconCategory.Navigation, "Previous"),
                [BeeDayIconName.ChevronRight] = Define("chevron-right", "lucide/navigation/chevron-right.svg", BeeDayIconCategory.Navigation, "Next"),
                [BeeDayIconName.Library] = Define("library", "lucide/books/library.svg", BeeDayIconCategory.Navigation, "Library"),
                [BeeDayIconName.Daily] = Define("daily", "lucide/navigation/daily.svg", BeeDayIconCategory.Navigation, "Daily"),
                [BeeDayIconName.Profile] = Define("profile", "lucide/profile/profile.svg", BeeDayIconCategory.Navigation, "Profile"),
                [BeeDayIconName.Donate] = Define("donate", "lucide/navigation/donate.svg", BeeDayIconCategory.Navigation, "Donate"),
                [BeeDayIconName.Logout] = Define("logout", "lucide/navigation/logout.svg", BeeDayIconCategory.Navigation, "Logout"),
                [BeeDayIconName.Menu] = Define("menu", "lucide/navigation/menu.svg", BeeDayIconCategory.Navigation, "Menu"),
                [BeeDayIconName.Support] = Define("support", "lucide/navigation/support.svg", BeeDayIconCategory.Navigation, "Support"),
                [BeeDayIconName.Facebook] = Define("facebook", "devicon/social/facebook.svg", BeeDayIconCategory.Social, "Facebook"),
                [BeeDayIconName.Instagram] = Define("instagram", "official-brand/social/instagram.svg", BeeDayIconCategory.Social, "Instagram"),
                [BeeDayIconName.YouTube] = Define("youtube", "official-brand/social/youtube.svg", BeeDayIconCategory.Social, "YouTube"),
                [BeeDayIconName.X] = Define("x", "official-brand/social/x.svg", BeeDayIconCategory.Social, "X"),
                [BeeDayIconName.LinkedIn] = Define("linkedin", "devicon/social/linkedin.svg", BeeDayIconCategory.Social, "LinkedIn"),
                [BeeDayIconName.GitHub] = Define("github", "devicon/social/github.svg", BeeDayIconCategory.Social, "GitHub"),
                [BeeDayIconName.Habit] = Define("habit", "lucide/habits/habit.svg", BeeDayIconCategory.Activities, "Habit"),
                [BeeDayIconName.RecurringTask] = Define("recurring-task", "lucide/tasks/recurring-task.svg", BeeDayIconCategory.Activities, "Recurring task"),
                [BeeDayIconName.Project] = Define("project", "lucide/projects/project.svg", BeeDayIconCategory.Activities, "Project"),
                [BeeDayIconName.Todo] = Define("todo", "lucide/tasks/todo.svg", BeeDayIconCategory.Activities, "To-Do"),
                [BeeDayIconName.Complete] = Define("complete", "lucide/actions/complete.svg", BeeDayIconCategory.Actions, "Complete"),
                [BeeDayIconName.Filter] = Define("filter", "lucide/actions/filter.svg", BeeDayIconCategory.Actions, "Filter"),
                [BeeDayIconName.Calendar] = Define("calendar", "lucide/actions/calendar.svg", BeeDayIconCategory.Actions, "Calendar"),
                [BeeDayIconName.Repeat] = Define("repeat", "lucide/actions/repeat.svg", BeeDayIconCategory.Actions, "Repeat"),
                [BeeDayIconName.Tag] = Define("tag", "lucide/actions/tag.svg", BeeDayIconCategory.Actions, "Tag"),
                [BeeDayIconName.Cancel] = Define("cancel", "lucide/actions/cancel.svg", BeeDayIconCategory.Actions, "Cancel"),
                [BeeDayIconName.Featured] = Define("featured", "lucide/feedback/featured.svg", BeeDayIconCategory.Feedback, "Featured"),
                [BeeDayIconName.Progress] = Define("progress", "lucide/statistics/progress.svg", BeeDayIconCategory.Statistics, "Progress"),
                [BeeDayIconName.Success] = Define("success", "lucide/feedback/success.svg", BeeDayIconCategory.Feedback, "Success"),
                [BeeDayIconName.ValidationError] = Define("validation-error", "lucide/feedback/validation-error.svg", BeeDayIconCategory.Feedback, "Validation error"),
                [BeeDayIconName.Loading] = Define("loading", "lucide/feedback/loading.svg", BeeDayIconCategory.Feedback, "Loading"),
                [BeeDayIconName.Select] = Define("select", "lucide/forms/select.svg", BeeDayIconCategory.Forms, "Select"),
                [BeeDayIconName.CheckboxUnchecked] = Define("checkbox-unchecked", "lucide/forms/checkbox-unchecked.svg", BeeDayIconCategory.Forms, "Unchecked"),
                [BeeDayIconName.CheckboxChecked] = Define("checkbox-checked", "lucide/forms/checkbox-checked.svg", BeeDayIconCategory.Forms, "Checked"),
                [BeeDayIconName.Experience] = Define("experience", "lucide/statistics/experience.svg", BeeDayIconCategory.Statistics, "Experience"),
                [BeeDayIconName.Level] = Define("level", "lucide/statistics/level.svg", BeeDayIconCategory.Statistics, "Level"),
                [BeeDayIconName.Wallet] = Define("wallet", "lucide/statistics/wallet.svg", BeeDayIconCategory.Statistics, "Wallet"),
                [BeeDayIconName.Income] = Define("income", "lucide/statistics/income.svg", BeeDayIconCategory.Statistics, "Income"),
                [BeeDayIconName.Expense] = Define("expense", "lucide/statistics/expense.svg", BeeDayIconCategory.Statistics, "Expense"),
                [BeeDayIconName.Statistics] = Define("statistics", "lucide/statistics/statistics.svg", BeeDayIconCategory.Statistics, "Statistics"),
                [BeeDayIconName.TrendUp] = Define("trend-up", "lucide/statistics/trend-up.svg", BeeDayIconCategory.Statistics, "Upward trend"),
                [BeeDayIconName.TrendDown] = Define("trend-down", "lucide/statistics/trend-down.svg", BeeDayIconCategory.Statistics, "Downward trend"),
                [BeeDayIconName.Completed] = Define("completed", "lucide/statistics/completed.svg", BeeDayIconCategory.Statistics, "Completed"),
                [BeeDayIconName.Pending] = Define("pending", "lucide/statistics/pending.svg", BeeDayIconCategory.Statistics, "Pending")
            });

    public static IReadOnlyDictionary<BeeDayIconName, BeeDayIconDefinition> All => Definitions;

    public static BeeDayIconDefinition Resolve(BeeDayIconName name)
    {
        if (Definitions.TryGetValue(name, out var definition))
        {
            return definition;
        }

        return Definitions[DefaultFallback];
    }

    public static bool TryGet(BeeDayIconName name, out BeeDayIconDefinition definition) =>
        Definitions.TryGetValue(name, out definition!);

    private static BeeDayIconDefinition Define(
        string symbolId,
        string assetPath,
        BeeDayIconCategory category,
        string semanticName) =>
        new(symbolId, $"/icons/{assetPath}", category, semanticName, semanticName, DefaultFallback);
}
