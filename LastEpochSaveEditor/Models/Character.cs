using LastEpochSaveEditor.Models.Characters;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LastEpochSaveEditor.Models
{
    public class Character
	{
		[JsonProperty("partitionKey")]
		public object PartitionKey { get; set; }

		[JsonProperty("characterName")]
		public string CharacterName { get; set; }

		[JsonProperty("level")]
		public int Level { get; set; }

		[JsonProperty("slot")]
		public int Slot { get; set; }

		[JsonProperty("currentExp")]
		public int CurrentExp { get; set; }

		[JsonProperty("hardcore")]
		public bool Hardcore { get; set; }

		[JsonProperty("died")]
		public bool Died { get; set; }

		[JsonProperty("deaths")]
		public int Deaths { get; set; }

		[JsonProperty("masochist")]
		public bool Masochist { get; set; }

		[JsonProperty("closedPassivesTooltip")]
		public bool ClosedPassivesTooltip { get; set; }

		[JsonProperty("closedSkillsTooltip")]
		public bool ClosedSkillsTooltip { get; set; }

		[JsonProperty("closedMonolithTooltip")]
		public bool ClosedMonolithTooltip { get; set; }

		[JsonProperty("closedIdolsTooltip")]
		public bool ClosedIdolsTooltip { get; set; }

		[JsonProperty("closedMinSkillLevelTutorial")]
		public bool ClosedMinSkillLevelTutorial { get; set; }

		[JsonProperty("characterClass")]
		public int CharacterClass { get; set; }

		[JsonProperty("savedItems")]
		public List<SavedItem> SavedItems { get; set; }

		[JsonProperty("savedCharacterTree")]
		public SavedCharacterTree SavedCharacterTree { get; set; }

		[JsonProperty("savedSkillTrees")]
		public List<SavedSkillTree> SavedSkillTrees { get; set; }

		[JsonProperty("characterTreeNodeProgression")]
		public List<object> CharacterTreeNodeProgression { get; set; }

		[JsonProperty("unlockedWaypointScenes")]
		public List<string> UnlockedWaypointScenes { get; set; }

		[JsonProperty("openedOneShotCaches")]
		public List<int> OpenedOneShotCaches { get; set; }

		[JsonProperty("abilityBar")]
		public List<string> AbilityBar { get; set; }

		[JsonProperty("moveButtonBehaviour")]
		public int MoveButtonBehaviour { get; set; }

		[JsonProperty("werebearAbilityBar")]
		public List<object> WerebearAbilityBar { get; set; }

		[JsonProperty("sprigganFormAbilityBar")]
		public List<object> SprigganFormAbilityBar { get; set; }

		[JsonProperty("swarmbladeAbilityBar")]
		public List<object> SwarmbladeAbilityBar { get; set; }

		[JsonProperty("portalUnlocked")]
		public bool PortalUnlocked { get; set; }

		[JsonProperty("reachedTown")]
		public bool ReachedTown { get; set; }

		[JsonProperty("soloChallenge")]
		public bool SoloChallenge { get; set; }

		[JsonProperty("respecs")]
		public int Respecs { get; set; }

		[JsonProperty("uniquesPickedUp")]
		public int UniquesPickedUp { get; set; }

		[JsonProperty("savedQuests")]
		public List<SavedQuest> SavedQuests { get; set; }

		[JsonProperty("savedMonolithQuests")]
		public List<SavedMonolithQuest> SavedMonolithQuests { get; set; }

		[JsonProperty("maxWave")]
		public int MaxWave { get; set; }

		[JsonProperty("competitiveCharacterVersion")]
		public int CompetitiveCharacterVersion { get; set; }

		[JsonProperty("oneTimeEvents")]
		public List<string> OneTimeEvents { get; set; }

		[JsonProperty("focusedQuest")]
		public int FocusedQuest { get; set; }

		[JsonProperty("sceneProgresses")]
		public List<SceneProgress> SceneProgresses { get; set; }

		[JsonProperty("monolithRuns")]
		public List<MonolithRun> MonolithRuns { get; set; }

		[JsonProperty("timelineCompletion")]
		public List<TimelineCompletion> TimelineCompletion { get; set; }

		[JsonProperty("timelineDifficultyCompletion")]
		public List<TimelineDifficultyCompletion> TimelineDifficultyCompletion { get; set; }

		[JsonProperty("timelineDifficultyUnlocks")]
		public List<TimelineDifficultyUnlock> TimelineDifficultyUnlocks { get; set; }

		[JsonProperty("currentMonolithRunTimelineID")]
		public int CurrentMonolithRunTimelineID { get; set; }

		[JsonProperty("currentMonolithRunDifficultyIndex")]
		public int CurrentMonolithRunDifficultyIndex { get; set; }

		[JsonProperty("currentMonolithRunToSetBack")]
		public int CurrentMonolithRunToSetBack { get; set; }

		[JsonProperty("currentMonolithRunToSetBackDifficultyIndex")]
		public int CurrentMonolithRunToSetBackDifficultyIndex { get; set; }

		[JsonProperty("previousMonolithEchoTimelineID")]
		public int PreviousMonolithEchoTimelineID { get; set; }

		[JsonProperty("previousMonolithEchoDifficultyIndex")]
		public int PreviousMonolithEchoDifficultyIndex { get; set; }

		[JsonProperty("monolithEchoesConquered")]
		public int MonolithEchoesConquered { get; set; }

		[JsonProperty("monolithTimelinesConquered")]
		public int MonolithTimelinesConquered { get; set; }

		[JsonProperty("monolithOptions")]
		public List<object> MonolithOptions { get; set; }

		[JsonProperty("activeMonolithMods")]
		public List<object> ActiveMonolithMods { get; set; }

		[JsonProperty("monolithDepth")]
		public int MonolithDepth { get; set; }

		[JsonProperty("hasRerolledMonolithOptions")]
		public bool HasRerolledMonolithOptions { get; set; }

		[JsonProperty("chosenMastery")]
		public int ChosenMastery { get; set; }

		[JsonProperty("clickedUnlockMasteriesButton")]
		public bool ClickedUnlockMasteriesButton { get; set; }

		[JsonProperty("currentArenaRunWaves")]
		public int CurrentArenaRunWaves { get; set; }

		[JsonProperty("previousArenaRunWaves")]
		public int PreviousArenaRunWaves { get; set; }

		[JsonProperty("arenaTiersCompleted")]
		public List<object> ArenaTiersCompleted { get; set; }

		[JsonProperty("blessingsDiscovered")]
		public List<int> BlessingsDiscovered { get; set; }

		[JsonProperty("dungeonCompletion")]
		public List<object> DungeonCompletion { get; set; }

		[JsonProperty("lanternLuminance")]
		public double LanternLuminance { get; set; }

		[JsonProperty("soulEmbers")]
		public int SoulEmbers { get; set; }

		[JsonProperty("_ts")]
		public int Ts { get; set; }

		[JsonProperty("lastPlayed")]
		public DateTime LastPlayed { get; set; }

		[JsonProperty("cycle")]
		public int Cycle { get; set; }

		[JsonProperty("id")]
		public string Id { get; set; }

		[JsonProperty("seqNo")]
		public int SeqNo { get; set; }

		[JsonProperty("version")]
		public int Version { get; set; }

		[JsonIgnore]
		public ClassInfo ClassInfo => ClassInfo.Parse(CharacterClass, ChosenMastery);
	}
}
