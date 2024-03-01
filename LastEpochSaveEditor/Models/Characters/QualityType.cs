namespace LastEpochSaveEditor.Models.Characters;

[Flags]
public enum QualityType
{
	Basic = 0,
	Magic = 1,
	Rare = 3,
	Exalted = 4,
	Unique = 7,
	Set = 8,
	Legendary = 9,
	NotUniqueOrSet = Basic | Magic | Rare | Exalted
}