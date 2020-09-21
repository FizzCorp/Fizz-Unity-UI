using System.Collections.Generic;
using Fizz.Chat;
using Fizz.Common;
using UnityEngine;
using UnityEngine.UI;

namespace Fizz.Demo
{
    /// <summary>
    /// This is the script attached to GroupInviteNode Node.
    /// It uses both accept and reject group invite funtions.
    /// </summary>
    public class GroupInviteNode : MonoBehaviour
    {
        [SerializeField] Text titleLabel;

        [SerializeField] Button acceptButton;
        [SerializeField] Button rejectButton;

        [SerializeField] RectTransform actionsNode;

        private string _groupId;

        public void SetData(FizzChannelMessage message)
        {
            if (message != null && message.Data != null)
            {
                string title = string.Empty;
                if (message.Data.TryGetValue("title", out title))
                {
                    titleLabel.text = title + " Invitation";
                }

                message.Data.TryGetValue("group-id", out _groupId);
                actionsNode.gameObject.SetActive(true);
            }
        }

        private void OnEnable()
        {
            acceptButton.onClick.AddListener(OnAcceptButtonPressed);
            rejectButton.onClick.AddListener(OnRejectButtonPressed);
        }

        private void OnDisable()
        {
            acceptButton.onClick.RemoveListener(OnAcceptButtonPressed);
            rejectButton.onClick.RemoveListener(OnRejectButtonPressed);
        }

        private void OnAcceptButtonPressed()
        {
            // Accept Group Invite
            try
            {
                FizzService.Instance.JoinGroup(_groupId, ex =>
                {
                    if (ex == null)
                    {
                        FizzLogger.D("Invite Accepted");
                    }
                });
            }
            catch
            {
                FizzLogger.E("Something went wrong while calling Join Group of FizzService.");
            }
        }

        private void OnRejectButtonPressed()
        {
            // Reject Group Invite
            try
            {
                FizzService.Instance.RemoveGroup(_groupId, ex =>
                {
                    if (ex == null)
                    {
                        FizzLogger.D("Invite Rejected");
                    }
                });
            }
            catch
            {
                FizzLogger.E("Something went wrong while calling Reject Group of FizzService.");
            }
        }

    }
}
