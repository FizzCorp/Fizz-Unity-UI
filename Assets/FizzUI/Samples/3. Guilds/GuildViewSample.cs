using System.Collections.Generic;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Fizz.Demo
{
    /// <summary>
    /// GuildViewSample demonstrate the use of FizzChatView in a popup where its channel list is changed with adding or removing a guild channel.
    /// </summary>
    public class GuildViewSample : MonoBehaviour
    {
        [SerializeField] private Text titleLabel = null;
        /// <summary>
        /// Popup which contains FizzChatView
        /// </summary>
        [SerializeField] private ChatPopup chatPopup = null;

        // Global Channel Id
        private readonly string globalChannelId = "global-channel";

        private readonly string localChannelId = "local-channel";

        private FizzChannelMeta guildChannel;

        // Called when any of GuildA, GuildB, GuildC or GuildD is selected.
        public void OnSelectGuild(string guild)
        {
            // Remove previous channel if any
            RemoveGuildChannel();

            guildChannel = new FizzChannelMeta(guild + "-channel", guild, "GUILD");

            // Subscribe to channel (Note that it is compalsory to Subscribe a channel before adding to UI)
            FizzService.Instance.SubscribeChannel(guildChannel);

            titleLabel.text = "Joined " + guild.ToUpper();
        }

        // Unsubscribe and remove guild channel
        public void OnLeaveGuild()
        {
            RemoveGuildChannel();

            guildChannel = null;
            titleLabel.text = "No Guild";
        }

        public void OnChat()
        {
            List<string> channelList = new List<string>
            {
                globalChannelId,
                localChannelId
            };

            // Add guild channel to  list if joined
            if (guildChannel != null)
            {
                channelList.Add(guildChannel.Id);
            }

            // Open Chat popup with the chennel list
            chatPopup.Show(channelList);
        }

        private void OnDestroy()
        {
            RemoveGuildChannel();
        }

        private void RemoveGuildChannel()
        {
            if (guildChannel != null)
            {
                // Unsubscribe channel from FizzService
                FizzService.Instance.UnsubscribeChannel(guildChannel.Id);
            }
        }

        public void HandleClose()
        {
            SceneManager.LoadScene("SceneSelector");
        }
    }
}