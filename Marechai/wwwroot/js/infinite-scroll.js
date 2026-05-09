/******************************************************************************
// MARECHAI: Master repository of computing history artifacts information
// ----------------------------------------------------------------------------
//
// Tiny IntersectionObserver wrapper so a Blazor page can be notified when a
// sentinel element scrolls into view. Used to trigger the next page-fetch
// in the Software search infinite scroll.
//
// Copyright © 2003-2026 Natalia Portillo
*******************************************************************************/

window.marechaiInfiniteScroll = (function () {
    const observers = {};

    return {
        observe: function (sentinelId, dotnetRef, rootSelector, rootMargin) {
            if (observers[sentinelId]) {
                return;
            }

            const target = document.getElementById(sentinelId);
            if (!target) {
                // Sentinel not yet in the DOM — try once more on the next animation frame.
                requestAnimationFrame(function () {
                    window.marechaiInfiniteScroll.observe(sentinelId, dotnetRef, rootSelector, rootMargin);
                });
                return;
            }

            const root = rootSelector ? document.querySelector(rootSelector) : null;

            const observer = new IntersectionObserver(
                function (entries) {
                    if (entries.some(function (e) { return e.isIntersecting; })) {
                        dotnetRef.invokeMethodAsync('OnSentinelVisibleAsync');
                    }
                },
                {
                    root: root,
                    rootMargin: rootMargin || '200px',
                    threshold: 0
                }
            );

            observer.observe(target);
            observers[sentinelId] = observer;
        },

        unobserve: function (sentinelId) {
            const observer = observers[sentinelId];
            if (observer) {
                observer.disconnect();
                delete observers[sentinelId];
            }
        }
    };
})();
