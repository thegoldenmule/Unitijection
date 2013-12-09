using System;

using Ninject;

namespace TheGoldenMule
{
	/// <summary>
	/// Retrieves (and creates if necessary) a MonoBehaviour or GameObject based on a path
	/// and injects it into the member when UIUtils.InjectLinkages() is called.
	/// 
	/// See https://jira.kixeye.com/wiki/display/WCM/InjectLinkage for usage.
	/// 
	/// </summary>
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