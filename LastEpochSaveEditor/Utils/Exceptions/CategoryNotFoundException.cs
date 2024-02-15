using System;

namespace LastEpochSaveEditor.Utils.Exceptions
{
	internal class CategoryNotFoundException : Exception
    {
        public CategoryNotFoundException(string category, string subCategory, int index)
            : base($"Category '${category}' with sub-category '${subCategory}' and item type '${0}' not found") { }
    }
}
