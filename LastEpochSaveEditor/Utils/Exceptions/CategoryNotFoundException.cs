namespace LastEpochSaveEditor.Utils.Exceptions
{
	internal class CategoryNotFoundException : Exception
    {
		public CategoryNotFoundException(string category, string subCategory)
			: base($"Category '${category}' with sub-category '${subCategory}' and item type not found") { }

		public CategoryNotFoundException(string category, string subCategory, params int[] subTypeIds)
            : base($"Category '${category}' with sub-category '${subCategory}' and item type '${string.Join(", ", subTypeIds)}' not found") { }
    }
}
