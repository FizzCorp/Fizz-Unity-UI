using System.Collections.Generic;
using Fizz;
using Fizz.Common;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GuildViewSample : MonoBehaviour
{
    [SerializeField] Text titleLabel;
    [SerializeField] ChatPopup chatPopup;

    private FizzChannelMeta globalChannel;
    private FizzChannelMeta guildChannel;

    private void Awake()
    {
        Screen.orientation = ScreenOrientation.Portrait;

        globalChannel = new FizzChannelMeta("global-channel", "Global");

        FizzService.Instance.AddChannel(globalChannel);
    }

    public void OnSelectGuild(string guild)
    {
        if (guildChannel != null)
        {
            FizzService.Instance.RemoveChannel(guildChannel);
        }

        guildChannel = new FizzChannelMeta(string.Join("-", guild, "channel"), guild);
        FizzService.Instance.AddChannel(guildChannel);

        titleLabel.text = "Joined " + guild.ToUpper();
    }

    public void OnLeaveGuild()
    {
        if (guildChannel != null)
        {
            FizzService.Instance.RemoveChannel(guildChannel);
        }

        guildChannel = null;
        titleLabel.text = "No Guild";
    }

    public void OnChat()
    {
        chatPopup.Show(new List<FizzChannelMeta>() { globalChannel, guildChannel });
    }

    private void OnDestroy()
    {
        FizzService.Instance.RemoveChannel(globalChannel);

        if (guildChannel != null)
        {
            FizzService.Instance.RemoveChannel(guildChannel);
        }
    }

    public void HandleClose()
    {
        SceneManager.LoadScene("SceneSelector");
    }
}
