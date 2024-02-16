namespace LastEpochSaveEditor.Models.Characters
{
	public record class AffixInfo(int Tier, int Id, int Value)
	{
        public string Name { get; set; }
    }
	
}
