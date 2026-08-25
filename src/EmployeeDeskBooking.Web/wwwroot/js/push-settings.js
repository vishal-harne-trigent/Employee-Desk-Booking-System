(function () {
    const unsupportedMessage = document.getElementById('push-unsupported-message');
    const supportedControls = document.getElementById('push-supported-controls');
    const enableButton = document.getElementById('enable-push-btn');
    const enableForm = document.getElementById('enable-push-form');
    const subscriptionInput = document.getElementById('subscription-json');
    const permissionDenied = document.getElementById('push-permission-denied');
    const permissionHelp = document.getElementById('push-permission-help');

    function showMessage(message) {
        if (permissionDenied) {
            permissionDenied.hidden = false;
            permissionDenied.textContent = message;
        }
    }

    function setDeniedState() {
        if (enableButton) {
            enableButton.disabled = true;
        }
        if (permissionHelp) {
            permissionHelp.hidden = false;
        }
        showMessage('Browser permission was denied. Reset notifications for this site in your browser settings, reload this page, then click Enable push again.');
    }

    if (!window.isSecureContext) {
        if (supportedControls) {
            supportedControls.hidden = true;
        }
        if (unsupportedMessage) {
            unsupportedMessage.textContent = 'Push notifications require HTTPS or localhost. Email alerts will still be sent.';
        }
        return;
    }

    if (!('serviceWorker' in navigator) || !('PushManager' in window) || !('Notification' in window)) {
        if (supportedControls) {
            supportedControls.hidden = true;
        }
        if (unsupportedMessage) {
            unsupportedMessage.textContent = 'This browser does not support push notifications. Email alerts will still be sent.';
        }
        return;
    }

    if (!enableButton || !enableForm || !subscriptionInput) {
        return;
    }

    if (Notification.permission === 'denied') {
        setDeniedState();
    }

    enableButton.addEventListener('click', async function () {
        enableButton.disabled = true;

        try {
            let permission = Notification.permission;
            if (permission === 'default') {
                permission = await Notification.requestPermission();
            }

            if (permission !== 'granted') {
                setDeniedState();
                return;
            }

            const publicKey = window.deskBookingPush?.vapidPublicKey;
            if (!publicKey) {
                showMessage('Push is not configured on the server. Email alerts will still be sent.');
                enableButton.disabled = false;
                return;
            }

            const registration = await navigator.serviceWorker.register('/sw.js');
            await navigator.serviceWorker.ready;
            const subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey),
            });

            subscriptionInput.value = JSON.stringify(subscription.toJSON());
            enableForm.submit();
        } catch (error) {
            console.error('Enable push failed', error);
            if (error instanceof DOMException && error.name === 'SecurityError') {
                showMessage('Could not register the service worker because of an SSL certificate error. Use http://localhost:5198 for local development, or run: dotnet dev-certs https --trust');
            } else {
                showMessage('Could not enable push notifications. Allow notifications for this site in your browser settings, then try again.');
            }
            if (permissionHelp) {
                permissionHelp.hidden = false;
            }
            enableButton.disabled = false;
        }
    });

    function urlBase64ToUint8Array(base64String) {
        const padding = '='.repeat((4 - (base64String.length % 4)) % 4);
        const base64 = (base64String + padding).replace(/-/g, '+').replace(/_/g, '/');
        const rawData = window.atob(base64);
        const outputArray = new Uint8Array(rawData.length);
        for (let i = 0; i < rawData.length; ++i) {
            outputArray[i] = rawData.charCodeAt(i);
        }
        return outputArray;
    }
})();
