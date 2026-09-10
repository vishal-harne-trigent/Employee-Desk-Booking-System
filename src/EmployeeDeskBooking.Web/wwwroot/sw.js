self.addEventListener('push', function (event) {
    if (!event.data) {
        return;
    }

    var payload = event.data.json();
    event.waitUntil(self.registration.showNotification(payload.title, {
        body: payload.body,
    }));
});
