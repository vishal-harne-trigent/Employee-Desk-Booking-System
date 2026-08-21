(function () {
    const unsupportedMessage = document.getElementById('push-unsupported-message');
    const supportedControls = document.getElementById('push-supported-controls');
    const enableButton = document.getElementById('enable-push-btn');
    const enableForm = document.getElementById('enable-push-form');
    const subscriptionInput = document.getElementById('subscription-json');
    const permissionDenied = document.getElementById('push-permission-denied');

    if (!('serviceWorker' in navigator) || !('PushManager' in window)) {
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

    enableButton.addEventListener('click', async function () {
        try {
            const permission = await Notification.requestPermission();
            if (permission !== 'granted') {
                if (permissionDenied) {
                    permissionDenied.hidden = false;
                }
                return;
            }

            const publicKey = window.deskBookingPush?.vapidPublicKey;
            if (!publicKey) {
                if (permissionDenied) {
                    permissionDenied.hidden = false;
                    permissionDenied.textContent = 'Push is not configured on the server. Email alerts will still be sent.';
                }
                return;
            }

            const registration = await navigator.serviceWorker.register('/sw.js');
            const subscription = await registration.pushManager.subscribe({
                userVisibleOnly: true,
                applicationServerKey: urlBase64ToUint8Array(publicKey),
            });

            subscriptionInput.value = JSON.stringify(subscription.toJSON());
            enableForm.submit();
        } catch {
            if (permissionDenied) {
                permissionDenied.hidden = false;
            }
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
