# Full Screen
Full screen sample is designed to demonstrate the usage of `FizzChatView` to create a full screen view. 

## Configurations
Add `FizzChatView` to a node stretching to cover full screen and do the following configuration to its instance.

    // Show channel list
    chatView.ShowChannelsButton = true;
	
	// Show header view
	chatView.ShowHeaderView = true;

	// Show close button
    chatView.ShowCloseButton = true;

    // Show chat input 
    chatView.ShowInput = true;

	// Use close button call back to disable chatview
    chatView.onClose.AddListener (() => gameObject.SetActive (false));
