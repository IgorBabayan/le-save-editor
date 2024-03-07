namespace LastEpochSaveEditor.Extensions;

internal static class ObjectExtensions
{
	public static string GetName(this object self)
	{
		if (self == null)
			throw new ArgumentNullException(nameof(self));

		return self.GetType() switch
		{
			{ } type when type == typeof(SubItem) => !string.IsNullOrWhiteSpace(((SubItem)self).Name)
				? ((SubItem)self).Name!.GetItemName()
				: ((SubItem)self).DisplayName!.GetItemName(),
			{ } type when type == typeof(Unique) => !string.IsNullOrWhiteSpace(((Unique)self).Name)
				? ((Unique)self).Name!.GetItemName()
				: ((Unique)self).DisplayName!.GetItemName(),
			{ } type when type == typeof(WeaponType<SubItem>) => !string.IsNullOrWhiteSpace(((WeaponType<SubItem>)self).Entity.Name)
				? ((WeaponType<SubItem>)self).Entity.Name!.GetItemName()
				: ((WeaponType<SubItem>)self).Entity.DisplayName!.GetItemName(),
			{ } type when type == typeof(WeaponType<Unique>) => !string.IsNullOrWhiteSpace(((WeaponType<Unique>)self).Entity.Name)
				? ((WeaponType<Unique>)self).Entity.Name!.GetItemName()
				: ((WeaponType<Unique>)self).Entity.DisplayName!.GetItemName(),
			_ => throw new ArgumentException("Type not supported")
		};
	}

	public static string GetBaseTypeName(this object self)
	{
		if (self == null)
			throw new ArgumentNullException(nameof(self));

		return self.GetType() switch
		{
			{ } type when type == typeof(SubItem) => ((SubItem)self).Name!,
			{ } type when type == typeof(Unique) => ((Unique)self).BaseType.ToString(),
			_ => throw new ArgumentException("Type not supported")
		};
	}

	public static string GetWeaponTypePath(this object self)
	{
		if (self == null)
			throw new ArgumentNullException(nameof(self));

		return self.GetType() switch
		{
			{ } type when type == typeof(WeaponType<SubItem>) => ((WeaponType<SubItem>)self).IsTwoHanded
					? TWO_HAND_WEAPONS
					: ONE_HAND_WEAPONS,
			{ } type when type == typeof(WeaponType<Unique>) => ((WeaponType<Unique>)self).IsTwoHanded
					? TWO_HAND_WEAPONS
					: ONE_HAND_WEAPONS,
			_ => throw new InvalidOperationException("Type not supported")
		};
	}
}
