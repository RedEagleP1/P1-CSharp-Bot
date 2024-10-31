using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bot.SlashCommands.DbUtils
{
	/// <summary>
	/// This utils class contains methods that retrieve a team settings value for the specified organization or legion.
	/// </summary>
	public static class TeamSettingsUtils
	{
		/// <summary>
		/// Gets the team settings record that applies to the specified guild.
		/// </summary>
		/// <param name="guildId">The Id of the guild to check.</param>
		/// <param name="context">The database context.</param>
		/// <returns>The TeamSettings record that applies to the specified guild, or null if that guild was not found.</returns>
		public static TeamSettings? GetTeamSettingsForGuild(ulong guildId, ApplicationDbContext context)
		{
			// Find the guild.
			Guild guild = context.Guilds.First(o => o.Id == guildId);
			if (guild == null)
			{
				Console.WriteLine($"ERROR: Could not find a guild with Id {guildId}.");
				return null;
			}

			// Find the team settings record.
			TeamSettings teamSettings = context.TeamSettings.First(t => t.GuildId == guildId);
			if (teamSettings == null)
			{
				Console.WriteLine($"ERROR: Could not find a team settings record for the \"{guild.Name}\" guild.  Creating a new team settings record.  GuildId={guildId}");
				teamSettings = TeamSettings.CreateDefault(guildId);
			}


			return teamSettings;
		}

		/// <summary>
		/// Gets the team settings record that applies to the specified legion.
		/// </summary>
		/// <param name="legionId">The Id of the legion to check.</param>
		/// <param name="context">The database context.</param>
		/// <returns>The TeamSettings record that applies to the specified legion, or null if that legion was not found.</returns>
		public static TeamSettings? GetTeamSettingsForLegion(ulong legionId, ApplicationDbContext context)
		{
			// Find the organization.
			Legion legion = context.Legions.First(o => o.Id == legionId);
			if (legion == null)
			{
				Console.WriteLine($"ERROR: Could not find a legion with Id {legionId}.");
				return null;
			}

			// Find the TeamSettings record.
			TeamSettings teamSettings = context.TeamSettings.First(t => t.GuildId == legion.GuildId);
			if (teamSettings == null)
			{
				Console.WriteLine($"ERROR: Could not find a team settings record for the \"{legion.Name}\" legion.  Creating a new team settings record.  GuildId={legion.GuildId}");
				teamSettings = TeamSettings.CreateDefault(legion.GuildId);
			}


			return teamSettings;
		}

		/// <summary>
		/// Gets the team settings record that applies to the specified organization.
		/// </summary>
		/// <param name="orgId">The Id of the organization to check.</param>
		/// <param name="context">The database context.</param>
		/// <returns>The TeamSettings record that applies to the specified organization, or null if that organization was not found.</returns>
		public static TeamSettings? GetTeamSettingsForOrg(ulong orgId, ApplicationDbContext context)
		{
			// Find the organization.
			Organization org = context.Organizations.First(o => o.Id == orgId);
			if (org == null)
			{
				Console.WriteLine($"ERROR: Could not find an organization with Id {orgId}.");
				return null;
			}

			// Find the TeamSettings record.
			TeamSettings teamSettings = context.TeamSettings.First(t => t.GuildId == org.GuildId);
			if (teamSettings == null)
			{
				Console.WriteLine($"ERROR: Could not find a team settings record for the \"{org.Name}\" organization.  Creating a new team settings record.  GuildId={org.GuildId}");
				teamSettings = TeamSettings.CreateDefault(org.GuildId);
			}


			return teamSettings;
		}

	}
}
