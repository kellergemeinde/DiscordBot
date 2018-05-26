using DSharpPlus.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TheUltimateBot.Data
{
    public interface IDataConnector
    {
        List<DiscordMember> GetActive();

        List<DiscordMember> GetInactive();

        void SetActive(DiscordMember member);

        void SetInactive(DiscordMember member);

        void Remove(DiscordMember member);

        List<DiscordMember> AddRoleAsActive(DiscordGuild guild, DiscordRole role);
    }
}