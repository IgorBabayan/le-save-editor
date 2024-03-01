namespace LastEpochSaveEditor.Extensions;

internal static class QualityTypeExtension
{
	public static string GetQualityFolderName(this QualityType self)
	{
		switch (self)
		{
			case QualityType.Basic:
			case QualityType.Magic:
			case QualityType.Rare:
			case QualityType.Exalted:
				return Const.BASIC_FOLDER_NAME;

			case QualityType.Unique:
			case QualityType.Legendary:
				return Const.UNIQUE_FOLDER_NAME;

			default:
				return Const.SET_FOLDER_NAME;
		}
	}
}
