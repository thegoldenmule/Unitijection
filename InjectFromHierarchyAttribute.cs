using System;

using Ninject;

namespace TheGoldenMule
{
	[AttributeUsage(
		AttributeTargets.Property | AttributeTargets.Field,
		AllowMultiple = true)]
	public class InjectFromHierarchy : InjectAttribute
	{
		public InjectFromHierarchy(string tag, string query = null)
		{
			Tag = tag;
			Query = query;
		}

		public string Tag
		{
			get;
			private set;
		}

		public string Query
		{
			get;
			private set;
		}
	}
}
