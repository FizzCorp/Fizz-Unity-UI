﻿using Fizz.UI.Model;
using Fizz.Common;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Fizz.Demo
{
	public class PresenceSample : MonoBehaviour
	{
		[SerializeField] private Dropdown userDropdown = null;
		[SerializeField] private Button OpenButton = null;
		[SerializeField] private Button CloseButton = null;

		[SerializeField] private Text[] userLabels = null;
		[SerializeField] private Image[] statusImages = null;

		[SerializeField] private RectTransform usersContainer = null;

		string userId;

		void Start()
		{
			userId = "presence-user-01";

			ResetUsers();
		}

		void OnEnable()
		{
			userDropdown.onValueChanged.AddListener(HandleDropdownChange);
			OpenButton.onClick.AddListener(HandleOpenButton);
			CloseButton.onClick.AddListener(HandleCloseButton);

			FizzService.Instance.OnConnected += OnConnected;
			FizzService.Instance.OnDisconnected += OnDisconnected;
			FizzService.Instance.UserRepository.OnUserUpdated += OnUserUpdate;
		}

		void OnDisable()
		{
			userDropdown.onValueChanged.RemoveListener(HandleDropdownChange);
			OpenButton.onClick.RemoveListener(HandleOpenButton);
			CloseButton.onClick.RemoveListener(HandleCloseButton);

			FizzService.Instance.OnConnected -= OnConnected;
			FizzService.Instance.OnDisconnected -= OnDisconnected;
			FizzService.Instance.UserRepository.OnUserUpdated -= OnUserUpdate;
		}

		void OnDestroy()
		{
			FizzService.Instance.Close();
		}

		void HandleOpenButton()
		{
			FizzService.Instance.Open(userId, userId, FizzLanguageCodes.English, FizzServices.All, false, isDone =>
		   {
			   if (isDone)
			   {
				   FizzLogger.D("HandleOpenButton " + isDone);
			   }
		   });
		}

		void HandleCloseButton()
		{
			FizzService.Instance.Close();

			ResetUsers();
		}

		void HandleDropdownChange(int index)
		{
			userDropdown.interactable = false;

			userLabels[index].gameObject.SetActive(false);
			statusImages[index].gameObject.SetActive(false);

			usersContainer.gameObject.SetActive(true);

			userId = userDropdown.captionText.text;
		}

		void OnConnected(bool syncReq)
		{
			OpenButton.interactable = false;
			CloseButton.interactable = true;

			if (syncReq)
			{
				GetAndSubscribeUser();
			}
		}

		void OnDisconnected(FizzException ex)
		{
			OpenButton.interactable = true;
			CloseButton.interactable = false;
		}

		void GetAndSubscribeUser()
		{
			foreach (Dropdown.OptionData data in userDropdown.options)
			{
				FizzService.Instance.UserRepository.GetUser(data.text, (user, ex) =>
				{
					user.Subscribe(null);
					SetPresenceStatus(user.Id, user.Online);
				});
			}
		}

		void ResetUsers()
		{
			for (int i = 0; i < userDropdown.options.Count; i++)
			{
				userLabels[i].text = userDropdown.options[i].text;
				statusImages[i].color = Color.white;
			}
		}

		void OnUserUpdate(FizzUserModel user)
		{
			SetPresenceStatus(user.Id, user.Online);
		}

		private void SetPresenceStatus(string userId, bool online)
		{
			Color color = (online) ? Color.green : Color.white;
			if (userId.EndsWith("01"))
			{
				statusImages[0].color = color;
			}
			else if (userId.EndsWith("02"))
			{
				statusImages[1].color = color;
			}
			else if (userId.EndsWith("03"))
			{
				statusImages[2].color = color;
			}
			else if (userId.EndsWith("04"))
			{
				statusImages[3].color = color;
			}
			else if (userId.EndsWith("05"))
			{
				statusImages[4].color = color;
			}
			else if (userId.EndsWith("06"))
			{
				statusImages[5].color = color;
			}
		}

		public void HandleClose()
		{
			SceneManager.LoadScene("SceneSelector");
		}
	}
}
