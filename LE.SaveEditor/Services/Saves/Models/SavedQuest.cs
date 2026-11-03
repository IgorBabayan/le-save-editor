namespace LE.SaveEditor.Services.Saves.Models;

/// <summary>One entry of <c>savedQuests</c>: a quest the character started and the step it has reached.</summary>
/// <param name="QuestId"><c>questID</c>.</param>
/// <param name="QuestStepId"><c>questStepID</c>: the step the quest is at; the quest's last step once it is completed.</param>
public sealed record SavedQuest(int QuestId, int QuestStepId);
