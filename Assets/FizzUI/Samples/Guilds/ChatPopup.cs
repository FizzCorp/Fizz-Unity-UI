using System.Collections.Generic;
using Fizz.UI;
using Fizz.UI.Model;
using UnityEngine;

public class ChatPopup : MonoBehaviour
{
    [SerializeField] FizzChatView chatView;

    public void Show(List<FizzChannelMeta> channels)
    {
        gameObject.SetActive(true);

        for (int index = 0; index < channels.Count; index++)
        {
            FizzChannelMeta channelMeta = channels[index];
            if (channelMeta == null)
                continue;

            chatView.AddChannel(channelMeta);

            if (index == 0)
                chatView.SetChannel(channelMeta);
        }
    }

    public void Hide()
    {
        chatView.Reset();
        gameObject.SetActive(false);
    }
}
